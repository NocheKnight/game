using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerMover))]
public class PlayerInventory : MonoBehaviour
{
    [Header("Настройки инвентаря")]
    [SerializeField] private int _maxWeight = 10;
    [SerializeField] private int _currentWeight = 0;
    [SerializeField] private int _maxItems = 20;
    
    [Header("Система веса")]
    [SerializeField] private float _weightPerItem = 1f;
    [SerializeField] private float _fragileItemWeight = 0.5f;
    [SerializeField] private float _heavyItemWeight = 2f;
    
    private List<Goods> _goods = new List<Goods>();
    private Player _player;
    private PlayerMover _mover;
    
    public int MaxWeight => _maxWeight;
    public int CurrentWeight => _currentWeight;
    public int MaxItems => _maxItems;
    public int ItemsCount => _goods.Count;
    public List<Goods> Goods => _goods;
    
    public event UnityAction<int> WeightChanged;
    public event UnityAction<int> ItemsCountChanged;
    public event UnityAction<Goods> ItemAdded;
    public event UnityAction<Goods> ItemRemoved;
    public event UnityAction InventoryFull;
    public event UnityAction InventoryOverloaded;
    
    private void Awake()
    {
        _player = GetComponent<Player>();
        _mover = GetComponent<PlayerMover>();
    }
    
    private void Start()
    {
        UpdateMaxWeight();
        UpdateOverloadStatus();
    }
    
    public bool TryAddItem(Goods item)
    {
        if (item == null)
            return false;
            
        int itemWeight = GetItemWeight(item);
        
        if (_currentWeight + itemWeight > _maxWeight)
        {
            InventoryOverloaded?.Invoke();
            return false;
        }
        
        if (_goods.Count >= _maxItems)
        {
            InventoryFull?.Invoke();
            return false;
        }
        
        _goods.Add(item);
        _currentWeight += itemWeight;
        
        UpdateOverloadStatus();
        WeightChanged?.Invoke(_currentWeight);
        ItemsCountChanged?.Invoke(_goods.Count);
        ItemAdded?.Invoke(item);
        
        return true;
    }
    
    public bool TryRemoveItem(Goods item)
    {
        if (item == null || !_goods.Contains(item))
            return false;
            
        int itemWeight = GetItemWeight(item);
        
        _goods.Remove(item);
        _currentWeight -= itemWeight;
        
        UpdateOverloadStatus();
        WeightChanged?.Invoke(_currentWeight);
        ItemsCountChanged?.Invoke(_goods.Count);
        ItemRemoved?.Invoke(item);
        
        return true;
    }
    
    public bool TryRemoveItemByIndex(int index)
    {
        if (index < 0 || index >= _goods.Count)
            return false;
            
        return TryRemoveItem(_goods[index]);
    }
    
    public void RemoveAllItems()
    {
        _goods.Clear();
        _currentWeight = 0;
        
        UpdateOverloadStatus();
        WeightChanged?.Invoke(_currentWeight);
        ItemsCountChanged?.Invoke(_goods.Count);
    }
    
    public Goods GetItemByIndex(int index)
    {
        if (index < 0 || index >= _goods.Count)
            return null;
            
        return _goods[index];
    }
    
    public bool HasItem(Goods item)
    {
        return _goods.Contains(item);
    }
    
    public int GetItemCount(Goods item)
    {
        int count = 0;
        foreach (var good in _goods)
        {
            if (good == item)
                count++;
        }
        return count;
    }
    
    public int GetTotalValue()
    {
        int totalValue = 0;
        foreach (var item in _goods)
        {
            totalValue += item.Price;
        }
        return totalValue;
    }
    
    public void SellAllItems()
    {
        int totalValue = GetTotalValue();
        _player.AddMoney(totalValue);
        RemoveAllItems();
    }
    
    public void SellItem(Goods item)
    {
        if (TryRemoveItem(item))
        {
            _player.AddMoney(item.Price);
        }
    }
    
    public void SellItemByIndex(int index)
    {
        Goods item = GetItemByIndex(index);
        if (item != null)
        {
            SellItem(item);
        }
    }
    
    public void UpdateMaxWeight(int newMaxWeight)
    {
        _maxWeight = newMaxWeight;
        UpdateOverloadStatus();
    }
    
    private int GetItemWeight(Goods item)
    {
        // Базовая логика веса товаров
        // В будущем нужно добавить поле Weight в Goods ScriptableObject
        
        string itemName = item.Label.ToLower();
        
        // Хрупкие предметы (яйца, огурцы)
        if (itemName.Contains("яйцо") || itemName.Contains("огурец") || itemName.Contains("помидор"))
        {
            return Mathf.RoundToInt(_fragileItemWeight);
        }
        
        // Тяжелые предметы (алкоголь, консервы)
        if (itemName.Contains("водка") || itemName.Contains("пиво") || itemName.Contains("консерв"))
        {
            return Mathf.RoundToInt(_heavyItemWeight);
        }
        
        // Обычные товары
        return Mathf.RoundToInt(_weightPerItem);
    }
    
    private void UpdateOverloadStatus()
    {
        if (_mover != null)
        {
            _mover.UpdateOverloadStatus(_currentWeight, _maxWeight);
        }
    }
    
    private void UpdateMaxWeight()
    {
        if (_player != null)
        {
            _maxWeight = _player.MaxInventoryWeight;
        }
    }
    
    // Метод для отладки
    public void ShowInventoryInfo()
    {
        Debug.Log($"Инвентарь: {_goods.Count}/{_maxItems} предметов, {_currentWeight}/{_maxWeight} веса");
        
        foreach (var item in _goods)
        {
            Debug.Log($"- {item.Label}: {item.Price}₽, вес: {GetItemWeight(item)}");
        }
    }
    
    public bool CanAddItem(Goods item)
    {
        if (item == null)
            return false;
            
        int itemWeight = GetItemWeight(item);
        return _currentWeight + itemWeight <= _maxWeight && _goods.Count < _maxItems;
    }
    
    public int GetFreeWeight()
    {
        return _maxWeight - _currentWeight;
    }
    
    public int GetFreeSlots()
    {
        return _maxItems - _goods.Count;
    }
}