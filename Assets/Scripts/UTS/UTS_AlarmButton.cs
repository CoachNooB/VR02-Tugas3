using UnityEngine;
using UnityEngine.UI;

public class UTS_AlarmButton : MonoBehaviour
{
    [SerializeField] Button _alarmButton;
    [SerializeField] UTS_GameController _gameController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _alarmButton.onClick.AddListener(OnAlarmButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnAlarmButtonClick()
    {
        _gameController.SetAlarmActive();
    }
}
