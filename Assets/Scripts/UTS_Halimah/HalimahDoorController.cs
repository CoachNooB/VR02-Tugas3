using UnityEngine;

public class HalimahDoorController : MonoBehaviour
{
    private Vector3 closedPosition;
    private Vector3 openPosition;
    public float openOffset = 2f;
    private bool isOpen = false;
    private float moveSpeed = 3f;

    private void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + new Vector3(0, 0, openOffset);
    }

    private void Update()
    {
        if (isOpen)
        {
            transform.position = Vector3.Lerp(transform.position, openPosition, Time.deltaTime * moveSpeed);
        }
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            Debug.Log("[HalimahDoorController] Door opened!");
        }
    }

    public void CloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            Debug.Log("[HalimahDoorController] Door closed!");
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}
