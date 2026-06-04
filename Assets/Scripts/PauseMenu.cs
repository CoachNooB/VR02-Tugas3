using UnityEngine;

// Menu jeda: tekan Esc untuk pause/lanjut. Saat pause, game dibekukan.
public class PauseMenu : MonoBehaviour
{
    [Header("Referensi")]
    public GameObject pausePanel;      // panel menu jeda

    private bool isPaused = false;
    private CursorLockMode previousLock;
    private bool previousCursorVisible;

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void Update()
    {
        // Input lama gaya P2 (project ini mode "Both", jadi ini aman dipakai)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);

        Time.timeScale = 0f;           // bekukan game

        // simpan kondisi kursor, lalu tampilkan biar bisa klik tombol
        previousLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Dipanggil tombol "Resume" (OnClick)
    public void Resume()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);

        Time.timeScale = 1f;           // lanjutkan game

        // kembalikan kondisi kursor seperti sebelum pause
        Cursor.lockState = previousLock;
        Cursor.visible = previousCursorVisible;
    }

    // Dipanggil Slider volume (OnValueChanged) -> atur volume semua suara
    public void SetVolume(float value)
    {
        AudioListener.volume = value;
    }
}
