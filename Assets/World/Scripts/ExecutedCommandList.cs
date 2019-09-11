using Assets.World.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.World
{
    public sealed class ExecutedCommandListChangedEventArgs
    {
        // for now it will hold everything in one formatted string
        public string CommandListText;
    }

    // wrapper class for convenience
    public class ExecutedCommandList : IEnumerable
    {
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

        // custom indexer for convenience
        internal AbstractCommand this[int id] => _executedCommands[id];

        /// <summary>
        /// Subscribe to this event to receive notifications each time executed command list status has changed.
        /// </summary>
        public static event EventHandler<ExecutedCommandListChangedEventArgs> StatusChangedEventHandler;

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

        // for now it is public but maybe it should be kept in GameMap in order to provide more unified interface for external users
        public static void UndoLastCommand()
        {
            if (_executedCommands.Count == 0)
                return;

            int lastCmdId = _executedCommands.Count - 1;

            // this will also remove it from the list as every command removes itself once undo is called successfully
            _executedCommands[lastCmdId].Undo();
            _executedCommands.RemoveAt(lastCmdId);

            _isDirty = true;
        }

        /// <summary>
        /// Used for synchronization with Unity life cycle.
        /// This is necessary to call this method in any Update method.
        /// </summary>
        internal static void EndFrameSignal()
        {
            if (_isDirty)
                BroadcastStatusChanged();
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

        // we call the event - if there is no subscribers we will get a null exception error therefore we use a safe call (null check)
        static void BroadcastStatusChanged() => StatusChangedEventHandler?.Invoke(
            _instance,
            new ExecutedCommandListChangedEventArgs { CommandListText = UpdateCommandListText() });
    }
}
