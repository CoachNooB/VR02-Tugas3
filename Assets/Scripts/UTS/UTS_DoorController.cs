using UnityEngine;
using UnityEngine.UI;

public class UTS_DoorController : MonoBehaviour
{
    [Header ("Door Button")]
    [SerializeField] Button _doorButton;
    [Header ("Left Door")]
    [SerializeField] Transform _leftDoor;
    private Vector3 leftDoorDefaultPosition;
    private Vector3 leftDoorTargetPosition;
    [Header ("Right Door")]
    [SerializeField] Transform _rightDoor;
    private Vector3 rightDoorDefaultPosition;
    private Vector3 rightDoorTargetPosition;

    private bool isOpen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isOpen = false;
        _doorButton.onClick.AddListener(OnDoorButtonClick);
        leftDoorDefaultPosition = _leftDoor.position;
        rightDoorDefaultPosition = _rightDoor.position;
        if(_leftDoor.localScale.x > _leftDoor.localScale.z)
        {
            leftDoorTargetPosition = new Vector3(_leftDoor.position.x - 5f, _leftDoor.position.y, _leftDoor.position.z);
        } 
        else 
        {
            leftDoorTargetPosition = new Vector3(_leftDoor.position.x, _leftDoor.position.y, _leftDoor.position.z + 5f);
        }

        if(_rightDoor.localScale.x > _rightDoor.localScale.z)
        {
            rightDoorTargetPosition = new Vector3(_rightDoor.position.x + 5f, _rightDoor.position.y, _rightDoor.position.z);
        } 
        else 
        {
            rightDoorTargetPosition = new Vector3(_rightDoor.position.x, _rightDoor.position.y, _rightDoor.position.z - 5f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isOpen)
        {
            AnimateDoorOpen();
        } else
        {
            AnimateDoorClose();
        }
    }

    public void OnDoorButtonClick()
    {
        isOpen = !isOpen;
    }

    public void AnimateDoorOpen()
    {
        _leftDoor.position = Vector3.MoveTowards(_leftDoor.position, leftDoorTargetPosition, 5f * Time.deltaTime);
        _rightDoor.position = Vector3.MoveTowards(_rightDoor.position, rightDoorTargetPosition, 5f * Time.deltaTime);
    }

    public void AnimateDoorClose()
    {
        _leftDoor.position = Vector3.MoveTowards(_leftDoor.position, leftDoorDefaultPosition, 5f * Time.deltaTime);
        _rightDoor.position = Vector3.MoveTowards(_rightDoor.position, rightDoorDefaultPosition, 5f * Time.deltaTime);
    }
}
