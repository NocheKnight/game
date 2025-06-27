using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Good", menuName = "Shop/Good", order = 51)]
public class Goods : ScriptableObject
{
    [Header("Основная информация")]
    [SerializeField] private String _label;
    [SerializeField] private String _description;
    [SerializeField] private int _price;
    [SerializeField] private GameObject _prefab;
    
    [Header("Игровые параметры")]
    [SerializeField] private float _weight = 1f;
    [SerializeField] private GoodsType _type = GoodsType.Regular;
    [SerializeField] private int _stealingDifficulty = 1;
    [SerializeField] private float _noiseLevel = 1f;
    [SerializeField] private bool _isFragile = false;
    [SerializeField] private bool _isValuable = false;
    
    [Header("Визуальные настройки")]
    [SerializeField] private Sprite _icon;
    [SerializeField] private Color _rarityColor = Color.white;
    
    // Публичные свойства
    public string Label => _label;
    public string Description => _description;
    public int Price => _price;
    public GameObject Prefab => _prefab;
    public float Weight => _weight;
    public GoodsType Type => _type;
    public int StealingDifficulty => _stealingDifficulty;
    public float NoiseLevel => _noiseLevel;
    public bool IsFragile => _isFragile;
    public bool IsValuable => _isValuable;
    public Sprite Icon => _icon;
    public Color RarityColor => _rarityColor;
}

// Перечисление типов товаров
public enum GoodsType
{
    Regular,        // Обычные товары
    Food,           // Продукты питания
    Alcohol,        // Алкоголь
    Electronics,    // Электроника
    Clothing,       // Одежда
    Medicine,       // Лекарства
    Luxury,         // Роскошные товары
    Illegal         // Запрещенные товары
}