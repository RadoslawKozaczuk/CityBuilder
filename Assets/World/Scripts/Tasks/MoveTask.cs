using Assets.World.DataModels;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.World.Tasks
{
    internal sealed class MoveTask : AbstractTask
    {
        internal readonly List<Vector2Int> Path;
        internal Vehicle Vehicle;

        Vector3 _startPos;
        Vector3 _endPos;
        readonly float _totalTime; // in seconds
        float _currentTime; // in seconds
        bool _notMovedYet = true;

        internal MoveTask(List<Vector2Int> path, Vehicle vehicle)
        {
#if UNITY_EDITOR
            if (path == null)
                throw new System.ArgumentNullException("path", "path argument cannot be null");
            else if (path.Count < 2)
                throw new System.ArgumentException("path argument must be of length of at least 2", "path");
#endif
            
            // we need to check if this vehicle does not have a move task already assigned


            Path = new List<Vector2Int>(path);
            Vehicle = vehicle;

            _totalTime = GameMap.CELL_SIZE / Vehicle.Speed; // how long in seconds it takes to move from one cell to another
            _currentTime = 0;

            _startPos = GameMap.GetCellMiddlePosition(path[0]);
            _endPos = GameMap.GetCellMiddlePosition(path[1]);
        }

        internal override void Update()
        {
            if (Completed)
                return;

            _currentTime += Time.deltaTime;

            if(_currentTime > _totalTime)
            {
                if (!_aborted && Path.Count > 2)
                {
                    _currentTime = 0;
                    Path.RemoveAt(0);
                    _startPos = GameMap.GetCellMiddlePosition(Path[0]);
                    _endPos = GameMap.GetCellMiddlePosition(Path[1]);
                    _notMovedYet = true;
                }
                else
                {
                    Completed = true;
                    Vehicle.transform.position = _endPos;
                    return;
                }
            }

            // to use SIMD
            float2 offset = new float2(_endPos.x - _startPos.x, _endPos.z - _startPos.z);
            // offset will be always +/- CELL_SIZE and 0

            float proportion = _currentTime / _totalTime;
            if(_notMovedYet && proportion > 0.5f)
            {
                // move occupied
                GameMap.MoveVehicle(Vehicle, Path[1]);
                _notMovedYet = false;
            }

            offset *= proportion;

            Vehicle.transform.position = new Vector3(_startPos.x + offset.x, _startPos.y, _startPos.z + offset.y);
        }

        internal override void Abort() => _aborted = true;
    }
}
