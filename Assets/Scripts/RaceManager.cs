using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// RaceManager: ngurus timer balapan, deteksi selesai, layar finish, dan restart.
// Lap-nya sendiri sudah dihitung di Player.cs (tag "StartFinishLine").
// Script ini cuma "membaca" status lap dari Player, lalu menampilkan UI.
public class RaceManager : MonoBehaviour
{
    [Header("Referensi")]
    public Player player;                   // mobil pemain (drag dari Hierarchy)
    public TextMeshProUGUI timerText;       // teks waktu berjalan (HUD)
    public GameObject finishPanel;          // panel yang muncul saat selesai
    public TextMeshProUGUI finishTimeText;  // teks waktu akhir di panel finish
    public AudioSource finishSound;         // suara saat finish (opsional, Fase 3)

    private float elapsedTime = 0f;         // waktu yang sudah berjalan (detik)
    private bool isRacing = true;           // true selama balapan masih berlangsung

    private void Start()
    {
        // pastikan panel finish disembunyikan dulu di awal
        if (finishPanel != null) 
        {
            finishPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isRacing)
        {
            // Sudah finish: tekan ENTER untuk main lagi.
            // (pakai keyboard biar pasti jalan, nggak kena masalah klik saat Time.timeScale = 0)
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                RestartRace();
            return;
        }

        // Cek apakah pemain sudah menyelesaikan semua lap.
        // (Player.cs menambah currentLap tiap melewati garis finish)
        if (player != null && player.currentLap > player.raceLap)
        {
            FinishRace();
            return;
        }

        // Tambah waktu tiap frame. Time.deltaTime = waktu antar frame (materi P2).
        elapsedTime += Time.deltaTime;
        UpdateTimerText(timerText, elapsedTime);
    }

    private void FinishRace()
    {
        isRacing = false;

        if (finishPanel != null)
            finishPanel.SetActive(true);

        // tampilkan waktu akhir di panel
        UpdateTimerText(finishTimeText, elapsedTime);

        // munculkan kursor lagi biar bisa klik tombol "Main Lagi"
        // (saat balapan, kursor disembunyikan & dikunci untuk memutar kamera)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (finishSound != null)
            finishSound.Play();
    }

    // Ubah angka detik jadi format menit:detik (contoh 83 detik -> "01:23")
    private void UpdateTimerText(TextMeshProUGUI text, float time)
    {
        if (text == null)
            return;

        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        text.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    // Dipanggil oleh tombol "Restart" (lewat event OnClick di Inspector)
    public void RestartRace()
    {
        // PENTING: Player.cs mengubah Time.timeScale jadi 0 saat finish (game beku).
        // Tanpa baris ini, scene yang di-reload akan tetap beku.
        Time.timeScale = 1f;

        // muat ulang scene yang sedang aktif (mulai dari awal)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
