using Assets.Database;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.GraphicRepresentation.UI
{
    public class BuildingButtonUI : MonoBehaviour
    {
        public TextMeshProUGUI Title;
        public Transform Resources;
        public Button BuildButton;
        [HideInInspector] public BuildingType BuildingType;
    }
}
