using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public CarFollow followCamera;

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        followCamera.SetCameraControl(true);
    }

    public void BackToMenu()
    {
        mainMenuPanel.SetActive(true);
        followCamera.SetCameraControl(false);
    }
}