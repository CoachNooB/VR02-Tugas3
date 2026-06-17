using UnityEngine;
using UnityEngine.UI;

public class UTS_EmergencyButton : MonoBehaviour
{
    [SerializeField] UTS_GameController _gameController;
    [SerializeField] Button _emergencyExitButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _emergencyExitButton.onClick.AddListener(OnEmergencyButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEmergencyButtonClick()
    {
        _gameController.SetEmergencyExitActive();
    }
}
