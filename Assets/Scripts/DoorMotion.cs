using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DoorMotion : MonoBehaviour
{
    [Header("Настройки двери")]
    [SerializeField] private string _openParameterName = "DoorIsOpening";
    [SerializeField] private LayerMask _triggerLayers = -1; // Все слои по умолчанию
    
    private Animator _animator;
    private bool _isOpen = false;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        
        if (_animator == null)
        {
            Debug.LogError($"DoorMotion: Animator не найден на {gameObject.name}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что объект в нужном слое
        if (((1 << other.gameObject.layer) & _triggerLayers) != 0)
        {
            OpenDoor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Проверяем, что объект в нужном слое
        if (((1 << other.gameObject.layer) & _triggerLayers) != 0)
        {
            CloseDoor();
        }
    }
    
    public void OpenDoor()
    {
        if (_animator != null && !_isOpen)
        {
            _animator.SetBool(_openParameterName, true);
            _isOpen = true;
    }
    }
    
    public void CloseDoor()
    {
        if (_animator != null && _isOpen)
        {
            _animator.SetBool(_openParameterName, false);
            _isOpen = false;
        }
    }
    
    public void ToggleDoor()
    {
        if (_isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }
    
    public bool IsOpen => _isOpen;
}
