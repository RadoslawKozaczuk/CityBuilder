using Assets.World;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    class CommandListUI : MonoBehaviour
    {
        public TextMeshProUGUI CommandListText;

        void Start()
        {
            // subscribe to ResourceManager
            ExecutedCommandList.StatusChangedEventHandler += ResourceUpdate;
        }

        void ResourceUpdate(object sender, ExecutedCommandListChangedEventArgs eventArgs)
        {
            CommandListText.text = eventArgs.CommandListText;
        }
    }
}
