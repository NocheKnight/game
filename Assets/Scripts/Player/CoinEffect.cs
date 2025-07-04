using UnityEngine;

public class CoinEffect : MonoBehaviour
{
    [Header("Настройки монетки")]
    [SerializeField] private float _rotationSpeed = 360f;
    [SerializeField] private float _bobHeight = 0.1f;
    [SerializeField] private float _bobSpeed = 2f;
    
    [Header("Частицы")]
    [SerializeField] private ParticleSystem _sparkleParticles;
    [SerializeField] private ParticleSystem _landingParticles;
    
    [Header("Звуки")]
    [SerializeField] private AudioClip _coinSound;
    [SerializeField] private AudioClip _landingSound;
    
    private AudioSource _audioSource;
    private Vector3 _startPosition;
    private float _bobTimer = 0f;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        _startPosition = transform.position;
    }
    
    private void Start()
    {
        // Воспроизводим звук появления
        if (_coinSound != null)
        {
            _audioSource.PlayOneShot(_coinSound);
        }
        
        // Запускаем эффект блеска
        if (_sparkleParticles != null)
        {
            _sparkleParticles.Play();
        }
    }
    
    private void Update()
    {
        // Вращение монетки
        transform.Rotate(Vector3.forward, _rotationSpeed * Time.deltaTime);
        
        // Покачивание вверх-вниз
        _bobTimer += Time.deltaTime * _bobSpeed;
        float bobOffset = Mathf.Sin(_bobTimer) * _bobHeight;
        transform.position = _startPosition + Vector3.up * bobOffset;
    }
    
    public void PlayLandingEffect()
    {
        // Эффект приземления
        if (_landingParticles != null)
        {
            _landingParticles.Play();
        }
        
        // Звук приземления
        if (_landingSound != null)
        {
            _audioSource.PlayOneShot(_landingSound);
        }
    }
    
    private void OnDestroy()
    {
        // Останавливаем все частицы
        if (_sparkleParticles != null)
        {
            _sparkleParticles.Stop();
        }
        
        if (_landingParticles != null)
        {
            _landingParticles.Stop();
        }
    }
} 