using UnityEngine;

[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    [Header("Анимации")]
    [SerializeField] private Animator _animator;
    
    [Header("Параметры анимации")]
    [SerializeField] private string _isWalkingParam = "IsWalking";
    [SerializeField] private string _isRunningParam = "IsRunning";
    [SerializeField] private string _isSuspiciousParam = "IsSuspicious";
    [SerializeField] private string _isAlertedParam = "IsAlerted";
    [SerializeField] private string _speedParam = "Speed";
    [SerializeField] private string _lookAroundParam = "LookAround";
    
    [Header("Настройки движения")]
    [SerializeField] private float _movementThreshold = 0.1f;
    [SerializeField] private float _speedSmoothing = 5f;
    
    private Enemy _enemy;
    private EnemyStateMachine _stateMachine;
    private PatrolState _patrolState;
    private SuspiciousState _suspiciousState;
    private ChaseState _chaseState;
    
    private bool _isMoving = false;
    private float _currentSpeed = 0f;
    private Vector3 _lastPosition;
    private float _smoothedSpeed = 0f;
    
    private void Start()
    {
        _enemy = GetComponent<Enemy>();
        _stateMachine = GetComponent<EnemyStateMachine>();
        
        // Получаем ссылки на состояния
        _patrolState = GetComponent<PatrolState>();
        _suspiciousState = GetComponent<SuspiciousState>();
        _chaseState = GetComponent<ChaseState>();
        
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
        
        _lastPosition = transform.position;
        
        // Подписываемся на события
        if (_enemy != null)
        {
            _enemy.SuspiciousChanged += OnSuspiciousChanged;
            _enemy.AlertedChanged += OnAlertedChanged;
        }
    }
    
    private void Update()
    {
        UpdateMovementAnimation();
        UpdateStateAnimation();
    }
    
    private void UpdateMovementAnimation()
    {
        if (_animator == null) return;
        
        // Определяем скорость движения по изменению позиции
        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - _lastPosition;
        _currentSpeed = movement.magnitude / Time.deltaTime;
        
        // Сглаживаем скорость
        _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, _currentSpeed, _speedSmoothing * Time.deltaTime);
        
        // Определяем, движется ли охранник
        _isMoving = _smoothedSpeed > _movementThreshold;
        
        // Обновляем параметры анимации
        _animator.SetBool(_isWalkingParam, _isMoving && !IsInChaseState());
        _animator.SetBool(_isRunningParam, _isMoving && IsInChaseState());
        _animator.SetFloat(_speedParam, _smoothedSpeed);
        
        _lastPosition = currentPosition;
    }
    
    private void UpdateStateAnimation()
    {
        if (_animator == null) return;
        
        // Обновляем параметры состояний
        _animator.SetBool(_isSuspiciousParam, _enemy.IsSuspicious && !_enemy.IsAlerted);
        _animator.SetBool(_isAlertedParam, _enemy.IsAlerted);
        
        // Анимация осмотра в подозрительном состоянии
        if (_suspiciousState != null && _suspiciousState.enabled)
        {
            _animator.SetBool(_lookAroundParam, true);
        }
        else
        {
            _animator.SetBool(_lookAroundParam, false);
        }
    }
    
    private bool IsInChaseState()
    {
        return _chaseState != null && _chaseState.enabled;
    }
    
    private void OnSuspiciousChanged(bool isSuspicious)
    {
        if (_animator != null)
        {
            _animator.SetBool(_isSuspiciousParam, isSuspicious && !_enemy.IsAlerted);
        }
    }
    
    private void OnAlertedChanged(bool isAlerted)
    {
        if (_animator != null)
        {
            _animator.SetBool(_isAlertedParam, isAlerted);
            _animator.SetBool(_isSuspiciousParam, _enemy.IsSuspicious && !isAlerted);
        }
    }
    
    // Публичные методы для внешнего управления анимациями
    public void PlayAnimation(string triggerName)
    {
        if (_animator != null)
        {
            _animator.SetTrigger(triggerName);
        }
    }
    
    public void SetAnimationBool(string paramName, bool value)
    {
        if (_animator != null)
        {
            _animator.SetBool(paramName, value);
        }
    }
    
    public void SetAnimationFloat(string paramName, float value)
    {
        if (_animator != null)
        {
            _animator.SetFloat(paramName, value);
        }
    }
    
    public void SetAnimationInt(string paramName, int value)
    {
        if (_animator != null)
        {
            _animator.SetInteger(paramName, value);
        }
    }
    
    private void OnDestroy()
    {
        if (_enemy != null)
        {
            _enemy.SuspiciousChanged -= OnSuspiciousChanged;
            _enemy.AlertedChanged -= OnAlertedChanged;
        }
    }
} 