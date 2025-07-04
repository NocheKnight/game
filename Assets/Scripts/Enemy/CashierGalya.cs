using UnityEngine;

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
    
    private AudioSource _audioSource;
    
    public bool IsDistracted => _isDistracted;
    public float DistractionTimer => _distractionTimer;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        SetupValentinaParameters();
    }
    
    private void Start()
    {
        // Удалить все обращения к GetComponent<Enemy>()
    }
    
    private void Update()
    {
        UpdateDistraction();
    }
    
    private void SetupValentinaParameters()
    {
        // Увеличиваем дальность обнаружения
        // Удалить все обращения к GetComponent<Enemy>()
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

        // Удалить все обращения к GetComponent<EnemyStateMachine>()

        StartDistraction("Что-то упало!");
        PlaySound(_distractionSound);
    }
    
    private void StartDistraction(string message)
    {
        _isDistracted = true;
        _distractionTimer = _distractionDuration * _distractionResistance;
        
        // Временно снижаем бдительность
        // Удалить все обращения к GetComponent<Enemy>()
    }
    
    private void EndDistraction()
    {
        _isDistracted = false;
        _distractionTimer = 0f;
    }
    
    private void OnPlayerDetected(Player player)
    {
        PlaySound(_detectionSound);
    }
    
    private void OnPlayerLost()
    {
        // Кассирша потеряла игрока
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
        // Gizmos.DrawWireSphere(transform.position, _enemy != null ? _enemy.DetectionRange : 10f);
    }
    
    private void OnDestroy()
    {
        // Удалить все обращения к GetComponent<Enemy>()
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