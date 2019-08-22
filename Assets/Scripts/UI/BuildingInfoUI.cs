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

        void OnDisable() => ResourceManager.ResourceChangedEventHandler -= ResourceUpdate; // unsubscribe from ResourceManager

        void OnEnable()
        {
            ResourceManager.ResourceChangedEventHandler += ResourceUpdate; // subscribe to ResourceManager

            _reallocateButton.gameObject.SetActive(Building.AbleToReallocate);

            _reallocateButton.onClick.RemoveAllListeners();
            _reallocateButton.onClick.AddListener(() =>
            {
                GameEngine.Instance.StartBuildingReallocation(Building);
                gameObject.SetActive(false);
                Building = null;
            });

            SetReallocateButtonInteractivity();
        }

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
        }

        public void StartProduction()
        {
            Building.StartProduction();
            _startProductionButton.interactable = false;
        }

        void ResourceUpdate(object sender, ResourceChangedEventArgs eventArgs) => SetReallocateButtonInteractivity();

        void SetReallocateButtonInteractivity()
            => _reallocateButton.interactable = Building.AbleToReallocate
                ? ResourceManager.IsEnoughResources(Building.ReallocationCost)
                : true;
    }
}