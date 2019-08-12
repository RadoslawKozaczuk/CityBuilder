using Assets.Scripts.DataModels;
using Assets.Scripts.DataSource;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    class BuildingPanelUI : MonoBehaviour
    {
        public Transform Resources;
        public GameObject BuildingElementPrefab;
        public GameObject ResourceElementPrefab;
        public GameEngine GameEngine;

        void Start()
        {
            // initialize buildings
            foreach (BuildingData b in BuildingDataSource.Buildings)
            {
                BuildingButtonUI buttonUI = Instantiate(BuildingElementPrefab, Resources).GetComponent<BuildingButtonUI>();
                buttonUI.Title.text = b.Name;
                buttonUI.BuildingType = b.Type;
                buttonUI.GetComponent<Button>().onClick.AddListener(() => GameEngine.BuildingConstructionAction(buttonUI.BuildingType));

                foreach (Resource r in b.Cost)
                {
                    GameObject go = Instantiate(ResourceElementPrefab, buttonUI.Resources);
                    ResourceElementUI resUI = go.GetComponent<ResourceElementUI>();
                    Transform t = go.GetComponent<Transform>();

                    t.localScale = new Vector3(0.6f, 0.6f);

                    resUI.Amount.text = r.Quantity.ToString();
                    resUI.Image.sprite = ResourceManager.Instance.ResourceIcons[(int)r.ResourceType];
                }

                GameObject go1 = Instantiate(ResourceElementPrefab, buttonUI.Resources);
                ResourceElementUI resUI1 = go1.GetComponent<ResourceElementUI>();
                Transform t1 = go1.GetComponent<Transform>();

                t1.localScale = new Vector3(0.6f, 0.6f);

                resUI1.Amount.text = "123";
                resUI1.Image.sprite = ResourceManager.Instance.ResourceIcons[0];
            }
        }
    }
}
