using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpacebarClickHandler : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Raycast dari tengah layar
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = new Vector2(Screen.width / 2, Screen.height / 2);
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                Button btn = result.gameObject.GetComponent<Button>();
                if (btn != null && btn.interactable)
                {
                    btn.onClick.Invoke();
                    Debug.Log("Spacebar clicked: " + btn.name);
                    break;
                }
            }
        }
    }
}