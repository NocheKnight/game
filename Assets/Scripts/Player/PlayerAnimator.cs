using UnityEngine;

[RequireComponent(typeof(PlayerMover))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Аниматор")]
    [SerializeField] private Animator _animator;
    
    [Header("Параметры анимации")]
    [SerializeField] private string _isWalkingParam = "IsWalking";
    [SerializeField] private string _isRunningParam = "IsRunning";
    [SerializeField] private string _isCrouchingParam = "IsCrouching";
    [SerializeField] private string _isStealthParam = "IsStealth";
    [SerializeField] private string _speedParam = "Speed";
    
    private PlayerMover _mover;
    private bool _isMoving = false;
    private float _currentSpeed = 0f;
    
    private void Start()
    {
        _mover = GetComponent<PlayerMover>();
        
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
        
        // Подписываемся на события
        if (_mover != null)
        {
            _mover.RunChanged += OnRunChanged;
            _mover.StealthModeChanged += OnStealthChanged;
            _mover.CrouchChanged += OnCrouchChanged;
            _mover.SpeedChanged += OnSpeedChanged;
        }
    }
    
    private void Update()
    {
        UpdateMovementAnimation();
    }
    
    private void UpdateMovementAnimation()
    {
        if (_animator == null || _mover == null) return;
        
        // Определяем, движется ли игрок
        _isMoving = _mover.IsMoving();
        _currentSpeed = _mover.CurrentSpeed;
        
        // Обновляем параметры анимации
        _animator.SetBool(_isWalkingParam, _isMoving && !_mover.IsRunning && !_mover.IsCrouching);
        _animator.SetBool(_isRunningParam, _isMoving && _mover.IsRunning && !_mover.IsCrouching);
        _animator.SetBool(_isCrouchingParam, _mover.IsCrouching);
        _animator.SetBool(_isStealthParam, _mover.IsStealthMode);
        _animator.SetFloat(_speedParam, _currentSpeed);
    }
    
    private void OnRunChanged(bool isRunning)
    {
        if (_animator != null)
        {
            _animator.SetBool(_isRunningParam, isRunning && _mover.IsMoving());
        }
    }
    
    private void OnStealthChanged(bool isStealth)
    {
        if (_animator != null)
        {
            _animator.SetBool(_isStealthParam, isStealth);
        }
    }
    
    private void OnCrouchChanged(bool isCrouching)
    {
        if (_animator != null)
        {
            _animator.SetBool(_isCrouchingParam, isCrouching);
        }
    }
    
    private void OnSpeedChanged(float speed)
    {
        if (_animator != null)
        {
            _animator.SetFloat(_speedParam, speed);
        }
    }
    
    // Публичные методы для внешнего управления
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
    
    private void OnDestroy()
    {
        if (_mover != null)
        {
            _mover.RunChanged -= OnRunChanged;
            _mover.StealthModeChanged -= OnStealthChanged;
            _mover.CrouchChanged -= OnCrouchChanged;
            _mover.SpeedChanged -= OnSpeedChanged;
        }
    }
} 