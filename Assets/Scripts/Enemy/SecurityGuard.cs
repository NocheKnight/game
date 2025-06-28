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
        Debug.Log($"SecurityGuard Start: {gameObject.name}, наследует от Enemy: {this is Enemy}");
        
        // Проверяем стейт-машину
        var stateMachine = GetComponent<EnemyStateMachine>();
        if (stateMachine != null)
        {
            Debug.Log($"StateMachine найдена: {stateMachine.name}");
            Debug.Log($"First State: {stateMachine.Current?.name ?? "NULL"}");
        }
        else
        {
            Debug.LogError("StateMachine НЕ НАЙДЕНА!");
        }
        
        // Проверяем PatrolState
        var patrolState = GetComponent<PatrolState>();
        if (patrolState != null)
        {
            Debug.Log($"PatrolState найден: {patrolState.name}");
            Debug.Log($"PatrolState активен: {patrolState.enabled}");
        }
        else
        {
            Debug.LogError("PatrolState НЕ НАЙДЕН!");
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
        Debug.Log("Охранник зовёт подмогу!");
    }

    public void OnBackupCalled(Vector3 point)
    {
        // Можно реализовать: идти к точке, повышать тревогу и т.д.
        Debug.Log($"Охранник получил сигнал подмоги! Бежит к {point}");
    }

    public void Distract(Vector3 distractionPoint)
    {
        // Реакция на отвлечение (аналогично кассирше)
        var stateMachine = GetComponent<EnemyStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.SetPatrolDestination(distractionPoint);
        }
        Debug.Log("Охранник отвлечён!");
    }

    protected override void OnPlayerSeen()
    {
        Debug.Log($"Охранник видит игрока! Подозрение: {SuspicionLevel:F2}");
        
        float suspicionSpeed = _enhancedSuspicionGrowthSpeed;
        var playerMover = Target != null ? Target.GetComponent<PlayerMover>() : null;
        
        if (playerMover != null && playerMover.IsStealthMode)
        {
            suspicionSpeed *= _stealthSuspicionMultiplier;
            Debug.Log("Игрок крадётся! Множитель подозрения: " + _stealthSuspicionMultiplier);
        }
        
        if (Target != null && Target.IsStealing)
        {
            suspicionSpeed *= _theftSuspicionMultiplier;
            Debug.Log("Игрок ворует! Множитель подозрения: " + _theftSuspicionMultiplier);
        }
        
        AddSuspicion(suspicionSpeed * Time.deltaTime);
        Debug.Log($"Новое подозрение: {SuspicionLevel:F2}");
    }
    
    protected override void OnPlayerHeard(float distance, float noiseLevel)
    {
        Debug.Log($"Охранник слышит игрока! Расстояние: {distance:F1}, шум: {noiseLevel:F2}");
        
        // Охранник более чувствителен к звукам
        float hearingChance = CalculateHearingChance(distance, noiseLevel) * 1.5f;
        if (UnityEngine.Random.value <= hearingChance)
        {
            AddSuspicion(_enhancedSuspicionGrowthSpeed * 0.7f * Time.deltaTime);
        }
    }
    
    protected override void OnPlayerOutOfRange()
    {
        Debug.Log($"Игрок вне зоны! Подозрение спадает...");
        // Подозрения спадают автоматически в базовом классе
    }
} 