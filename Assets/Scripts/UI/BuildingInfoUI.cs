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
        [SerializeField] TextMeshProUGUI _buildingName;
        [SerializeField] Slider _slider;
        [SerializeField] Image _fill;

        void Update()
        {
            if (Building.ScheduledTask == null || (Building.Finished && !Building.ProductionStarted))
            {
                _slider.value = 0f;
                _fill.gameObject.SetActive(false);
            }
            else
            {
                _fill.gameObject.SetActive(true);
                float p = 10f;
                float t = Building.ScheduledTask.TimeLeft;
                _slider.value = 1 - t / p;
            }

            _buildingName.text = Building.Finished
                ? Building.Name
                : "Building " + Building.Name;

            _startProductionButton.interactable = Building.Finished && !Building.ProductionStarted;
        }

        public void StartProduction()
        {
            Building.StartProduction();
            _startProductionButton.interactable = false;
        }
    }
}