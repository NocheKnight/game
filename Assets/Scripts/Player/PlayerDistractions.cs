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
    [SerializeField] private AudioClip _coinDropSound;
    
    [Header("Визуальные эффекты")]
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private ParticleSystem _coinSparkleEffect;
    [SerializeField] private float _coinFallHeight = 2f;
    [SerializeField] private float _coinFallDuration = 1f;
    
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
        
        Vector3 distractionPoint = transform.position + transform.forward * 2f;
        CreateDistraction(distractionPoint, _moneyThrowRange);
        CreateCoinEffect(distractionPoint);

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
        
        Vector3 promoPoint = FindPromoPoint();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _shoutRange);
        
        int customersAlerted = 0;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<Customer>(out var customer))
            {
                customer.AnnouncePromo(promoPoint);
                customersAlerted++;
            }
        }

        CreateDistraction(transform.position, _shoutRange);
        
            PlaySound(_shoutSound);
        Debug.Log($"Крик об акции! Откликнулось покупателей: {customersAlerted}");
        
        _canShout = false;
        _shoutCooldownTimer = _distractionCooldown;
        ShoutUsed?.Invoke();
    }
    
    private Vector3 FindPromoPoint()
    {
        Shelf[] shelves = FindObjectsOfType<Shelf>();
        if (shelves.Length == 0)
        {
            return transform.position + transform.forward * 5f;
        }

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
        
        Vector3 dir = (transform.position - nearest.transform.position).normalized;
        Vector3 promoPoint = nearest.transform.position + dir * 1.5f;
        promoPoint += new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        
        return promoPoint;
    }
    
    private void CreateDistraction(Vector3 position, float radius)
        {
        GameObject distractionObject = new GameObject("DistractionZone");
        distractionObject.transform.position = position;
        
        SphereCollider collider = distractionObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = radius;

        DistractionZone zone = distractionObject.AddComponent<DistractionZone>();
        zone.Initialize(radius);
        
        Destroy(distractionObject, 2f);
    }
    
    private void CreateCoinEffect(Vector3 targetPosition)
    {
        if (_coinPrefab != null)
        {
            GameObject coin = Instantiate(_coinPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            StartCoroutine(AnimateCoinFall(coin, targetPosition));
        }
        
        if (_coinSparkleEffect != null)
        {
            ParticleSystem sparkle = Instantiate(_coinSparkleEffect, targetPosition, Quaternion.identity);
            Destroy(sparkle.gameObject, 3f);
        }
        
        PlaySound(_coinDropSound);
    }
    
    private IEnumerator AnimateCoinFall(GameObject coin, Vector3 targetPosition)
    {
        Vector3 startPosition = coin.transform.position;
        Vector3 endPosition = targetPosition;
        float elapsedTime = 0f;
            
        while (elapsedTime < _coinFallDuration)
            {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / _coinFallDuration;
            
            float height = _coinFallHeight * Mathf.Sin(progress * Mathf.PI);
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, progress);
            currentPosition.y = Mathf.Lerp(startPosition.y, endPosition.y, progress) + height;
            
            coin.transform.position = currentPosition;
            coin.transform.Rotate(Vector3.forward, 360f * Time.deltaTime);
                
                yield return null;
            }
            
        coin.transform.position = endPosition;
        CreateLandingEffect(endPosition);
        Destroy(coin, 2f);
        }
        
    private void CreateLandingEffect(Vector3 position)
    {
        if (_coinSparkleEffect != null)
        {
            ParticleSystem landingEffect = Instantiate(_coinSparkleEffect, position, Quaternion.identity);
            var main = landingEffect.main;
            main.startSpeed = 3f;
            main.startLifetime = 0.5f;
            main.startSize = 0.1f;
            
            Destroy(landingEffect.gameObject, 1f);
        }
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
        if (money < _moneyThrowCost)
        {
            _canThrowMoney = false;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _moneyThrowRange);
        
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

public class DistractionZone : MonoBehaviour
{
    public void Initialize(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var hitCollider in hitColliders)
        {
            // Реакция кассирши на отвлечение
            if (hitCollider.TryGetComponent<CashierGalya>(out var cashier))
            {
                cashier.Distract(transform.position); 
            }
            
            // Реакция покупателей на монетку
            if (hitCollider.TryGetComponent<Customer>(out var customer))
            {
                customer.AnnouncePromo(transform.position);
            }
            
            // Реакция охранников на отвлечение
            if (hitCollider.TryGetComponent<SecurityGuard>(out var guard))
            {
                guard.Distract(transform.position);
            }
            
            // Реакция через GuardLogic (если используется)
            if (hitCollider.TryGetComponent<GuardLogic>(out var guardLogic))
            {
                guardLogic.AddSuspicion(10f, transform.position);
            }
        }
    }
} 