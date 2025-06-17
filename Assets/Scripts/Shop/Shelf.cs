using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Shelf : MonoBehaviour
{
    [Header("Настройки полки")]
    [SerializeField] private string _shelfName = "Полка";
    [SerializeField] private int _maxItems = 10;
    [SerializeField] private float _interactionRange = 2f;
    [SerializeField] private bool _isLocked = false;
    [SerializeField] private int _lockPickDifficulty = 3;
    
    [Header("Товары")]
    [SerializeField] private List<Goods> _availableGoods = new List<Goods>();
    
    [Header("Визуальные настройки")]
    [SerializeField] private GameObject _shelfVisual;
    [SerializeField] private Transform _itemSpawnPoint;
    [SerializeField] private Material _normalMaterial;
    [SerializeField] private Material _highlightMaterial;
    
    [Header("Визуальные товары")]
    [SerializeField] private List<ShelfItemVisual> _visualItems = new List<ShelfItemVisual>();
    [SerializeField] private Transform _visualItemsParent;
    [SerializeField] private Vector3 _itemSpacing = new Vector3(0.5f, 0f, 0.5f);
    [SerializeField] private int _itemsPerRow = 3;
    
    [Header("Состояние")]
    [SerializeField] private bool _isPlayerInRange = false;
    [SerializeField] private bool _isBeingRobbed = false;
    
    public string ShelfName => _shelfName;
    public int MaxItems => _maxItems;
    public int CurrentItemsCount => _visualItems.Count;
    public bool IsLocked => _isLocked;
    public bool IsPlayerInRange => _isPlayerInRange;
    public bool IsBeingRobbed => _isBeingRobbed;
    public List<Goods> CurrentItems 
    { 
        get 
        {
            List<Goods> items = new List<Goods>();
            foreach (var visualItem in _visualItems)
            {
                if (visualItem != null && !visualItem.IsEmpty)
                {
                    items.Add(visualItem.Goods);
                }
            }
            return items;
        }
    }
    
    public event UnityAction<Goods> ItemStolen;
    public event UnityAction<Shelf> ShelfEmptied;
    public event UnityAction<bool> PlayerInRangeChanged;
    public event UnityAction<bool> BeingRobbedChanged;
    
    private Player _player;
    private PlayerInventory _playerInventory;
    private Renderer _shelfRenderer;
    private Material _originalMaterial;
    
    private void Awake()
    {
        _shelfRenderer = GetComponent<Renderer>();
        if (_shelfRenderer != null)
        {
            _originalMaterial = _shelfRenderer.material;
        }
        
        if (_visualItemsParent == null)
        {
            GameObject visualParent = new GameObject("VisualItems");
            visualParent.transform.SetParent(transform);
            visualParent.transform.localPosition = Vector3.zero;
            _visualItemsParent = visualParent.transform;
        }
    }
    
    private void Start()
    {
        _player = FindObjectOfType<Player>();
        if (_player != null)
        {
            _playerInventory = _player.GetComponent<PlayerInventory>();
        }
    }
    
    private void Update()
    {
        CheckPlayerDistance();
        HandleInput();
    }
    
    private void InitializeShelf()
    {
        if (_availableGoods.Count == 0) return;
        
        ClearVisualItems();
        
        int itemsToAdd = Random.Range(3, _maxItems + 1);
        
        for (int i = 0; i < itemsToAdd; i++)
        {
            Goods randomGood = _availableGoods[Random.Range(0, _availableGoods.Count)];
            
            CreateVisualItem(randomGood, i);
        }
        
        Debug.Log($"Полка '{_shelfName}' инициализирована с {_visualItems.Count} товарами");
    }
    
    private void CreateVisualItem(Goods goods, int index)
    {
        if (goods == null || goods.Prefab == null) return;
        
        Vector3 position = CalculateItemPosition(index);
        
        GameObject visualItemObj = new GameObject($"VisualItem_{index}");
        visualItemObj.transform.SetParent(_visualItemsParent);
        visualItemObj.transform.localPosition = position;
        
        ShelfItemVisual visualItem = visualItemObj.AddComponent<ShelfItemVisual>();
        visualItem.Initialize(goods);
        
        _visualItems.Add(visualItem);
    }
    
    private Vector3 CalculateItemPosition(int index)
    {
        int row = index / _itemsPerRow;
        int col = index % _itemsPerRow;
        
        Vector3 position = Vector3.zero;
        position.x = col * _itemSpacing.x;
        position.z = row * _itemSpacing.z;
        position.y = 0.5f; // Высота над полкой
        
        return position;
    }
    
    private void ClearVisualItems()
    {
        foreach (var visualItem in _visualItems)
        {
            if (visualItem != null)
            {
                DestroyImmediate(visualItem.gameObject);
            }
        }
        _visualItems.Clear();
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
        if (!_isPlayerInRange || _visualItems.Count == 0) return;
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            TryStealAllItems();
        }
    }
    
    public bool TryStealAllItems()
    {
        if (_visualItems.Count == 0 || _playerInventory == null) return false;
        
        int stolenCount = 0;
        List<ShelfItemVisual> visualItemsToRemove = new List<ShelfItemVisual>();
        
        foreach (var visualItem in _visualItems)
        {
            if (visualItem == null || visualItem.IsEmpty || visualItem.IsBeingPickedUp) continue;
            
            Goods goods = visualItem.Goods;
            if (goods == null) continue;
            
            if (_playerInventory.CanAddItem(goods) && CheckStealingSuccess(goods))
            {
                _playerInventory.TryAddItem(goods);
                visualItem.PickupItem();
                visualItemsToRemove.Add(visualItem);
                stolenCount++;
                
                _player.AddCrimeRate(goods.StealingDifficulty * 5);
                
                ItemStolen?.Invoke(goods);
            }
        }
        
        foreach (var visualItem in visualItemsToRemove)
        {
            _visualItems.Remove(visualItem);
        }
        
        if (stolenCount > 0)
        {
            Debug.Log($"Украдено товаров: {stolenCount}!");
            
            if (_visualItems.Count == 0)
            {
                ShelfEmptied?.Invoke(this);
                SetBeingRobbed(false);
            }
            
            return true;
        }
        
        return false;
    }
    
    private bool CheckStealingSuccess(Goods item)
    {
        // Базовая вероятность успеха зависит от сложности кражи
        float baseChance = 1f - (item.StealingDifficulty * 0.1f);
        
        // Бонусы от навыков игрока
        if (_player != null)
        {
            baseChance += _player.GetStealthBonus();
            baseChance += _player.GetPickpocketChance() * 0.5f;
        }
        
        baseChance -= item.GetStealthPenalty();
        
        // Ограничиваем вероятность
        baseChance = Mathf.Clamp01(baseChance);
        
        return Random.value <= baseChance;
    }
    
    private void UpdateVisuals()
    {
        if (_shelfRenderer == null) return;
        
        if (_isPlayerInRange && _visualItems.Count > 0)
        {
            _shelfRenderer.material = _highlightMaterial != null ? _highlightMaterial : _originalMaterial;
        }
        else
        {
            _shelfRenderer.material = _originalMaterial;
        }
    }
    
    private void SetBeingRobbed(bool isBeingRobbed)
    {
        if (_isBeingRobbed != isBeingRobbed)
        {
            _isBeingRobbed = isBeingRobbed;
            BeingRobbedChanged?.Invoke(_isBeingRobbed);
        }
    }
    
    public void RefillShelf()
    {
        if (_availableGoods.Count == 0) return;
        
        ClearVisualItems();
        
        int itemsToAdd = Random.Range(3, _maxItems + 1);
        
        for (int i = 0; i < itemsToAdd; i++)
        {
            Goods randomGood = _availableGoods[Random.Range(0, _availableGoods.Count)];
            
            CreateVisualItem(randomGood, i);
        }
        
        Debug.Log($"Полка '{_shelfName}' пополнена. Товаров: {_visualItems.Count}");
    }
    
    public void SetAvailableGoods(List<Goods> goods)
    {
        _availableGoods = goods;
        
        InitializeShelf();
    }
    
    // Методы для отладки
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _interactionRange);
        
        if (_itemSpawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_itemSpawnPoint.position, 0.1f);
        }
        
        Gizmos.color = Color.yellow;
        for (int i = 0; i < _maxItems; i++)
        {
            Vector3 pos = CalculateItemPosition(i);
            Gizmos.DrawWireCube(transform.position + pos, Vector3.one * 0.3f);
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
} 