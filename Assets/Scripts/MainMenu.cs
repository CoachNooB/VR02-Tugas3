using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public CarFollow followCamera;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject RaceManager;
    [SerializeField] private GameObject speedText;
    [SerializeField] private GameObject timerText;

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        EnableGame();
    }

    public void BackToMenu()
    {
        mainMenuPanel.SetActive(true);
        followCamera.SetCameraControl(false);
    }

    public void EnableGame()
    {
        player.SetActive(true);
        RaceManager.SetActive(true);
        followCamera.SetCameraControl(true);
        speedText.SetActive(true);
        timerText.SetActive(true);
    }
}