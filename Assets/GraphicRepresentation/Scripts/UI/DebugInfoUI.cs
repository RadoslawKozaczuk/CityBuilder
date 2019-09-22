using Assets.GameLogic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Assets.GraphicRepresentation.UI
{
    class DebugInfoUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _debugInfoText;

        readonly StringBuilder _sb = new StringBuilder();
        string _executedCommandStatus;
        string _taskManagerStatus;

        void Start()
        {
            GameMap.ExecutedCommandsStatusChangedEventHandler += CommandListUpdate;
            GameMap.TaskManagerStatusChangedEventHandler += TaskManagerUpdate;
        }

        void CommandListUpdate(object _, DebugLogEventArgs eventArgs)
        {
            _executedCommandStatus = eventArgs.Log;
            DisplayAll();
        }

        void TaskManagerUpdate(object _, DebugLogEventArgs eventArgs)
        {
            _taskManagerStatus = eventArgs.Log;
            DisplayAll();
        }

        void DisplayAll()
        {
            _sb.Clear();

            _sb.Append("<b>Press ");
            _sb.Append(Application.isEditor ? "Z+X" : "Ctrl+Z");
            _sb.Append(" to undo last command</b>");
            _sb.AppendLine("\nExecuted Commands:");
            _sb.AppendLine(_executedCommandStatus);
            _sb.AppendLine("Ongoing tasks:");
            _sb.AppendLine(_taskManagerStatus);

            _debugInfoText.text = _sb.ToString();
        }
    }
}
