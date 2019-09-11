﻿using Assets.World.DataModels;
using Assets.World.Interfaces;
using Assets.World.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.World.Commands
{
    public class MoveVehicleCommand : AbstractCommand, ICommand, ICloneable<AbstractCommand>
    {
        internal Vehicle Vehicle { get; private set; }

        public readonly Vector2Int To;

        MoveTask _moveTask;

        /// <summary>
        /// Move vehicle from its current position to target position.
        /// </summary>
        internal MoveVehicleCommand(Vehicle vehicle, Vector2Int to)
        {
            Vehicle = vehicle;
            To = to;
        }

        public override bool Call()
        {
            if (_succeeded || !CheckConditions())
                return false;

            MoveTask task = new MoveTask(GameMap.Instance.Path, Vehicle);
            _moveTask = task;
            GameMap.ScheduleTask(task);

            return base.Call();
        }

        public override bool Undo()
        {
            if (!_succeeded)
                return false;

            _moveTask.Abort();

            return base.Undo();
        }

        public override bool CheckExecutionContext()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return false; // cursor is over the UI

            return true;
        }

        public override bool CheckConditions()
        {
            // is player's vehicle 
            // is selectable etc.

            // for now always true
            return true;
        }

        public override string ToString() => $"Move unit {Vehicle.Type.ToString()} from {Vehicle.Position} to {To}";

        /// <summary>
        /// Returns a shallow copy of the command.
        /// </summary>
        public override AbstractCommand Clone() => new SelectVehicleCommand(Vehicle);
    }
}
