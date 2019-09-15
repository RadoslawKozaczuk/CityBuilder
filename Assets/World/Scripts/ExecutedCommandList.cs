using Assets.World.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Assets.World
{
    // wrapper class for convenience
    internal class ExecutedCommandList : IEnumerable
    {
        // custom indexer for convenience
        internal AbstractCommand this[int id] => _executedCommands[id];

        static readonly List<AbstractCommand> _executedCommands = new List<AbstractCommand>();
        static readonly StringBuilder _sb = new StringBuilder();
        static bool _isDirty = true; // true to force initial message broadcast

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        static int _lastFrame = int.MinValue; // safety mechanism
#endif

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
        /// It is necessary to call this method in any Update method but not more than once per frame.
        /// </summary>
        internal static void EndFrameSignal()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_lastFrame == UnityEngine.Time.frameCount)
                throw new System.Exception("ExecutedCommandList's EndFrameSignal method was called more than once per frame.");
            _lastFrame = UnityEngine.Time.frameCount;
#endif

            if (_isDirty)
                GameMap.BroadcastExecutedCommandsStatusChanged(UpdateCommandListText());
        }

        static string UpdateCommandListText()
        {
            _sb.Clear();
            for (int i = _executedCommands.Count - 1; i >= 0; i--)
                _sb.AppendLine("- " + _executedCommands[i]);

            return _sb.ToString();
        }

        public IEnumerator GetEnumerator() => throw new NotImplementedException();
    }
}
