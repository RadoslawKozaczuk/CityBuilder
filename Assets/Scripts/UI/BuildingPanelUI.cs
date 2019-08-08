using Assets.Scripts.DataModels;
using Assets.Scripts.DataSource;
using UnityEngine;

namespace Assets.Scripts.UI
{
    class BuildingPanelUI : MonoBehaviour
    {
        public Transform Resources;
        public GameObject BuildingElementPrefab;
        public GameObject ResourceElementPrefab;

        void Start()
        {
            // initialize buildings
            foreach (BuildingData b in BuildingDataSource.Buildings)
            {
                BuildingButtonUI buttonUI = Instantiate(BuildingElementPrefab, Resources).GetComponent<BuildingButtonUI>();
                buttonUI.Title.text = b.Name;

                foreach(Resource r in b.Cost)
                {
                    GameObject go = Instantiate(ResourceElementPrefab, buttonUI.Resources);
                    ResourceElementUI resUI = go.GetComponent<ResourceElementUI>();
                    Transform t = go.GetComponent<Transform>();
                    //t.localScale = new Vector3(0.5f, 0.5f);

                    resUI.Amount.text = r.Quantity.ToString();
                    resUI.Image.sprite = ResourceManager.Instance.ResourceIcons[(int)r.ResourceType];
                }
            }
        }
    }
}
