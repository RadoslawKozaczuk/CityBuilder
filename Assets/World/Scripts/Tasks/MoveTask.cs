using Assets.World.DataModels;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.World.Tasks
{
    internal sealed class MoveTask : AbstractTask
    {
        internal List<Vector2Int> Path;
        internal Vehicle Vehicle;

        Vector2Int _from; // from may be changed if the task is waiting for another one
        readonly Vector2Int _to;

        Vector3 _startWorldPos;
        Vector3 _endWorldPos;
        readonly float _totalTime; // in seconds
        float _currentTime; // in seconds
        bool _notMovedYet = true;

        internal MoveTask(Vector2Int from, Vector2Int to, List<Vector2Int> path, Vehicle vehicle) : base()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (path == null)
                throw new System.ArgumentNullException("path", "path argument cannot be null");
            else if (path.Count < 2)
                throw new System.ArgumentException("path argument must be of length of at least 2", "path");
#endif

            _from = from;
            _to = to;

            // we need to check if this vehicle does not have a move task already assigned
            if(vehicle.ScheduledTask != null)
            {
                vehicle.ScheduledTask.Abort();
                Status = TaskStatus.Pending;
                WaitingFor = vehicle.ScheduledTask; // this task is waiting for another task before it can start
            }

            Path = new List<Vector2Int>(path);
            Vehicle = vehicle;

            _totalTime = GameMap.CELL_SIZE / Vehicle.Speed; // how long in seconds it takes to move from one cell to another
            _currentTime = 0;

            _startWorldPos = GameMap.GetCellMiddlePosition(path[0]);
            _endWorldPos = GameMap.GetCellMiddlePosition(path[1]);

            vehicle.ScheduledTask = this;
        }

        internal override void Update()
        {
            if (Status == TaskStatus.Completed)
                return;

            if (WaitingFor != null)
            {
                if (WaitingFor.Status == TaskStatus.Completed)
                {
                    WaitingFor = null;
                    Status = TaskStatus.Ongoing;
                    // przekalkulowac droge
                    _from = Vehicle.Position;
                    Path = GameMap.Instance.PathFinder.FindPath(_from, _to);
                    _startWorldPos = GameMap.GetCellMiddlePosition(Path[0]);
                    _endWorldPos = GameMap.GetCellMiddlePosition(Path[1]);
                    return;
                }

                return;
            }

            _currentTime += Time.deltaTime;

            if(_currentTime > _totalTime)
            {
                if (!_aborted && Path.Count > 2)
                {
                    _currentTime = 0;
                    Path.RemoveAt(0);
                    _startWorldPos = GameMap.GetCellMiddlePosition(Path[0]);
                    _endWorldPos = GameMap.GetCellMiddlePosition(Path[1]);
                    _notMovedYet = true;
                }
                else
                {
                    Status = TaskStatus.Completed;
                    Vehicle.transform.position = _endWorldPos;
                    return;
                }
            }

            // to use SIMD
            float2 offset = new float2(_endWorldPos.x - _startWorldPos.x, _endWorldPos.z - _startWorldPos.z);
            // offset will be always +/- CELL_SIZE and 0

            float proportion = _currentTime / _totalTime;
            if(_notMovedYet && proportion > 0.5f)
            {
                // move occupied
                GameMap.MoveVehicle(Vehicle, Path[1]);
                _notMovedYet = false;
            }

            offset *= proportion;

            Vehicle.transform.position = new Vector3(_startWorldPos.x + offset.x, _startWorldPos.y, _startWorldPos.z + offset.y);
        }

        internal override string ToString()
        {
            return $"MoveTask ID[{Id}] from: {_from} to: {_to} time: {string.Format("{0:0.00}", _currentTime)} status: {Status}";
        }
    }
}
