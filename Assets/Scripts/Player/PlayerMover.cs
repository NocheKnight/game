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
    [SerializeField] private float _stealthSpeed = 1.5f;
    
    [Header("Настройки перегруза")]
    [SerializeField] private float _overloadSpeedMultiplier = 0.5f;
    [SerializeField] private float _maxOverloadPenalty = 0.3f;
    
    [Header("Настройки стелса")]
    [SerializeField] private float _stealthSpeedMultiplier = 0.7f;
    [SerializeField] private float _noiseLevel = 1f;
    
    [Header("Настройки камеры")]
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private Transform _cameraHolder;
    [SerializeField] private float _maxLookAngle = 80f;
    
    private float _currentSpeed;
    private bool _isOverloaded = false;
    private bool _isRunning = false;
    private bool _isStealthMode = false;
    
    private Player _player;
    private PlayerInventory _inventory;
    private Rigidbody _rigidbody;
    private Vector3 _moveDirection;
    private float _verticalRotation = 0f;
    
    public bool IsOverloaded => _isOverloaded;
    public bool IsRunning => _isRunning;
    public bool IsStealthMode => _isStealthMode;
    public float CurrentSpeed => _currentSpeed;
    public float NoiseLevel => _noiseLevel;
    
    public event UnityAction<bool> OverloadChanged;
    public event UnityAction<bool> RunChanged;
    public event UnityAction<bool> StealthModeChanged;
    public event UnityAction<float> SpeedChanged;
    
    private void Awake()
    {
        _player = GetComponent<Player>();
        _inventory = GetComponent<PlayerInventory>();
        _rigidbody = GetComponent<Rigidbody>();
        
        if (_cameraHolder == null)
        {
            GameObject cameraHolder = new GameObject("CameraHolder");
            cameraHolder.transform.SetParent(transform);
            cameraHolder.transform.localPosition = new Vector3(0, 1.6f, 0); // Высота глаз
            _cameraHolder = cameraHolder.transform;
        }
        
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.SetParent(_cameraHolder);
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Start()
    {
        UpdateSpeed();
    }
    
    private void Update()
    {
        UpdateSpeed();
        UpdateNoiseLevel();
    }
    
    private void FixedUpdate()
    {
        Move();
    }
    
    public void SetMoveDirection(Vector2 direction)
    {
        Vector3 forward = _cameraHolder.forward;
        Vector3 right = _cameraHolder.right;
        
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        
        _moveDirection = (forward * direction.y + right * direction.x).normalized;
    }
    
    public void HandleMouseLook(float mouseX, float mouseY)
    {
        transform.Rotate(0, mouseX * _mouseSensitivity, 0);
        
        _verticalRotation -= mouseY * _mouseSensitivity;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -_maxLookAngle, _maxLookAngle);
        _cameraHolder.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
    }
    
    private void Move()
    {
        if (_moveDirection.magnitude > 0.1f)
        {
            Vector3 velocity = _moveDirection * _currentSpeed;
            _rigidbody.linearVelocity = new Vector3(velocity.x, _rigidbody.linearVelocity.y, velocity.z);
        }
        else
        {
            _rigidbody.linearVelocity = new Vector3(0f, _rigidbody.linearVelocity.y, 0f);
        }
    }
    
    private void UpdateSpeed()
    {
        float baseSpeed = _walkSpeed;
        
        if (_isRunning && !_isStealthMode)
        {
            baseSpeed = _runSpeed;
        }
        else if (_isStealthMode)
        {
            baseSpeed = _stealthSpeed;
        }
        
        float speedMultiplier = 1f;
        
        if (_isOverloaded)
        {
            speedMultiplier *= _overloadSpeedMultiplier;
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
        
        if (_isRunning)
        {
            baseNoise = 2f;
        }
        
        if (_isStealthMode)
        {
            baseNoise *= 0.3f;
        }
        
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
    
    public void ToggleRun()
    {
        if (!_isStealthMode) // Нельзя бежать в стелс-режиме
        {
            _isRunning = !_isRunning;
            RunChanged?.Invoke(_isRunning);
        }
    }
    
    public void ToggleStealthMode()
    {
        _isStealthMode = !_isStealthMode;
        StealthModeChanged?.Invoke(_isStealthMode);
        
        // Если включаем стелс, выключаем бег
        if (_isStealthMode && _isRunning)
        {
            _isRunning = false;
            RunChanged?.Invoke(_isRunning);
        }
    }
    
    public void ForceStop()
    {
        _moveDirection = Vector3.zero;
        _rigidbody.linearVelocity = new Vector3(0f, _rigidbody.linearVelocity.y, 0f);
    }
    
    public bool IsMoving()
    {
        return _moveDirection.magnitude > 0.1f;
    }
    
    public Vector3 GetMoveDirection()
    {
        return _moveDirection;
    }
    
    public void UpdateOverloadStatus(int currentWeight, int maxWeight)
    {
        float overloadRatio = (float)currentWeight / maxWeight;
        bool shouldBeOverloaded = overloadRatio > 0.8f; // Перегруз при 80% заполнения
        
        SetOverloaded(shouldBeOverloaded);
    }
}