using Assets.Scripts.DataModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class BuildingInfoUI : MonoBehaviour
    {
        public Building Building;

        [SerializeField] Button _startProductionButton;
        [SerializeField] Button _reallocateButton;
        [SerializeField] TextMeshProUGUI _buildingName;
        [SerializeField] Slider _slider;
        [SerializeField] Image _fill;
        [SerializeField] GameEngine _gameEngine;

        void Start() => _reallocateButton.onClick.AddListener(ReallocateBuilding);

        void Update()
        {
            if (Building.ScheduledTask == null || (Building.Constructed && !Building.ProductionStarted))
            {
                _slider.value = 0f;
                _fill.gameObject.SetActive(false);
            }
            else
            {
                _slider.value = 1 - Utils.Map(0, 1, 0, Building.ScheduledTask.TotalTime, Building.ScheduledTask.TimeLeft);
                _fill.gameObject.SetActive(true);
            }

            _buildingName.text = Building.Constructed
                ? Building.Name
                : "Building " + Building.Name;

            _startProductionButton.interactable = Building.Constructed && !Building.ProductionStarted;

            // check if player has enough resources to reallocate
            if(Building.AbleToReallocate)
                _reallocateButton.interactable = ResourceManager.Instance.IsEnoughResource(Building.ReallocationCost);
        }

        public void Initialize() => _reallocateButton.gameObject.SetActive(Building.AbleToReallocate);

        public void StartProduction()
        {
            Building.StartProduction();
            _startProductionButton.interactable = false;
        }

        void ReallocateBuilding()
        {
            _gameEngine.BuildingReallocationAction(Building);
            gameObject.SetActive(false);
        }
    }
}