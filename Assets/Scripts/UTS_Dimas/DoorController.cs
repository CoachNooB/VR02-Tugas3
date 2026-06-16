using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Vector3 closedPosition;
    public Vector3 openOffset = new Vector3(0, 0, 2f);
    private bool isOpen = false;

    private void Start()
    {
        closedPosition = transform.position;
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            transform.position = closedPosition + openOffset;
            isOpen = true;
        }
    }
}