using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public SlidingDoor door;

    private void OnTriggerEnter(Collider other)
    {
        bool isPlayer = other.CompareTag("Player") || other.transform.root.CompareTag("Player");
        if (isPlayer)
        {
            Debug.Log($"Player masuk trigger: {gameObject.name}");
            if (door != null)
                door.OpenDoor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        bool isPlayer = other.CompareTag("Player") || other.transform.root.CompareTag("Player");
        if (isPlayer)
        {
            if (door != null)
                door.CloseDoor();
        }
    }
}