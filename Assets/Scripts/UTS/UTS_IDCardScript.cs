using UnityEngine;
using UnityEngine.UI;

public class UTS_IDCardScript : MonoBehaviour
{
    [SerializeField] GameObject _idCard;
    [SerializeField] Button _idCardButtonA;
    [SerializeField] Button _idCardButtonB;
    [SerializeField] UTS_GameController _gameController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _idCardButtonA.onClick.AddListener(OnIDCardClick);
        _idCardButtonB.onClick.AddListener(OnIDCardClick);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, 90f * Time.deltaTime, 0f);
    }

    public void OnIDCardClick()
    {
        _gameController.SetHasKeyCard();
        _idCard.SetActive(false);
    }
}
