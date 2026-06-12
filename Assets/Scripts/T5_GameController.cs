using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class T5_GameController : MonoBehaviour
{
    [Header("Reload")]
    [SerializeField] Button _reloadButton;
    [SerializeField] GameObject _reloadBar;
    [SerializeField] Image _reloadImage;
    private bool isReloading = false;

    [Header("Shoot")]
    [SerializeField] Button _shootButton;
    [SerializeField] TextMeshProUGUI _teksAmmo;
    public int MaxAmmo;
    public int CurrentAmmo;
    public int Damage;

    [Header ("HP")]
    [SerializeField] TextMeshProUGUI _textHP;
    [SerializeField] Image _healthBar;
    [SerializeField] Image _damageOverlay;
    [SerializeField] Image _healOverlay;
    public float MaxHP;
    public float CurrentHP;
    private bool isDamaged = false;

    [Header ("Heal")]
    [SerializeField] Button _healButton;
    public int HealValue;
    private bool isHeal = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _textHP.text = CurrentHP + "/" + MaxHP;
        _teksAmmo.text = CurrentAmmo + "/" + MaxAmmo;
        _reloadButton.onClick.AddListener(OnReloadButtonClick);
        _shootButton.onClick.AddListener(OnShootButtonClick);
        _healButton.onClick.AddListener(OnHealButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealthBar();
        ReloadAnimation();
        DamageAnimation();
        HealAnimation();
        if(CurrentAmmo < 1)
        {
            _shootButton.interactable = false;
        }

        if(CurrentHP < 1)
        {
            _shootButton.interactable = false;
            _reloadButton.interactable = false;
        }
    }

    public void UpdateHealthBar ()
    {
        _healthBar.fillAmount = CurrentHP / MaxHP;
    }

    public void OnReloadButtonClick ()
    {
        isReloading = true;
        _reloadButton.interactable = !isReloading;
    }

    public void ReloadAnimation()
    {
        if(isReloading)
        {
            _reloadBar.SetActive(true);
            _reloadImage.fillAmount = _reloadImage.fillAmount + 0.5f * Time.deltaTime;
        }
        if(_reloadImage.fillAmount >= 1f)
        {
            _reloadBar.SetActive(false);
            _reloadButton.interactable = true;
            _reloadImage.fillAmount = 0;
            isReloading = false;
            CurrentAmmo = MaxAmmo;
            _shootButton.interactable = true;
        }
    }

    public void OnShootButtonClick()
    {
        isDamaged = true;
        CurrentAmmo -= 1;
        CurrentHP -= Damage;

        _textHP.text = CurrentHP + "/" + MaxHP;
        _teksAmmo.text = CurrentAmmo + "/" + MaxAmmo;
    }

    public void DamageAnimation()
    {
        Color damageColor = _damageOverlay.color;
        if(isDamaged)
        {
            damageColor.a = 0.6f;
            _damageOverlay.color = damageColor;
        }
        isDamaged = false;
        damageColor.a -= 0.5f * Time.deltaTime;
        _damageOverlay.color = damageColor;
    }

    public void OnHealButtonClick()
    {
        isHeal = true;
        if((CurrentHP + HealValue) >= MaxHP)
        {
            CurrentHP = MaxHP;
        } else {
            CurrentHP += HealValue;
        }

        _textHP.text = CurrentHP + "/" + MaxHP;
    }
    public void HealAnimation()
    {
        Color healColor = _healOverlay.color;
        if(isHeal)
        {
            healColor.a = 0.6f;
            _healOverlay.color = healColor;
        }
        isHeal = false;
        healColor.a -= 0.5f * Time.deltaTime;
        _healOverlay.color = healColor;
    }
}
