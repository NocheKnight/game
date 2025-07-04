// D:/unity/projects/Kradylechka/Assets/Scripts/Enemy/EnemyAnimator.cs
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    private Animator _animator;
    private EnemyStateMachine _stateMachine;

    private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int _isRunningHash = Animator.StringToHash("IsRunning");
    private readonly int _isSuspiciousHash = Animator.StringToHash("IsSuspicious");
    private readonly int _speedHash = Animator.StringToHash("Speed");

    [Header("Animation Settings")]
    [Tooltip("How smoothly the speed parameter changes for fluid animation.")]
    [SerializeField] private float _speedSmoothing = 10f;

    private Vector3 _lastPosition;
    private float _smoothedSpeed;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        if (TryGetComponent<GuardLogic>(out var guardLogic))
        {
            _stateMachine = guardLogic.StateMachine;
        }
        else if (TryGetComponent<Customer>(out var customer))
        {
            // _stateMachine = customer.StateMachine; 
        }
        _lastPosition = transform.position;
    }

    private void OnEnable()
    {
        if (_stateMachine != null)
        {
            _stateMachine.OnStateChanged += HandleStateChange;
            if (_stateMachine.CurrentState != null)
            {
                HandleStateChange(_stateMachine.CurrentState);
            }
        }
    }

    private void OnDisable()
    {
        if (_stateMachine != null)
        {
            _stateMachine.OnStateChanged -= HandleStateChange;
        }
    }

    private void Update()
    {
        UpdateMovementAnimation();
    }

    private void HandleStateChange(IState newState)
    {
        if (newState == null || _animator == null) return;

        _animator.SetBool(_isWalkingHash, false);
        _animator.SetBool(_isRunningHash, false);
        _animator.SetBool(_isSuspiciousHash, false);

        if (newState is PatrolState)
        {
            _animator.SetBool(_isWalkingHash, true);
        }
        else if (newState is ChaseState)
        {
            _animator.SetBool(_isRunningHash, true);
        }
        else if (newState is InvestigateState)
        {
            _animator.SetBool(_isSuspiciousHash, true);
        }
    }

    private void UpdateMovementAnimation()
    {
        float currentSpeed = (transform.position - _lastPosition).magnitude / Time.deltaTime;

        _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, currentSpeed, _speedSmoothing * Time.deltaTime);

        // _animator.SetFloat(_speedHash, _smoothedSpeed);

        _lastPosition = transform.position;
    }
}