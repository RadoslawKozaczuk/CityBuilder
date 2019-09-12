using Assets.World.Tasks;
using System.Collections.Generic;

namespace Assets.World
{
    static class TaskManager
    {
        static readonly List<AbstractTask> _taskBuffer = new List<AbstractTask>();
        static readonly List<AbstractTask> _scheduledTasks = new List<AbstractTask>();

#if UNITY_EDITOR
        static int _lastFrame = int.MinValue; // safety mechanism
#endif

        /// <summary>
        /// Should not be called more than once per frame.
        /// </summary>
        static internal void Update()
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
        }

        static internal void ScheduleTask(AbstractTask task) => _taskBuffer.Add(task);
    }
}
