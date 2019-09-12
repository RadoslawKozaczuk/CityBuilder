using Assets.World.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.World
{
    static class TaskManager
    {
        static readonly List<AbstractTask> _taskBuffer = new List<AbstractTask>();
        static readonly List<AbstractTask> _scheduledTasks = new List<AbstractTask>();
        static readonly StringBuilder _sb = new StringBuilder(); // for debug log only

#if UNITY_EDITOR
        static int _lastFrame = int.MinValue; // safety mechanism
#endif

        /// <summary>
        /// Should not be called more than once per frame.
        /// </summary>
        static internal void UpdateTasks()
        {
#if UNITY_EDITOR
            if (_lastFrame == UnityEngine.Time.frameCount)
                throw new System.Exception("TaskManager's Update method was called more than once per frame.");
            _lastFrame = UnityEngine.Time.frameCount;
#endif

            for (int i = 0; i < _scheduledTasks.Count; i++)
            {
                AbstractTask task = _scheduledTasks[i];
                task.Update();

                if (task.Completed)
                    _scheduledTasks.RemoveAt(i--);
                //task.TimeLeft -= Time.deltaTime;

                //if (task.TimeLeft > 0)
                //    return;

                //task.ActionOnFinish();
            }

            _scheduledTasks.AddRange(_taskBuffer);
            _taskBuffer.Clear();

            GameMap.BroadcastTaskManagerStatusChanged(CurrentStatus());
        }

        static internal void ScheduleTask(AbstractTask task) => _taskBuffer.Add(task);

        static internal List<AbstractTask> FindAll(Type type)
        {
#if UNITY_EDITOR
            if (type.IsSubclassOf(typeof(AbstractTask)))
                throw new System.ArgumentException("Only types derived from AbstractTask are allowed.", "type");
#endif

            return _scheduledTasks.FindAll(task => task.GetType() == type);
        }

        static internal int GetFirstFreeTaskId() => _scheduledTasks.Count > 0 ? _scheduledTasks.Max(task => task.Id) + 1 : 0;

        static string CurrentStatus()
        {
            _sb.Clear();
            foreach(AbstractTask task in _scheduledTasks)
            {
                _sb.AppendLine(task.ToString());

                if(task.WaitingFor != null)
                    _sb.Append(" waiting for -> " + task.WaitingFor.GetType() + " id:" + task.Id);
            }

            return _sb.ToString();
        }
    }
}
