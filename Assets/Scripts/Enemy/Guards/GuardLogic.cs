// D:/unity/projects/Kradylechka/Assets/Scripts/Enemy/GuardLogic.cs

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(Enemy))]
public class GuardLogic : MonoBehaviour
{
    [Header("Core AI Settings")]
    [Tooltip("Объект игрока, за которым будет вестись погоня.")]
    [SerializeField] private Transform _player;
    [Tooltip("Точки для маршрута патрулирования.")]
    [SerializeField] private Transform[] _patrolPoints;

    [Header("Senses")]
    [Tooltip("Максимальное расстояние, на котором охранник может заметить подозрительное событие.")]
    [SerializeField] private float _sightRadius = 20f;

    [Header("Movement Speeds")]
    [SerializeField] private float _patrolSpeed = 2f;
    [SerializeField] private float _investigateSpeed = 3f;
    [SerializeField] private float _chaseSpeed = 5f;

    [Header("Chase Logic")]
    [Tooltip("На каком расстоянии от игрока охранник его ловит.")]
    [SerializeField] private float _catchDistance = 1.5f;

    [Header("Live State (for debugging)")]
    [Tooltip("Текущий уровень подозрительности (0-100)")]
    [SerializeField][Range(0, 100)] private float _suspicionLevel;

    public NavMeshAgent Agent { get; private set; }
    public Animator Animator { get; private set; }
    public Enemy Enemy { get; private set; }
    public Vector3 LastKnownPosition { get; set; }
    public EnemyStateMachine StateMachine { get; private set; }

    public float SuspicionLevel => _suspicionLevel;
    public Transform Player => _player;
    public Transform[] PatrolPoints => _patrolPoints;
    public float PatrolSpeed => _patrolSpeed;
    public float InvestigateSpeed => _investigateSpeed;
    public float ChaseSpeed => _chaseSpeed;
    public float CatchDistance => _catchDistance;

    #region Unity Lifecycle Methods

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        Enemy = GetComponent<Enemy>();

        if (Enemy == null)
        {
            Debug.LogError($"Критическая ошибка: Компонент 'Enemy' (или 'SecurityGuard') отсутствует на объекте '{gameObject.name}'. Пожалуйста, добавьте его в редакторе Unity.", this);
            this.enabled = false; 
            return;
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
        StateMachine.Update();
        DecreaseSuspicion(Time.deltaTime * 2f); 
    }

    #endregion

    private void InitializeStateMachine()
    {
        StateMachine = new EnemyStateMachine();
        
        var idle = new IdleState(this);
        var patrol = new PatrolState(this);
        var investigate = new InvestigateState(this);
        var chase = new ChaseState(this);

        StateMachine.AddTransition(idle, patrol,       () => SuspicionLevel >= 15);
        StateMachine.AddTransition(patrol, investigate,() => SuspicionLevel >= 50);
        StateMachine.AddTransition(investigate, chase,() => SuspicionLevel >= 80);
        
        StateMachine.AddTransition(chase, investigate, () => SuspicionLevel < 80);
        StateMachine.AddTransition(investigate, patrol,() => SuspicionLevel < 50);
        StateMachine.AddTransition(patrol, idle,       () => SuspicionLevel < 15);

        StateMachine.SetState(idle);
    }

    private void HandleSuspicionEvent(SuspicionEvent e)
    {
        if (Vector3.Distance(transform.position, e.Position) > _sightRadius)
        {
            return; // Слишком далеко, игнорируем
        }

        // TODO: В будущем здесь можно добавить проверку линии видимости (Raycast)
        // чтобы охранник не реагировал на события за стеной.

        // 2. Если событие в зоне досягаемости, повышаем подозрительность
        Debug.Log($"{gameObject.name} заметил событие '{e.Type}' в точке {e.Position}");
        AddSuspicion(e.Amount, e.Position);
    }

    #region Public Methods

    public void AddSuspicion(float amount, Vector3 position)
    {
        _suspicionLevel = Mathf.Clamp(_suspicionLevel + amount, 0, 100);
        LastKnownPosition = position;
        Debug.Log($"Подозрение повышено до {_suspicionLevel}. Место: {position}");
    }


    public void DecreaseSuspicion(float amount)
    {
        _suspicionLevel = Mathf.Clamp(_suspicionLevel - amount, 0, 100);
    }

    [ContextMenu("Add 20 Suspicion")]
    private void TestAddSuspicion()
    {
        if (Player != null)
        {
            AddSuspicion(20, Player.position);
        }
        else
        {
            AddSuspicion(20, transform.position + transform.forward * 5f);
        }
    }

    #endregion
}