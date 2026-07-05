using UnityEngine;

public class TunnelTrigger : MonoBehaviour
{
    public TrainFollow trainCamera;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (trainCamera != null)
                trainCamera.SetTunnel(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (trainCamera != null)
                trainCamera.SetTunnel(false);
        }
    }
}