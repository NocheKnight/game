// D:/unity/projects/Kradylechka/Assets/Scripts/Enemy/Customer.cs
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Customer : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private Transform[] _patrolPoints;
    [SerializeField] private float _patrolSpeed = 1.5f;
    [SerializeField] private float _promoSpeed = 3f;
    [SerializeField] private float _fleeSpeed = 4f;
    [SerializeField] private float _waitTime = 2f;

    [Header("Senses")]
    [SerializeField] private float _viewRadius = 10f;
    [SerializeField] [Range(0, 360)] private float _viewAngle = 90f;
    [SerializeField] private float _eyeHeight = 1.6f;

    public NavMeshAgent Agent { get; private set; }
    public Animator Animator { get; private set; }
    public Transform[] PatrolPoints => _patrolPoints;
    public float PatrolSpeed => _patrolSpeed;
    public float PromoSpeed => _promoSpeed;
    public float WaitTime => _waitTime;
    public float FleeSpeed => _fleeSpeed;
    public Vector3 TheftLocation => _theftLocation;
    public Vector3 PromoTargetLocation { get; private set; }

    private EnemyStateMachine _stateMachine;
    private bool _hasWitnessedTheft = false;
    private bool _isLuredByPromo = false;
    private Vector3 _theftLocation;

    public bool IsBlockingVision
    {
        get
        {
            if (_stateMachine.CurrentState is CustomerPatrolState patrolState)
            {
                return patrolState.IsWaiting;
            }
            return false;
        }
    }

    #region Unity Lifecycle
    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        
        // Проверяем инициализацию NavMeshAgent
        if (Agent == null)
        {
            Debug.LogError($"Customer {name}: NavMeshAgent не найден!");
            return;
        }
        
        // Убеждаемся, что агент активен
        if (!Agent.isActiveAndEnabled)
        {
            Debug.LogWarning($"Customer {name}: NavMeshAgent не активен!");
        }
        
        InitializeStateMachine();
    }

    private void OnEnable()
    {
        SuspicionEvents.OnRaised += HandleSuspicionEvent;
    }

    private void OnDisable()
    {
        SuspicionEvents.OnRaised -= HandleSuspicionEvent;
    }

    private void Update()
    {
        _stateMachine?.Update();
    }
    #endregion

    private void InitializeStateMachine()
    {
        _stateMachine = new EnemyStateMachine();

        var patrolState = new CustomerPatrolState(this);
        var fleeState = new CustomerFleeState(this);
        var promoState = new CustomerPromoState(this);

        _stateMachine.AddAnyTransition(fleeState, () => _hasWitnessedTheft);
        
        _stateMachine.AddTransition(patrolState, promoState, () => _isLuredByPromo);

        _stateMachine.AddTransition(fleeState, patrolState, () => !_hasWitnessedTheft);
        _stateMachine.AddTransition(promoState, patrolState, () => !_isLuredByPromo);

        _stateMachine.SetState(patrolState);
    }


    public void AnnouncePromo(Vector3 location)
    {
        if (_hasWitnessedTheft) return;

        Debug.Log($"{name} услышал про акцию!");
        PromoTargetLocation = location;
        _isLuredByPromo = true;
    }

    public void LoseInterestInPromo()
    {
        _isLuredByPromo = false;
    }

    private void HandleSuspicionEvent(SuspicionEvent e)
    {
        if (_hasWitnessedTheft) return;

        if (e.Type == SuspicionType.Theft && CanSeeLocation(e.Position))
        {
            Debug.Log($"{gameObject.name} увидел кражу в {e.Position}!");
            _theftLocation = e.Position;
            _hasWitnessedTheft = true; 

            var reportEvent = new SuspicionEvent(e.Position, 75f, SuspicionType.Theft);
            SuspicionEvents.Raise(reportEvent);
        }
    }

    public void CalmDown()
    {
        _hasWitnessedTheft = false;
    }

    private bool CanSeeLocation(Vector3 location)
    {
        Vector3 toLocation = (location - transform.position);
        if (toLocation.magnitude > _viewRadius) return false;

        if (Vector3.Angle(transform.forward, toLocation.normalized) > _viewAngle * 0.5f) return false;

        Vector3 eyePos = transform.position + Vector3.up * _eyeHeight;
        if (Physics.Raycast(eyePos, toLocation.normalized, toLocation.magnitude, LayerMask.GetMask("Default", "Wall")))
        {
            return false;
        }

        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        UnityEditor.Handles.color = new Color(1, 1, 0, 0.1f);
        Vector3 eyePos = transform.position + Vector3.up * _eyeHeight;
        UnityEditor.Handles.DrawSolidArc(eyePos, Vector3.up, Quaternion.Euler(0, -_viewAngle * 0.5f, 0) * transform.forward, _viewAngle, _viewRadius);
    }
#endif
}