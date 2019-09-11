﻿using Assets.World.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.World
{
    // wrapper class for convenience
    internal class ExecutedCommandList : IEnumerable
    {
        // custom indexer for convenience
        internal AbstractCommand this[int id] => _executedCommands[id];

        static ExecutedCommandList _instance;
        static ExecutedCommandList Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ExecutedCommandList();

                return _instance;
            }
        }

        static readonly List<AbstractCommand> _executedCommands = new List<AbstractCommand>();
        static bool _isDirty = true; // true to force initial message broadcast

        internal static void Add(AbstractCommand command)
        {
            _executedCommands.Add(command);
            _isDirty = true;
        }

        internal static void RemoveAt(int id)
        {
            _executedCommands.RemoveAt(id);
            _isDirty = true;
        }

        internal static void RemoveCommand(AbstractCommand command) => _executedCommands.Remove(command);

        // for now it is public but maybe it should be kept in GameMap in order to provide more unified interface for external users
        internal static void UndoLastCommand()
        {
            if (_executedCommands.Count == 0)
                return;

            int lastCmdId = _executedCommands.Count - 1;

            // this will also remove it from the list as every command removes itself once undo is called successfully
            if(_executedCommands[lastCmdId].Undo())
                _isDirty = true;
        }

        /// <summary>
        /// Used for synchronization with Unity life cycle.
        /// This is necessary to call this method in any Update method.
        /// </summary>
        internal static void EndFrameSignal()
        {
            if (_isDirty)
                GameMap.BroadcastStatusChanged(UpdateCommandListText());
        }

        static string UpdateCommandListText()
        {
            var sb = new StringBuilder();
            string shortcut = "<b>" + (Application.isEditor ? "Z+X" : "Ctrl+Z") + "</b>";
            sb.AppendLine($"Press {shortcut} to undo last command");
            sb.Append(Environment.NewLine);
            sb.AppendLine("Executed Commands:");

            for (int i = _executedCommands.Count - 1; i >= 0; i--)
                sb.AppendLine("- " + _executedCommands[i]);

            return sb.ToString();
        }

        public IEnumerator GetEnumerator() => throw new NotImplementedException();
    }
}
