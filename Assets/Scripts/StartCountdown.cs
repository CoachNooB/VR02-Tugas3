using UnityEngine;
using TMPro;

// Countdown "3 - 2 - 1 - GO!" di awal balapan.
// Selama hitung mundur, game dibekukan (Time.timeScale = 0) biar mobil belum bisa jalan.
public class StartCountdown : MonoBehaviour
{
    [Header("Referensi")]
    public TextMeshProUGUI countdownText;

    [Header("Pengaturan")]
    public float countFrom = 3f;       // mulai hitung dari angka berapa
    public float goDuration = 1f;      // berapa lama tulisan "GO!" tampil

    private float timer;
    private bool finished = false;

    private void Start()
    {
        timer = countFrom;
        Time.timeScale = 0f;           // bekukan game saat hitung mundur

        if (countdownText != null)
            countdownText.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (finished)
            return;

        // pakai unscaledDeltaTime karena timeScale lagi 0 (deltaTime biasa = 0)
        timer -= Time.unscaledDeltaTime;

        if (countdownText == null)
        {
            // kalau teks lupa dipasang, tetap mulai balapan setelah waktunya habis
            if (timer <= 0f) StartRace();
            return;
        }

        if (timer > 0f)
        {
            // tampilkan 3, 2, 1 (dibulatkan ke atas)
            countdownText.text = Mathf.Ceil(timer).ToString();
        }
        else if (timer > -goDuration)
        {
            countdownText.text = "GO!";
        }
        else
        {
            StartRace();
        }
    }

    private void StartRace()
    {
        finished = true;
        Time.timeScale = 1f;           // lepaskan, balapan dimulai

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }
}
