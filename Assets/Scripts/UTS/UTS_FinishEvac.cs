using UnityEngine;

public class UTS_FinishEvac : MonoBehaviour
{
    [SerializeField] UTS_GameController _gameController;
    [SerializeField] BoxCollider _finishCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            _gameController.SetEvacFinish();
        }
    }
}
