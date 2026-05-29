using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform targetToFollow;
    public Vector3 offset = new Vector3(0, 3, -6);

    void LateUpdate()
    {
        if (targetToFollow != null)
        {
            transform.position = targetToFollow.position + offset;
            transform.LookAt(targetToFollow);
        }
    }
}