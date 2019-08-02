using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class GameEngine : MonoBehaviour
    {
        void Update()
        {
            ProcessInput();
        }

        void ProcessInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Check if the mouse was clicked over a UI element
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    Debug.Log("Clicked on the UI");
                    return;
                }

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
                    return;

                if (hit.transform == null)
                    return;

                Debug.Log("Hit the " + hit.transform.gameObject.name);
            }
        }
    }
}