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
    
    [Header("Состояние")]
    [SerializeField] private bool _isAlerted = false;
    [SerializeField] private bool _isSuspicious = false;
    [SerializeField] private float _suspicionLevel = 0f;
    
    [Header("Цели")]
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
    private float _suspicionDecayRate = 0.5f; // Скорость снижения подозрений
    
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
        UpdateSuspicion();
        UpdateAlertTimer();
    }
    
    private void CheckForPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, _target.transform.position);
        
        // Проверяем дальность обнаружения
        if (distanceToPlayer <= _detectionRange)
        {
            // Проверяем поле зрения
            if (IsPlayerInFieldOfView())
            {
                float detectionChance = CalculateDetectionChance();
                
                if (detectionChance >= _suspicionThreshold)
                {
                    DetectPlayer();
                }
                else
                {
                    IncreaseSuspicion(detectionChance);
                }
            }
            
            // Проверяем слух (шум от игрока)
            if (_playerMover != null)
            {
                float noiseLevel = _playerMover.NoiseLevel;
                float hearingChance = CalculateHearingChance(distanceToPlayer, noiseLevel);
                
                if (hearingChance >= _suspicionThreshold)
                {
                    DetectPlayer();
                }
                else if (hearingChance > 0.3f)
                {
                    IncreaseSuspicion(hearingChance * 0.5f);
                }
            }
        }
        else
        {
            // Игрок вне зоны обнаружения
            if (_isAlerted)
            {
                LosePlayer();
            }
        }
    }
    
    private bool IsPlayerInFieldOfView()
    {
        Vector3 directionToPlayer = (_target.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        
        return angle <= _fieldOfView * 0.5f;
    }
    
    private float CalculateDetectionChance()
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
    
    private float CalculateHearingChance(float distance, float noiseLevel)
    {
        if (distance > _hearingRange) return 0f;
        
        float baseChance = noiseLevel * _noiseDetectionMultiplier;
        float distanceMultiplier = 1f - (distance / _hearingRange);
        
        return Mathf.Clamp01(baseChance * distanceMultiplier);
    }
    
    private void DetectPlayer()
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
    
    private void IncreaseSuspicion(float amount)
    {
        if (!_isAlerted)
        {
            _suspicionLevel = Mathf.Clamp01(_suspicionLevel + amount);
            
            if (_suspicionLevel >= _suspicionThreshold && !_isSuspicious)
            {
                _isSuspicious = true;
                SuspiciousChanged?.Invoke(_isSuspicious);
            }
            
            SuspicionLevelChanged?.Invoke(_suspicionLevel);
        }
    }
    
    private void UpdateSuspicion()
    {
        if (!_isAlerted && _suspicionLevel > 0f)
        {
            _suspicionLevel = Mathf.Max(0f, _suspicionLevel - _suspicionDecayRate * Time.deltaTime);
            
            if (_suspicionLevel < _suspicionThreshold && _isSuspicious)
            {
                _isSuspicious = false;
                SuspiciousChanged?.Invoke(_isSuspicious);
            }
            
            SuspicionLevelChanged?.Invoke(_suspicionLevel);
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
        if (target != null)
        {
            _playerMover = target.GetComponent<PlayerMover>();
        }
    }
    
    public void ResetToStartPosition()
    {
        transform.position = _startPosition;
        _isAlerted = false;
        _isSuspicious = false;
        _suspicionLevel = 0f;
        _alertTimer = 0f;
        
        AlertedChanged?.Invoke(_isAlerted);
        SuspiciousChanged?.Invoke(_isSuspicious);
        SuspicionLevelChanged?.Invoke(_suspicionLevel);
    }
    
    private void Die()
    {
        Dying?.Invoke();
        Destroy(gameObject);
    }
    
    // Методы для отладки
    private void OnDrawGizmosSelected()
    {
        // Поле зрения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
        
        // Поле слуха
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _hearingRange);
        
        // Направление взгляда
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
        
        // Последняя известная позиция игрока
        if (_lastKnownPosition != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_lastKnownPosition.position, 0.5f);
        }
    }
}
