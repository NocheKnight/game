using System;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    [Header("Основные характеристики")]
    [SerializeField] private int _health = 100;
    [SerializeField] private float _detectionRange = 10f;
    [SerializeField] private float _fieldOfView = 90f;
    [SerializeField] private float _hearingRange = 8f;
    
    [Header("Настройки стелса")]
    [SerializeField] private float _stealthDetectionMultiplier = 0.5f;
    [SerializeField] private float _noiseDetectionMultiplier = 1.5f;
    [SerializeField] private float _suspicionThreshold = 0.8f;
    
    [Header("Система подозрений")]
    [SerializeField] private float _suspicionGrowthSpeed = 0.2f;
    [SerializeField] private float _suspicionDecaySpeed = 0.1f;
    [SerializeField] private float _suspicionLevel = 0f;
    [SerializeField] private bool _isSuspicious = false;
    
    [Header("Состояние")]
    [SerializeField] private bool _isAlerted = false;
    [SerializeField] private Player _target;
    [SerializeField] private Transform _lastKnownPosition;
    
    public Player Target => _target;
    public bool IsAlerted => _isAlerted;
    public bool IsSuspicious => _isSuspicious;
    public float SuspicionLevel => _suspicionLevel;
    public Transform LastKnownPosition => _lastKnownPosition;
    public float DetectionRange => _detectionRange;
    public float FieldOfView => _fieldOfView;
    
    public event UnityAction Dying;
    public event UnityAction<bool> AlertedChanged;
    public event UnityAction<bool> SuspiciousChanged;
    public event UnityAction<float> SuspicionLevelChanged;
    public event UnityAction<Player> PlayerDetected;
    public event UnityAction PlayerLost;
    
    private EnemyStateMachine _stateMachine;
    private PlayerMover _playerMover;
    private Vector3 _startPosition;
    private float _alertTimer = 0f;
    
    private void Awake()
    {
        _stateMachine = GetComponent<EnemyStateMachine>();
        _startPosition = transform.position;
        
        // Находим игрока если не назначен
        if (_target == null)
        {
            _target = FindObjectOfType<Player>();
        }
        
        if (_target != null)
        {
            _playerMover = _target.GetComponent<PlayerMover>();
        }
    }
    
    private void Start()
    {
        if (_stateMachine != null)
        {
            _stateMachine.Initialize(_target);
        }
    }
    
    private void Update()
    {
        if (_target == null) return;
        
        CheckForPlayer();
        UpdateAlertTimer();
        UpdateSuspicionDecay();
    }
    
    private void CheckForPlayer()
    {
        if (_target == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _target.transform.position);
        
        if (distanceToPlayer <= _detectionRange)
        {
            if (IsPlayerInFieldOfView())
            {
                OnPlayerSeen();
            }
            
            if (_playerMover != null)
            {
                OnPlayerHeard(distanceToPlayer, _playerMover.NoiseLevel);
            }
        }
        else
        {
            OnPlayerOutOfRange();
        }
    }
    
    private bool IsPlayerInFieldOfView()
    {
        Vector3 directionToPlayer = (_target.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > _fieldOfView * 0.5f)
            return false;

        // --- Проверка на толпу покупателей ---
        float distance = Vector3.Distance(transform.position, _target.transform.position);
        Ray ray = new Ray(transform.position + Vector3.up * 1.5f, directionToPlayer); // немного выше, чтобы не цеплять пол
        RaycastHit[] hits = Physics.RaycastAll(ray, distance);
        foreach (var hit in hits)
        {
            var customer = hit.collider.GetComponent<Customer>();
            if (customer != null && customer.IsBlockingVision)
            {
                // Между врагом и игроком есть покупатель, который мешает обзору
                return false;
            }
        }
        return true;
    }
    
    protected float CalculateDetectionChance()
    {
        float baseChance = 1f;
        float distance = Vector3.Distance(transform.position, _target.transform.position);
        
        // Уменьшаем шанс с расстоянием
        float distanceMultiplier = 1f - (distance / _detectionRange);
        baseChance *= distanceMultiplier;
        
        // Учитываем стелс-режим игрока
        if (_playerMover != null && _playerMover.IsStealthMode)
        {
            baseChance *= _stealthDetectionMultiplier;
        }
        
        // Учитываем навыки стелса игрока
        if (_target != null)
        {
            float stealthBonus = _target.GetStealthBonus();
            baseChance *= (1f - stealthBonus);
        }
        
        return Mathf.Clamp01(baseChance);
    }
    
    protected float CalculateHearingChance(float distance, float noiseLevel)
    {
        if (distance > _hearingRange) return 0f;
        
        float baseChance = noiseLevel * _noiseDetectionMultiplier;
        float distanceMultiplier = 1f - (distance / _hearingRange);
        
        return Mathf.Clamp01(baseChance * distanceMultiplier);
    }
    
    public void AddSuspicion(float amount)
    {
        float oldSuspicion = _suspicionLevel;
        _suspicionLevel = Mathf.Clamp01(_suspicionLevel + amount);
        
        // Проверяем, нужно ли изменить состояние подозрительности
        bool wasSuspicious = _isSuspicious;
        _isSuspicious = _suspicionLevel > 0.1f;
        
        if (_isSuspicious != wasSuspicious)
        {
            SuspiciousChanged?.Invoke(_isSuspicious);
        }
        
        // Проверяем, нужно ли перейти в состояние тревоги
        if (_suspicionLevel >= _suspicionThreshold && !_isAlerted)
        {
            DetectPlayer();
        }
        
        if (oldSuspicion != _suspicionLevel)
        {
            SuspicionLevelChanged?.Invoke(_suspicionLevel);
        }
    }
    
    public void ReduceSuspicion(float amount)
    {
        float oldSuspicion = _suspicionLevel;
        _suspicionLevel = Mathf.Max(0f, _suspicionLevel - amount);
        
        // Проверяем, нужно ли изменить состояние подозрительности
        bool wasSuspicious = _isSuspicious;
        _isSuspicious = _suspicionLevel > 0.1f;
        
        if (_isSuspicious != wasSuspicious)
        {
            SuspiciousChanged?.Invoke(_isSuspicious);
        }
        
        if (oldSuspicion != _suspicionLevel)
        {
            SuspicionLevelChanged?.Invoke(_suspicionLevel);
        }
    }
    
    protected void DetectPlayer()
    {
        if (!_isAlerted)
        {
            _isAlerted = true;
            _isSuspicious = false;
            _suspicionLevel = 1f;
            _lastKnownPosition = _target.transform;
            _alertTimer = 10f; // Время преследования
            
            AlertedChanged?.Invoke(_isAlerted);
            SuspiciousChanged?.Invoke(_isSuspicious);
            SuspicionLevelChanged?.Invoke(_suspicionLevel);
            PlayerDetected?.Invoke(_target);
            
            // Добавляем преступность игроку
            _target.AddCrimeRate(50);
        }
    }
    
    private void LosePlayer()
    {
        if (_isAlerted)
        {
            _isAlerted = false;
            _alertTimer = 0f;
            
            AlertedChanged?.Invoke(_isAlerted);
            PlayerLost?.Invoke();
        }
    }
    
    private void UpdateAlertTimer()
    {
        if (_isAlerted && _alertTimer > 0f)
        {
            _alertTimer -= Time.deltaTime;
            
            if (_alertTimer <= 0f)
            {
                LosePlayer();
            }
        }
    }
    
    private void UpdateSuspicionDecay()
    {
        // Подозрения спадают со временем, если игрок не в зоне видимости
        if (_suspicionLevel > 0f && !_isAlerted)
        {
            ReduceSuspicion(_suspicionDecaySpeed * Time.deltaTime);
        }
    }
    
    public void TakeDamage(int damage)
    {
        _health -= damage;
        
        if (_health <= 0)
        {
            Die();
        }
    }
    
    public void SetTarget(Player target)
    {
        _target = target;
        if (_target != null)
        {
            _playerMover = _target.GetComponent<PlayerMover>();
        }
    }
    
    public void ResetToStartPosition()
    {
        transform.position = _startPosition;
        _isAlerted = false;
        _isSuspicious = false;
        _suspicionLevel = 0f;
        _alertTimer = 0f;
        
        if (_stateMachine != null)
        {
            _stateMachine.ResetToFirstState();
        }
        
        AlertedChanged?.Invoke(_isAlerted);
        SuspiciousChanged?.Invoke(_isSuspicious);
        SuspicionLevelChanged?.Invoke(_suspicionLevel);
    }
    
    private void Die()
    {
        Dying?.Invoke();
        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Зона обнаружения
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
        
        // Зона слуха
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _hearingRange);
        
        // Поле зрения
        Gizmos.color = Color.yellow;
        Vector3 forward = transform.forward;
        Vector3 left = Quaternion.Euler(0, -_fieldOfView * 0.5f, 0) * forward;
        Vector3 right = Quaternion.Euler(0, _fieldOfView * 0.5f, 0) * forward;
        
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, left * _detectionRange);
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, right * _detectionRange);
        
        // Последняя известная позиция
        if (_lastKnownPosition != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_lastKnownPosition.position, 0.5f);
        }
    }
    
    protected virtual void OnPlayerSeen() 
    { 
        // Базовый метод - можно переопределить в наследниках
        float detectionChance = CalculateDetectionChance();
        if (UnityEngine.Random.value <= detectionChance)
        {
            AddSuspicion(_suspicionGrowthSpeed * Time.deltaTime);
        }
    }
    
    protected virtual void OnPlayerHeard(float distance, float noiseLevel) 
    { 
        // Базовый метод - можно переопределить в наследниках
        float hearingChance = CalculateHearingChance(distance, noiseLevel);
        if (UnityEngine.Random.value <= hearingChance)
        {
            AddSuspicion(_suspicionGrowthSpeed * 0.5f * Time.deltaTime);
        }
    }
    
    protected virtual void OnPlayerOutOfRange() 
    { 
        // Базовый метод - можно переопределить в наследниках
        // Подозрения спадают автоматически в UpdateSuspicionDecay()
    }
    
    public void SetDetectionRange(float newRange)
    {
        _detectionRange = newRange;
    }
    
    public void SetFieldOfView(float newFieldOfView)
    {
        _fieldOfView = newFieldOfView;
    }
    
    public void SetHearingRange(float newHearingRange)
    {
        _hearingRange = newHearingRange;
    }
    
    public void SetSuspicionThreshold(float newThreshold)
    {
        _suspicionThreshold = newThreshold;
    }
    
    public void SetSuspicionGrowthSpeed(float newSpeed)
    {
        _suspicionGrowthSpeed = newSpeed;
    }
    
    public void SetSuspicionDecaySpeed(float newSpeed)
    {
        _suspicionDecaySpeed = newSpeed;
    }
}
