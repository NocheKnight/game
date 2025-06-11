using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInventory))]
public class PlayerMover : MonoBehaviour
{
    [Header("Скорости движения")]
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _runSpeed = 6f;
    [SerializeField] private float _crouchSpeed = 1.5f;
    
    [Header("Настройки перегруза")]
    [SerializeField] private float _overloadSpeedMultiplier = 0.5f;
    [SerializeField] private float _maxOverloadPenalty = 0.3f;
    
    [Header("Настройки стелса")]
    [SerializeField] private float _stealthSpeedMultiplier = 0.7f;
    [SerializeField] private float _noiseLevel = 1f;
    
    private float _currentSpeed;
    private bool _isOverloaded = false;
    private bool _isCrouching = false;
    private bool _isRunning = false;
    private bool _isStealthMode = false;
    
    private Player _player;
    private PlayerInventory _inventory;
    private Rigidbody _rigidbody;
    private Vector3 _moveDirection;
    
    public bool IsOverloaded => _isOverloaded;
    public bool IsCrouching => _isCrouching;
    public bool IsRunning => _isRunning;
    public bool IsStealthMode => _isStealthMode;
    public float CurrentSpeed => _currentSpeed;
    public float NoiseLevel => _noiseLevel;
    
    public event UnityAction<bool> OverloadChanged;
    public event UnityAction<bool> CrouchChanged;
    public event UnityAction<bool> RunChanged;
    public event UnityAction<bool> StealthModeChanged;
    public event UnityAction<float> SpeedChanged;
    
    private void Awake()
    {
        _player = GetComponent<Player>();
        _inventory = GetComponent<PlayerInventory>();
        _rigidbody = GetComponent<Rigidbody>();
        
        // if (_rigidbody == null)
        // {
        //     _rigidbody = gameObject.AddComponent<Rigidbody>();
        //     _rigidbody.freezeRotation = true;
        //     _rigidbody.drag = 1f;
        // }
    }
    
    private void Start()
    {
        UpdateSpeed();
    }
    
    private void Update()
    {
        HandleInput();
        UpdateSpeed();
        UpdateNoiseLevel();
    }
    
    private void FixedUpdate()
    {
        Move();
    }
    
    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        _moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        
        // Переключение режимов движения
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            ToggleRun();
        }
        
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            ToggleCrouch();
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleStealthMode();
        }
    }
    
    private void Move()
    {
        if (_moveDirection.magnitude > 0.1f)
        {
            // Поворачиваем персонажа в направлении движения
            Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
            
            Vector3 velocity = _moveDirection * _currentSpeed;
            _rigidbody.velocity = new Vector3(velocity.x, _rigidbody.velocity.y, velocity.z);
        }
        else
        {
            _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
        }
    }
    
    private void UpdateSpeed()
    {
        float baseSpeed = _walkSpeed;
        
        if (_isRunning && !_isCrouching)
        {
            baseSpeed = _runSpeed;
        }
        else if (_isCrouching)
        {
            baseSpeed = _crouchSpeed;
        }
        
        float speedMultiplier = 1f;
        
        if (_isOverloaded)
        {
            speedMultiplier *= _overloadSpeedMultiplier;
        }
        
        if (_isStealthMode)
        {
            speedMultiplier *= _stealthSpeedMultiplier;
        }
        
        if (_player != null)
        {
            speedMultiplier += _player.GetStealthBonus();
        }
        
        _currentSpeed = baseSpeed * speedMultiplier;
        SpeedChanged?.Invoke(_currentSpeed);
    }
    
    private void UpdateNoiseLevel()
    {
        float baseNoise = 1f;
        
        // Увеличиваем шум при беге
        if (_isRunning)
        {
            baseNoise = 2f;
        }
        
        // Уменьшаем шум при крадущемся движении
        if (_isCrouching || _isStealthMode)
        {
            baseNoise *= 0.3f;
        }
        
        // Увеличиваем шум при перегрузе
        if (_isOverloaded)
        {
            baseNoise *= 1.5f;
        }
        
        _noiseLevel = baseNoise;
    }
    
    public void SetOverloaded(bool overloaded)
    {
        if (_isOverloaded != overloaded)
        {
            _isOverloaded = overloaded;
            OverloadChanged?.Invoke(_isOverloaded);
            UpdateSpeed();
        }
    }
    
    public void ToggleCrouch()
    {
        _isCrouching = !_isCrouching;
        CrouchChanged?.Invoke(_isCrouching);
        
        if (_isCrouching && !_isStealthMode)
        {
            _isStealthMode = true;
            StealthModeChanged?.Invoke(_isStealthMode);
        }
    }
    
    public void ToggleRun()
    {
        if (!_isCrouching)
        {
            _isRunning = !_isRunning;
            RunChanged?.Invoke(_isRunning);
        }
    }
    
    public void ToggleStealthMode()
    {
        _isStealthMode = !_isStealthMode;
        StealthModeChanged?.Invoke(_isStealthMode);
        
        if (!_isStealthMode && _isCrouching)
        {
            _isCrouching = false;
            CrouchChanged?.Invoke(_isCrouching);
        }
    }
    
    public void ForceStop()
    {
        _moveDirection = Vector3.zero;
        _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
    }
    
    public bool IsMoving()
    {
        return _moveDirection.magnitude > 0.1f;
    }
    
    public Vector3 GetMoveDirection()
    {
        return _moveDirection;
    }
    
    // Метод для внешнего обновления перегруза
    public void UpdateOverloadStatus(int currentWeight, int maxWeight)
    {
        float overloadRatio = (float)currentWeight / maxWeight;
        bool shouldBeOverloaded = overloadRatio > 0.8f; // Перегруз при 80% заполнения
        
        SetOverloaded(shouldBeOverloaded);
    }
}