using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CashRegister : MonoBehaviour
{
    [Header("Настройки кассы")]
    [SerializeField] private string _registerName = "Касса";
    [SerializeField] private float _interactionRange = 2f;
    [SerializeField] private bool _isPlayerInRange = false;
    [SerializeField] private bool _isTransactionActive = false;
    
    [Header("Мини-игра")]
    [SerializeField] private float _minigameDuration = 5f;
    [SerializeField] private float _successThreshold = 0.7f;
    [SerializeField] private int _maxItemsPerTransaction = 5;
    
    [Header("Визуальные настройки")]
    [SerializeField] private GameObject _registerVisual;
    [SerializeField] private Material _normalMaterial;
    [SerializeField] private Material _highlightMaterial;
    [SerializeField] private Material _activeMaterial;
    
    [Header("UI элементы")]
    [SerializeField] private GameObject _minigameUI;
    [SerializeField] private UnityEngine.UI.Slider _progressSlider;
    [SerializeField] private UnityEngine.UI.Text _timerText;
    [SerializeField] private UnityEngine.UI.Text _itemCountText;
    
    public string RegisterName => _registerName;
    public bool IsPlayerInRange => _isPlayerInRange;
    public bool IsTransactionActive => _isTransactionActive;
    
    public event UnityAction<CashRegister> TransactionStarted;
    public event UnityAction<CashRegister, bool, int> TransactionCompleted;
    public event UnityAction<bool> PlayerInRangeChanged;
    
    private Player _player;
    private PlayerInventory _playerInventory;
    private Renderer _registerRenderer;
    private Material _originalMaterial;
    
    private Coroutine _transactionCoroutine;
    private List<Goods> _itemsToSell = new List<Goods>();
    private float _transactionProgress = 0f;
    private float _transactionTimer = 0f;
    
    private void Awake()
    {
        _registerRenderer = GetComponent<Renderer>();
        if (_registerRenderer != null)
        {
            _originalMaterial = _registerRenderer.material;
        }
    }
    
    private void Start()
    {
        _player = FindObjectOfType<Player>();
        if (_player != null)
        {
            _playerInventory = _player.GetComponent<PlayerInventory>();
        }
        
        if (_minigameUI != null)
        {
            _minigameUI.SetActive(false);
        }
    }
    
    private void Update()
    {
        CheckPlayerDistance();
        HandleInput();
        UpdateMinigameUI();
    }
    
    private void CheckPlayerDistance()
    {
        if (_player == null) return;
        
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        bool wasInRange = _isPlayerInRange;
        _isPlayerInRange = distance <= _interactionRange;
        
        if (wasInRange != _isPlayerInRange)
        {
            PlayerInRangeChanged?.Invoke(_isPlayerInRange);
            UpdateVisuals();
        }
    }
    
    private void HandleInput()
    {
        if (!_isPlayerInRange || _isTransactionActive) return;
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartTransaction();
        }
    }
    
    public void StartTransaction()
    {
        if (_isTransactionActive || _playerInventory == null) return;
        
        _itemsToSell = _playerInventory.GetStolenItems();
        
        if (_itemsToSell.Count == 0)
        {
            Debug.Log("Нет украденных товаров для продажи!");
            return;
        }
        
        if (_itemsToSell.Count > _maxItemsPerTransaction)
        {
            _itemsToSell = _itemsToSell.GetRange(0, _maxItemsPerTransaction);
        }
        
        _isTransactionActive = true;
        _transactionProgress = 0f;
        _transactionTimer = 0f;
        
        if (_minigameUI != null)
        {
            _minigameUI.SetActive(true);
        }
        
        _transactionCoroutine = StartCoroutine(TransactionMinigame());
        
        TransactionStarted?.Invoke(this);
        UpdateVisuals();
        
        Debug.Log($"Начата транзакция с {_itemsToSell.Count} товарами");
    }
    
    private IEnumerator TransactionMinigame()
    {
        float startTime = Time.time;
        
        while (_transactionTimer < _minigameDuration)
        {
            _transactionTimer = Time.time - startTime;
            
            if (Input.GetKey(KeyCode.Space))
            {
                _transactionProgress += Time.deltaTime * 2f;
            }
            else
            {
                _transactionProgress += Time.deltaTime * 0.5f;
            }
            
            _transactionProgress = Mathf.Clamp01(_transactionProgress);
            
            yield return null;
        }
        
        CompleteTransaction();
    }
    
    private void CompleteTransaction()
    {
        bool success = _transactionProgress >= _successThreshold;
        int totalValue = 0;
        
        if (success)
        {
            foreach (var item in _itemsToSell)
            {
                totalValue += item.Price;
            }
            
            _player.AddMoney(totalValue);
            
            foreach (var item in _itemsToSell)
            {
                _playerInventory.RemoveItem(item);
            }
            
            Debug.Log($"Транзакция успешна! Продано товаров на {totalValue}₽");
        }
        else
        {
            Debug.Log("Транзакция провалена! Товары не проданы.");
        }
        
        if (_minigameUI != null)
        {
            _minigameUI.SetActive(false);
        }
        
        _isTransactionActive = false;
        _itemsToSell.Clear();
        
        if (_transactionCoroutine != null)
        {
            StopCoroutine(_transactionCoroutine);
            _transactionCoroutine = null;
        }
        
        TransactionCompleted?.Invoke(this, success, totalValue);
        UpdateVisuals();
    }
    
    private void UpdateMinigameUI()
    {
        if (!_isTransactionActive || _minigameUI == null) return;
        
        if (_progressSlider != null)
        {
            _progressSlider.value = _transactionProgress;
        }
        
        // Обновляем таймер
        if (_timerText != null)
        {
            float remainingTime = _minigameDuration - _transactionTimer;
            _timerText.text = $"Время: {remainingTime:F1}с";
        }
        
        // Обновляем количество товаров
        if (_itemCountText != null)
        {
            _itemCountText.text = $"Товаров: {_itemsToSell.Count}";
        }
    }
    
    private void UpdateVisuals()
    {
        if (_registerRenderer == null) return;
        
        if (_isTransactionActive)
        {
            _registerRenderer.material = _activeMaterial != null ? _activeMaterial : _originalMaterial;
        }
        else if (_isPlayerInRange)
        {
            _registerRenderer.material = _highlightMaterial != null ? _highlightMaterial : _originalMaterial;
        }
        else
        {
            _registerRenderer.material = _originalMaterial;
        }
    }
    
    public void CancelTransaction()
    {
        if (!_isTransactionActive) return;
        
        if (_transactionCoroutine != null)
        {
            StopCoroutine(_transactionCoroutine);
            _transactionCoroutine = null;
        }
        
        _isTransactionActive = false;
        _itemsToSell.Clear();
        
        if (_minigameUI != null)
        {
            _minigameUI.SetActive(false);
        }
        
        UpdateVisuals();
        Debug.Log("Транзакция отменена");
    }
    
    // Методы для отладки
    private void OnDrawGizmosSelected()
    {
        // Радиус взаимодействия
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _interactionRange);
    }
    
    private void OnDrawGizmos()
    {
        // Показываем состояние кассы
        if (_isTransactionActive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, Vector3.one * 0.3f);
        }
        else if (_isPlayerInRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, Vector3.one * 0.2f);
        }
    }
} 