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
    
    public void ThrowMoney()
    {
        if (!_canThrowMoney || _player.Money < _moneyThrowCost)
            return;
        
        _player.SpendMoney(_moneyThrowCost);
        
        // Создаем "приманку" в точке перед игроком
        Vector3 distractionPoint = transform.position + transform.forward * 2f;
        CreateDistraction(distractionPoint, _moneyThrowRange);

        PlaySound(_moneyThrowSound);
        Debug.Log("Подброшен рубль!");

        _canThrowMoney = false;
        _moneyCooldownTimer = _distractionCooldown;
        MoneyThrown?.Invoke();
    }
    
    public void ShoutAboutSale()
    {
        if (!_canShout)
            return;

        // Находим ближайшую полку к игроку (или можно выбрать любую по логике акции)
        Shelf[] shelves = FindObjectsOfType<Shelf>();
        Shelf nearest = null;
        float minDist = float.MaxValue;
        foreach (var shelf in shelves)
        {
            float dist = Vector3.Distance(transform.position, shelf.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = shelf;
            }
        }
        if (nearest != null)
        {
            // Смещение promoPoint на 1.5 метра от полки в сторону игрока
            Vector3 dir = (transform.position - nearest.transform.position).normalized;
            Vector3 promoPoint = nearest.transform.position + dir * 1.5f;
            // (Опционально) Добавить небольшой разброс
            promoPoint += new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
            Customer.AnnouncePromo(promoPoint, 6f);
        }

        CreateDistraction(transform.position, _shoutRange);
        PlaySound(_shoutSound);
        Debug.Log("Крик о акции!");
        
        _canShout = false;
        _shoutCooldownTimer = _distractionCooldown;
        ShoutUsed?.Invoke();
    }
    
    private void CreateDistraction(Vector3 position, float radius)
    {
        GameObject distractionObject = new GameObject("DistractionZone");
        distractionObject.transform.position = position;
        
        SphereCollider collider = distractionObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = radius;

        // Добавим скрипт, который будет оповещать врагов
        DistractionZone zone = distractionObject.AddComponent<DistractionZone>();
        zone.Initialize(radius);
        
        // Уничтожаем объект через пару секунд
        Destroy(distractionObject, 2f);
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

/// <summary>
/// Простой компонент для зоны отвлечения, который ищет врагов при создании.
/// В будущем можно сделать его более сложным (например, чтобы враги реагировали на вход в триггер).
/// </summary>
public class DistractionZone : MonoBehaviour
{
    public void Initialize(float radius)
    {
        // Вместо FindObjectsOfType, мы делаем один OverlapSphere, что намного эффективнее.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var hitCollider in hitColliders)
        {
            // Пытаемся получить компонент врага. В будущем здесь может быть общий интерфейс IDistractable
            if (hitCollider.TryGetComponent<CashierGalya>(out var cashier))
            {
                // Говорим врагу, куда идти
                cashier.Distract(transform.position); 
            }
        }
    }
} 