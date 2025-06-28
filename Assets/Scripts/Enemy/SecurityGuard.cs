using UnityEngine;

public class SecurityGuard : Enemy
{
    [Header("Охранник")]
    [SerializeField] private float _backupCallRadius = 10f;
    [SerializeField] private AudioClip _backupCallSound;

    [Header("Особенности охранника")]
    [SerializeField] private float _enhancedSuspicionGrowthSpeed = 0.3f;
    [SerializeField] private float _stealthSuspicionMultiplier = 2.0f;
    [SerializeField] private float _theftSuspicionMultiplier = 5.0f;

    private void Start()
    {
        // Проверяем стейт-машину
        var stateMachine = GetComponent<EnemyStateMachine>();
        if (stateMachine != null)
        {
            // First State: {stateMachine.Current?.name ?? "NULL"}
        }
        else
        {
            // StateMachine НЕ НАЙДЕНА!
        }
        
        // Проверяем PatrolState
        var patrolState = GetComponent<PatrolState>();
        if (patrolState != null)
        {
            // PatrolState найден: {patrolState.name}
            // PatrolState активен: {patrolState.enabled}
        }
        else
        {
            // PatrolState НЕ НАЙДЕН!
        }
    }

    public void CallForBackup()
    {
        // Находим всех охранников в радиусе и оповещаем их
        Collider[] hits = Physics.OverlapSphere(transform.position, _backupCallRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<SecurityGuard>(out var guard) && guard != this)
            {
                guard.OnBackupCalled(transform.position);
            }
        }
        if (_backupCallSound != null)
        {
            var audio = GetComponent<AudioSource>();
            if (audio != null) audio.PlayOneShot(_backupCallSound);
        }
    }

    public void OnBackupCalled(Vector3 point)
    {
        // Можно реализовать: идти к точке, повышать тревогу и т.д.
    }

    public void Distract(Vector3 distractionPoint)
    {
        // Реакция на отвлечение (аналогично кассирше)
        var stateMachine = GetComponent<EnemyStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.SetPatrolDestination(distractionPoint);
        }
    }

    protected override void OnPlayerSeen()
    {
        float suspicionSpeed = _enhancedSuspicionGrowthSpeed;
        var playerMover = Target != null ? Target.GetComponent<PlayerMover>() : null;
        
        if (playerMover != null && playerMover.IsStealthMode)
        {
            suspicionSpeed *= _stealthSuspicionMultiplier;
        }
        
        if (Target != null && Target.IsStealing)
        {
            suspicionSpeed *= _theftSuspicionMultiplier;
        }
        
        AddSuspicion(suspicionSpeed * Time.deltaTime);
    }
    
    protected override void OnPlayerHeard(float distance, float noiseLevel)
    {
        // Охранник более чувствителен к звукам
        float hearingChance = CalculateHearingChance(distance, noiseLevel) * 1.5f;
        if (UnityEngine.Random.value <= hearingChance)
        {
            AddSuspicion(_enhancedSuspicionGrowthSpeed * 0.7f * Time.deltaTime);
        }
    }
    
    protected override void OnPlayerOutOfRange()
    {
        // Подозрения спадают автоматически в базовом классе
    }
} 