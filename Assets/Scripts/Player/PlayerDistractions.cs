using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerMover))]
[RequireComponent(typeof(AudioSource))]
public class PlayerDistractions : MonoBehaviour
{
    [Header("Настройки отвлечений")]
    [SerializeField] private float _moneyThrowRange = 5f;
    [SerializeField] private float _shoutRange = 8f;
    [SerializeField] private int _moneyThrowCost = 1;
    [SerializeField] private float _distractionCooldown = 3f;
    
    [Header("Эффекты")]
    [SerializeField] private AudioClip _moneyThrowSound;
    [SerializeField] private AudioClip _shoutSound;
    
    [Header("Состояние")]
    [SerializeField] private bool _canThrowMoney = true;
    [SerializeField] private bool _canShout = true;
    [SerializeField] private float _moneyCooldownTimer = 0f;
    [SerializeField] private float _shoutCooldownTimer = 0f;
    
    public bool CanThrowMoney => _canThrowMoney;
    public bool CanShout => _canShout;
    public float MoneyCooldownTimer => _moneyCooldownTimer;
    public float ShoutCooldownTimer => _shoutCooldownTimer;
    
    public event UnityAction MoneyThrown;
    public event UnityAction ShoutUsed;
    public event UnityAction<float> MoneyCooldownChanged;
    public event UnityAction<float> ShoutCooldownChanged;
    
    private Player _player;
    private PlayerMover _playerMover;
    private AudioSource _audioSource;
    private Camera _playerCamera;
    
    private void Awake()
    {
        _player = GetComponent<Player>();
        _playerMover = GetComponent<PlayerMover>();
        _audioSource = GetComponent<AudioSource>();
        
        _playerCamera = Camera.main;
    }
    
    private void Start()
    {
        // Подписываемся на события игрока
        if (_player != null)
        {
            _player.MoneyChanged += OnMoneyChanged;
        }
    }
    
    private void Update()
    {
        UpdateCooldowns();
        HandleInput();
    }
    
    private void UpdateCooldowns()
    {
        // Обновляем кулдаун подбрасывания денег
        if (!_canThrowMoney)
        {
            _moneyCooldownTimer -= Time.deltaTime;
            MoneyCooldownChanged?.Invoke(_moneyCooldownTimer);
            
            if (_moneyCooldownTimer <= 0f)
            {
                _canThrowMoney = true;
                _moneyCooldownTimer = 0f;
            }
        }
        
        // Обновляем кулдаун крика
        if (!_canShout)
        {
            _shoutCooldownTimer -= Time.deltaTime;
            ShoutCooldownChanged?.Invoke(_shoutCooldownTimer);
            
            if (_shoutCooldownTimer <= 0f)
            {
                _canShout = true;
                _shoutCooldownTimer = 0f;
            }
        }
    }
    
    private void HandleInput()
    {
        // Подбросить рубль (клавиша E)
        if (Input.GetKeyDown(KeyCode.E) && _canThrowMoney)
        {
            ThrowMoney();
        }
        
        // Крикнуть про акцию (клавиша Q)
        if (Input.GetKeyDown(KeyCode.Q) && _canShout)
        {
            ShoutAboutSale();
        }
    }
    
    public void ThrowMoney()
    {
        if (!_canThrowMoney || _player.Money < _moneyThrowCost)
            return;
        
        // Тратим деньги
        _player.SpendMoney(_moneyThrowCost);
        
        // Находим врагов в радиусе
        CashierGalya[] cashiers = FindObjectsOfType<CashierGalya>();
        bool distractedAny = false;
        
        foreach (var cashier in cashiers)
        {
            float distance = Vector3.Distance(transform.position, cashier.transform.position);
            if (distance <= _moneyThrowRange)
            {
                cashier.DistractWithMoney();
                distractedAny = true;
            }
        }
        
        if (distractedAny)
        {
            // Создаем эффект подброшенных денег
            CreateMoneyEffect();
            PlaySound(_moneyThrowSound);
            
            Debug.Log("Подброшен рубль! Враги отвлечены.");
        }
        else
        {
            Debug.Log("Нет врагов в радиусе для отвлечения.");
        }
        
        // Устанавливаем кулдаун
        _canThrowMoney = false;
        _moneyCooldownTimer = _distractionCooldown;
        MoneyThrown?.Invoke();
    }
    
    public void ShoutAboutSale()
    {
        if (!_canShout)
            return;
        
        // Находим врагов в радиусе
        CashierGalya[] cashiers = FindObjectsOfType<CashierGalya>();
        bool distractedAny = false;
        
        foreach (var cashier in cashiers)
        {
            float distance = Vector3.Distance(transform.position, cashier.transform.position);
            if (distance <= _shoutRange)
            {
                cashier.DistractWithSale();
                distractedAny = true;
            }
        }
        
        if (distractedAny)
        {
            // Создаем эффект крика
            CreateShoutEffect();
            PlaySound(_shoutSound);
            
            Debug.Log("Крик о акции! Враги отвлечены.");
        }
        else
        {
            Debug.Log("Нет врагов в радиусе для отвлечения.");
        }
        
        // Устанавливаем кулдаун
        _canShout = false;
        _shoutCooldownTimer = _distractionCooldown;
        ShoutUsed?.Invoke();
    }
    
    private void CreateMoneyEffect()
    {
        // Создаем простой эффект
        GameObject effect = new GameObject("MoneyEffect");
        effect.transform.position = transform.position + transform.forward * 2f;
        
        // Добавляем простую анимацию
        StartCoroutine(AnimateMoneyEffect(effect));
    }
    
    private void CreateShoutEffect()
    {
        // Создаем простой эффект
        GameObject effect = new GameObject("ShoutEffect");
        effect.transform.position = transform.position + Vector3.up * 1.5f;
        
        // Добавляем простую анимацию
        StartCoroutine(AnimateShoutEffect(effect));
    }
    
    private IEnumerator AnimateMoneyEffect(GameObject effect)
    {
        // Простая анимация разлетающихся монет
        for (int i = 0; i < 5; i++)
        {
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.transform.SetParent(effect.transform);
            coin.transform.localScale = new Vector3(0.1f, 0.05f, 0.1f);
            
            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.y = Mathf.Abs(randomDirection.y); // Всегда вверх
            
            Rigidbody coinRb = coin.AddComponent<Rigidbody>();
            coinRb.AddForce(randomDirection * 3f, ForceMode.Impulse);
            
            // Удаляем монету через 2 секунды
            Destroy(coin, 2f);
            
            yield return new WaitForSeconds(0.1f);
        }
        
        // Удаляем эффект через 3 секунды
        Destroy(effect, 3f);
    }
    
    private IEnumerator AnimateShoutEffect(GameObject effect)
    {
        // Простая анимация звуковой волны
        for (int i = 0; i < 3; i++)
        {
            GameObject wave = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            wave.transform.SetParent(effect.transform);
            wave.transform.localScale = Vector3.one * (i + 1) * 0.5f;
            
            Material waveMaterial = new Material(Shader.Find("Standard"));
            waveMaterial.color = new Color(1f, 1f, 1f, 0.3f);
            wave.GetComponent<Renderer>().material = waveMaterial;
            
            // Анимация расширения
            float duration = 1f;
            float elapsed = 0f;
            Vector3 startScale = wave.transform.localScale;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                
                wave.transform.localScale = startScale * (1f + progress);
                waveMaterial.color = new Color(1f, 1f, 1f, 0.3f * (1f - progress));
                
                yield return null;
            }
            
            Destroy(wave);
        }
        
        // Удаляем эффект
        Destroy(effect, 1f);
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
    
    private void OnMoneyChanged(int money)
    {
        // Проверяем, можем ли мы подбрасывать деньги
        if (money < _moneyThrowCost)
        {
            _canThrowMoney = false;
        }
    }
    
    // Методы для отладки
    private void OnDrawGizmosSelected()
    {
        // Радиус подбрасывания денег
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _moneyThrowRange);
        
        // Радиус крика
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _shoutRange);
    }
    
    private void OnDestroy()
    {
        if (_player != null)
        {
            _player.MoneyChanged -= OnMoneyChanged;
        }
    }
} 