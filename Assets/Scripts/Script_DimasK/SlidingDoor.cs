using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private Transform _doorTransform;
    public Vector3 openOffset = new Vector3(2f, 0, 0);
    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _stayOpenTime = 2f;

    private Vector3 _closedPosition;
    private Vector3 _openPosition;
    private float _timer;
    private bool _isOpen;
    private bool _isMoving;

    private void Awake()
    {
        if (_doorTransform == null) _doorTransform = transform;
        _closedPosition = _doorTransform.localPosition;
        _openPosition = _closedPosition + openOffset;
    }

    private void Update()
    {
        if (_isOpen && !_isMoving)
        {
            _timer += Time.deltaTime;
            if (_timer >= _stayOpenTime)
                _isMoving = true;
        }

        if (_isMoving && _isOpen)
        {
            _doorTransform.localPosition = Vector3.MoveTowards(_doorTransform.localPosition, _openPosition, _speed * Time.deltaTime);
            if (Vector3.Distance(_doorTransform.localPosition, _openPosition) < 0.01f)
            {
                _doorTransform.localPosition = _openPosition;
                _isMoving = false;
                _timer = 0;
            }
        }
        else if (_isMoving && !_isOpen)
        {
            _doorTransform.localPosition = Vector3.MoveTowards(_doorTransform.localPosition, _closedPosition, _speed * Time.deltaTime);
            if (Vector3.Distance(_doorTransform.localPosition, _closedPosition) < 0.01f)
            {
                _doorTransform.localPosition = _closedPosition;
                _isMoving = false;
            }
        }
    }

    public void OpenDoor()
    {
        if (!_isOpen)
        {
            _isOpen = true;
            _isMoving = true;
            _timer = 0;
        }
    }

    public void CloseDoor()
    {
        if (_isOpen)
        {
            _isOpen = false;
            _isMoving = true;
            _timer = 0;
        }
    }
}