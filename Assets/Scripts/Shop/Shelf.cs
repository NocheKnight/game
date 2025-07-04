using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Shelf : MonoBehaviour
{
    [Header("Настройки полки")]
    [SerializeField] private string _shelfName = "Полка";
    [SerializeField] private bool _isLocked = false;
    
    private List<StorableItem> _itemsOnShelf = new List<StorableItem>();

    [Header("Визуальные настройки")]
    [SerializeField] private Material _highlightMaterial;
    
    public string ShelfName => _shelfName;
    public int CurrentItemsCount => _itemsOnShelf.Count;
    public bool IsLocked => _isLocked;
    public List<Goods> CurrentItems => _itemsOnShelf.Select(item => item.Goods).ToList();
    
    public event UnityAction<Goods> ItemStolen;
    public event UnityAction<Shelf> ShelfEmptied;
    
    private Player _player;
    private Renderer _shelfRenderer;
    private Material _originalMaterial;
    
    private void Awake()
    {
        _shelfRenderer = GetComponent<Renderer>();
        if (_shelfRenderer != null)
        {
            _originalMaterial = _shelfRenderer.material;
        }
    }
    
    private void Start()
    {
        _player = FindObjectOfType<Player>();
        InitializeShelf();
    }
    
    private void Update()
    {
        // Можно оставить пустым или проверять расстояние для других целей,
        // но для подсветки и взаимодействия оно больше не нужно.
    }
    
    private void InitializeShelf()
    {
        ClearShelf();
        
        ItemSpawnPoint[] spawnPoints = GetComponentsInChildren<ItemSpawnPoint>();

        foreach (var point in spawnPoints)
        {
            if (point.Goods != null)
            {
                CreateItemModel(point);
            }
        }
        
        Debug.Log($"Полка '{_shelfName}' инициализирована с {CurrentItemsCount} товарами");
    }

    private void CreateItemModel(ItemSpawnPoint point)
    {
        if (point.Goods.Prefab == null) return;
        
        GameObject model = Instantiate(point.Goods.Prefab, point.transform.position, point.transform.rotation, transform);
        
        StorableItem storableItem = model.AddComponent<StorableItem>();
        storableItem.Initialize(point.Goods, this, _highlightMaterial);
        
        _itemsOnShelf.Add(storableItem);

        point.gameObject.SetActive(false);
    }
    
    private void ClearShelf()
    {
        foreach (var item in _itemsOnShelf)
        {
            if(item != null) Destroy(item.gameObject);
        }
        _itemsOnShelf.Clear();

        ItemSpawnPoint[] spawnPoints = GetComponentsInChildren<ItemSpawnPoint>(true);
        foreach (var point in spawnPoints)
        {
            point.gameObject.SetActive(true);
        }
    }
    
    private void CheckPlayerDistance()
    {
        // Эта проверка больше не нужна для основной логики взаимодействия
    }

    // D:/unity/projects/Kradylechka/Assets/Scripts/Shop/Shelf.cs

    public bool TryStealItem(StorableItem itemToSteal, Player player)
    {
        if (itemToSteal == null || !_itemsOnShelf.Contains(itemToSteal) || player == null) return false;

        var playerInventory = player.GetComponent<PlayerInventory>();
        if (playerInventory == null || !playerInventory.CanAddItem(itemToSteal.Goods))
        {
            Debug.Log("Инвентарь полон или перегружен!");
            return false;
        }

        // <<< ИСПРАВЛЕНО >>>
        // Сообщаем миру о ПОПЫТКЕ кражи. Это заменит оба вызова AlertNearbyCustomers.
        // Теперь любой NPC (покупатель или охранник) сам решит, видел ли он это.
        itemToSteal.OnStolen();

        if (!CheckStealingSuccess(itemToSteal.Goods, player))
        {
            Debug.Log($"Не удалось украсть {itemToSteal.Goods.Label}! Не повезло.");
            // Событие уже отправлено, больше ничего делать не нужно.
            return false;
        }

        playerInventory.TryAddItem(itemToSteal.Goods);
        
        ItemStolen?.Invoke(itemToSteal.Goods);
        
        _itemsOnShelf.Remove(itemToSteal);
        Destroy(itemToSteal.gameObject);

        if (CurrentItemsCount == 0)
        {
            ShelfEmptied?.Invoke(this);
        }
        
        Debug.Log($"Украден товар: {itemToSteal.Goods.Label}!");
        return true;
    }
    
    private bool CheckStealingSuccess(Goods item, Player player)
    {
        float baseChance = 1f - (item.StealingDifficulty * 0.1f);
        
        // TODO: Перенести бонусы в класс Player
        
        float penalty = 0f;
        if (item.IsFragile) penalty += 0.2f;
        if (item.IsValuable) penalty += 0.3f;
        if (item.Weight > 2f) penalty += 0.2f;
        
        baseChance -= penalty;
        
        baseChance = Mathf.Clamp01(baseChance);
        
        return Random.value <= baseChance;
    }
    
    private void UpdateVisuals()
    {
        // Больше не подсвечиваем полку
    }
    
    public void RefillShelf()
    {
        InitializeShelf();
    }
    
    private void OnDrawGizmosSelected()
    {
        // Можно убрать или оставить для отладки, но interactionRange здесь больше нет
    }
} 