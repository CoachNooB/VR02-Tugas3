using UnityEngine;

public class InvisibleLineAntiCheat : MonoBehaviour
{   
    [SerializeField] private BoxCollider _finishLine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            _finishLine.enabled = true;
        }
    }
}
