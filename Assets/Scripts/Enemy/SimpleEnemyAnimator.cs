using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class SimpleEnemyAnimator : MonoBehaviour
{
    [Header("Аниматор")]
    [SerializeField] private Animator _animator;
    
    [Header("Состояния")]
    [SerializeField] private string _idleState = "Idle";
    [SerializeField] private string _walkState = "Walk";
    [SerializeField] private string _runState = "Run";
    [SerializeField] private string _suspiciousState = "Suspicious";
    
    private Enemy _enemy;
    private EnemyStateMachine _stateMachine;
    private string _currentState = "";
    
    private void Start()
    {
        _enemy = GetComponent<Enemy>();
        _stateMachine = GetComponent<EnemyStateMachine>();
        
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
        
        // Устанавливаем начальное состояние
        PlayState(_idleState);
    }
    
    private void Update()
    {
        UpdateAnimationState();
    }
    
    private void UpdateAnimationState()
    {
        if (_animator == null || _enemy == null) return;
        
        string targetState = DetermineTargetState();
        
        if (targetState != _currentState)
        {
            PlayState(targetState);
        }
    }
    
    private string DetermineTargetState()
    {
        // Проверяем состояния стейт-машины
        if (_stateMachine != null)
        {
            var currentState = _stateMachine.Current;
            if (currentState != null)
            {
                if (currentState.GetType() == typeof(ChaseState))
                {
                    return _runState;
                }
                else if (currentState.GetType() == typeof(SuspiciousState))
                {
                    return _suspiciousState;
                }
                else if (currentState.GetType() == typeof(PatrolState))
                {
                    // Проверяем, движется ли охранник
                    if (IsMoving())
                    {
                        return _walkState;
                    }
                }
            }
        }
        
        return _idleState;
    }
    
    private bool IsMoving()
    {
        // Простая проверка движения по изменению позиции
        Vector3 currentPos = transform.position;
        float distance = Vector3.Distance(currentPos, _lastPosition);
        _lastPosition = currentPos;
        
        return distance > 0.01f;
    }
    
    private Vector3 _lastPosition;
    
    private void PlayState(string stateName)
    {
        if (_animator == null || string.IsNullOrEmpty(stateName)) return;
        
        _animator.Play(stateName);
        _currentState = stateName;
    }
    
    // Публичные методы для внешнего управления
    public void PlayIdle()
    {
        PlayState(_idleState);
    }
    
    public void PlayWalk()
    {
        PlayState(_walkState);
    }
    
    public void PlayRun()
    {
        PlayState(_runState);
    }
    
    public void PlaySuspicious()
    {
        PlayState(_suspiciousState);
    }
    
    // Метод для отладки
    [ContextMenu("Test Idle")]
    private void TestIdle()
    {
        PlayIdle();
    }
    
    [ContextMenu("Test Walk")]
    private void TestWalk()
    {
        PlayWalk();
    }
    
    [ContextMenu("Test Run")]
    private void TestRun()
    {
        PlayRun();
    }
    
    [ContextMenu("Test Suspicious")]
    private void TestSuspicious()
    {
        PlaySuspicious();
    }
} 