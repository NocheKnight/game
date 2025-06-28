using UnityEngine;

[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(EnemyStateMachine))]
public class CashierGalya : MonoBehaviour
{
    [Header("Особенности Кассирши")]
    [SerializeField] private float _xrayVisionRange = 12f; // Видит сквозь одежду
    [SerializeField] private float _xrayDetectionMultiplier = 1.2f;
    [SerializeField] private bool _canSeeThroughWalls = false;
    [SerializeField] private float _distractionResistance = 0.8f;
    
    [Header("Отвлечения")]
    [SerializeField] private bool _isDistracted = false;
    [SerializeField] private float _distractionDuration = 3f;
    [SerializeField] private float _distractionTimer = 0f;
    
    [Header("Звуки")]
    [SerializeField] private AudioClip _detectionSound;
    [SerializeField] private AudioClip _chaseSound;
    [SerializeField] private AudioClip _distractionSound;
    
    private Enemy _enemy;
    private AudioSource _audioSource;
    
    public bool IsDistracted => _isDistracted;
    public float DistractionTimer => _distractionTimer;
    
    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _audioSource = GetComponent<AudioSource>();
        
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        SetupValentinaParameters();
    }
    
    private void Start()
    {
        if (_enemy != null)
        {
            _enemy.PlayerDetected += OnPlayerDetected;
            _enemy.PlayerLost += OnPlayerLost;
            _enemy.AlertedChanged += OnAlertedChanged;
        }
    }
    
    private void Update()
    {
        UpdateDistraction();
    }
    
    private void SetupValentinaParameters()
    {
        if (_enemy != null)
        {
            // Увеличиваем дальность обнаружения
            _enemy.SetDetectionRange(_xrayVisionRange);
            
            // Увеличиваем поле зрения
            _enemy.SetFieldOfView(120f); // Широкое поле зрения
        }
    }
    
    private void UpdateDistraction()
    {
        if (_isDistracted)
        {
            _distractionTimer -= Time.deltaTime;
            
            if (_distractionTimer <= 0f)
            {
                EndDistraction();
            }
        }
    }
    
    public void Distract(Vector3 distractionPoint)
    {
        if (_isDistracted) return;

        var stateMachine = GetComponent<EnemyStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.SetPatrolDestination(distractionPoint);
        }

        StartDistraction("Что-то упало!");
        PlaySound(_distractionSound);
    }
    
    private void StartDistraction(string message)
    {
        _isDistracted = true;
        _distractionTimer = _distractionDuration * _distractionResistance;
        
        Debug.Log($"Кассирша Валентина отвлечена: {message}");
        
        // Временно снижаем бдительность
        if (_enemy != null)
        {
            _enemy.ReduceSuspicion(1f); // Полностью сбрасываем подозрения
        }
    }
    
    private void EndDistraction()
    {
        _isDistracted = false;
        _distractionTimer = 0f;
        Debug.Log("Кассирша Галя снова бдительна!");
    }
    
    private void OnPlayerDetected(Player player)
    {
        PlaySound(_detectionSound);
        Debug.Log("Кассирша Галя видит сквозь одежду! Игрок обнаружен!");
    }
    
    private void OnPlayerLost()
    {
        Debug.Log("Кассирша Галя потеряла игрока из виду");
    }
    
    private void OnAlertedChanged(bool isAlerted)
    {
        if (isAlerted)
        {
            PlaySound(_chaseSound);
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
    
    // Методы для отладки
    private void OnDrawGizmosSelected()
    {
        // X-Ray зрение
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _xrayVisionRange);
        
        // Обычное зрение
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _enemy != null ? _enemy.DetectionRange : 10f);
    }
    
    private void OnDestroy()
    {
        if (_enemy != null)
        {
            _enemy.PlayerDetected -= OnPlayerDetected;
            _enemy.PlayerLost -= OnPlayerLost;
            _enemy.AlertedChanged -= OnAlertedChanged;
        }
    }
    
    [System.Obsolete("Используйте новый метод Distract(Vector3)")]
    public void DistractWithMoney()
    {
        if (!_isDistracted)
        {
            StartDistraction("Подброшен рубль!");
            PlaySound(_distractionSound);
        }
    }
    
    [System.Obsolete("Используйте новый метод Distract(Vector3)")]
    public void DistractWithSale()
    {
        if (!_isDistracted)
        {
            StartDistraction("Акция на сгущёнку!");
            PlaySound(_distractionSound);
        }
    }
} 