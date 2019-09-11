using Assets.World;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    class CommandListUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _commandListText;

        // subscribe to ResourceManager
        void Start() => GameMap.StatusChangedEventHandler += ResourceUpdate;

        void ResourceUpdate(object _, ExecutedCommandListChangedEventArgs eventArgs) 
            => _commandListText.text = eventArgs.CommandListText;
    }
}
