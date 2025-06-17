using System.Collections.Generic;
using UnityEngine;

public class GoodsGenerator : MonoBehaviour
{
    [Header("Настройки генерации")]
    [SerializeField] private bool _generateOnStart = true;
    [SerializeField] private int _shelfCount = 5;
    [SerializeField] private Vector3 _shelfSpacing = new Vector3(3f, 0f, 3f);
    [SerializeField] private Vector3 _startPosition = Vector3.zero;
    
    [Header("Типы товаров")]
    [SerializeField] private List<Goods> _foodGoods = new List<Goods>();
    [SerializeField] private List<Goods> _alcoholGoods = new List<Goods>();
    [SerializeField] private List<Goods> _electronicsGoods = new List<Goods>();
    [SerializeField] private List<Goods> _clothingGoods = new List<Goods>();
    [SerializeField] private List<Goods> _medicineGoods = new List<Goods>();
    [SerializeField] private List<Goods> _luxuryGoods = new List<Goods>();
    
    [Header("Настройки полок")]
    [SerializeField] private GameObject _shelfPrefab;
    [SerializeField] private Material _shelfMaterial;
    [SerializeField] private Material _shelfHighlightMaterial;
    
    private List<Shelf> _generatedShelves = new List<Shelf>();
    
    private void Start()
    {
        if (_generateOnStart)
        {
            GenerateShop();
        }
    }
    
    [ContextMenu("Generate Shop")]
    public void GenerateShop()
    {
        ClearExistingShelves();
        GenerateShelves();
        Debug.Log($"Магазин сгенерирован! Создано полок: {_generatedShelves.Count}");
    }
    
    [ContextMenu("Clear Shop")]
    public void ClearExistingShelves()
    {
        foreach (var shelf in _generatedShelves)
        {
            if (shelf != null)
            {
                DestroyImmediate(shelf.gameObject);
            }
        }
        _generatedShelves.Clear();
    }
    
    private void GenerateShelves()
    {
        for (int i = 0; i < _shelfCount; i++)
        {
            Vector3 position = _startPosition + new Vector3(
                i * _shelfSpacing.x,
                0f,
                (i % 2) * _shelfSpacing.z
            );
            
            Shelf shelf = CreateShelf(position, i);
            _generatedShelves.Add(shelf);
        }
    }
    
    private Shelf CreateShelf(Vector3 position, int shelfIndex)
    {
        GameObject shelfObject;
        
        if (_shelfPrefab != null)
        {
            shelfObject = Instantiate(_shelfPrefab, position, Quaternion.identity);
        }
        else
        {
            shelfObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelfObject.transform.position = position;
            shelfObject.transform.localScale = new Vector3(2f, 1f, 0.5f);
            
            Renderer renderer = shelfObject.GetComponent<Renderer>();
            if (renderer != null && _shelfMaterial != null)
            {
                renderer.material = _shelfMaterial;
            }
        }
        
        shelfObject.name = $"Shelf_{shelfIndex}";
        
        Shelf shelf = shelfObject.AddComponent<Shelf>();
        
        ConfigureShelf(shelf, shelfIndex);
        
        return shelf;
    }
    
    private void ConfigureShelf(Shelf shelf, int shelfIndex)
    {
        GoodsType shelfType = GetShelfType(shelfIndex);
        List<Goods> availableGoods = GetGoodsByType(shelfType);
        
        Debug.Log($"[GoodsGenerator] Настройка полки {shelfIndex + 1}, тип: {shelfType}, доступных товаров: {availableGoods.Count}");
        
        var shelfNameField = typeof(Shelf).GetField("_shelfName", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (shelfNameField != null)
        {
            shelfNameField.SetValue(shelf, $"Полка {shelfIndex + 1} ({shelfType})");
        }
        
        shelf.SetAvailableGoods(availableGoods);
        
        var normalMaterialField = typeof(Shelf).GetField("_normalMaterial", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (normalMaterialField != null)
        {
            normalMaterialField.SetValue(shelf, _shelfMaterial);
        }
        
        var highlightMaterialField = typeof(Shelf).GetField("_highlightMaterial", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (highlightMaterialField != null)
        {
            highlightMaterialField.SetValue(shelf, _shelfHighlightMaterial);
        }
        
        Debug.Log($"Полка {shelfIndex + 1} настроена с типом {shelfType}, товаров: {availableGoods.Count}");
    }
    
    private GoodsType GetShelfType(int shelfIndex)
    {
        // Распределяем типы товаров по полкам
        switch (shelfIndex % 6)
        {
            case 0: return GoodsType.Food;
            case 1: return GoodsType.Alcohol;
            case 2: return GoodsType.Electronics;
            case 3: return GoodsType.Clothing;
            case 4: return GoodsType.Medicine;
            case 5: return GoodsType.Luxury;
            default: return GoodsType.Regular;
        }
    }
    
    private List<Goods> GetGoodsByType(GoodsType type)
    {
        switch (type)
        {
            case GoodsType.Food:
                return _foodGoods.Count > 0 ? _foodGoods : CreateDefaultFoodGoods();
            case GoodsType.Alcohol:
                return _alcoholGoods.Count > 0 ? _alcoholGoods : CreateDefaultAlcoholGoods();
            case GoodsType.Electronics:
                return _electronicsGoods.Count > 0 ? _electronicsGoods : CreateDefaultElectronicsGoods();
            case GoodsType.Clothing:
                return _clothingGoods.Count > 0 ? _clothingGoods : CreateDefaultClothingGoods();
            case GoodsType.Medicine:
                return _medicineGoods.Count > 0 ? _medicineGoods : CreateDefaultMedicineGoods();
            case GoodsType.Luxury:
                return _luxuryGoods.Count > 0 ? _luxuryGoods : CreateDefaultLuxuryGoods();
            default:
                return CreateDefaultRegularGoods();
        }
    }
    
    private List<Goods> CreateDefaultFoodGoods()
    {
        List<Goods> goods = new List<Goods>();
        
        goods.Add(CreateGood("Хлеб", "Свежий хлеб", 30, 0.5f, GoodsType.Food, 1, false, false));
        goods.Add(CreateGood("Молоко", "Свежее молоко", 80, 1f, GoodsType.Food, 1, true, false));
        goods.Add(CreateGood("Сыр", "Твердый сыр", 150, 0.3f, GoodsType.Food, 2, false, false));
        goods.Add(CreateGood("Яйца", "Куриные яйца", 120, 0.8f, GoodsType.Food, 1, true, false));
        
        return goods;
    }
    
    private List<Goods> CreateDefaultAlcoholGoods()
    {
        List<Goods> goods = new List<Goods>();
        
        goods.Add(CreateGood("Пиво", "Светлое пиво", 100, 1.5f, GoodsType.Alcohol, 3, false, false));
        goods.Add(CreateGood("Водка", "Крепкий алкоголь", 300, 2f, GoodsType.Alcohol, 5, false, true));
        goods.Add(CreateGood("Вино", "Красное вино", 500, 1.8f, GoodsType.Alcohol, 4, true, true));
        
        return goods;
    }
    
    private List<Goods> CreateDefaultElectronicsGoods()
    {
        List<Goods> goods = new List<Goods>();
        
        goods.Add(CreateGood("Телефон", "Смартфон", 15000, 0.2f, GoodsType.Electronics, 8, true, true));
        goods.Add(CreateGood("Наушники", "Беспроводные наушники", 3000, 0.1f, GoodsType.Electronics, 6, false, true));
        goods.Add(CreateGood("Зарядка", "USB кабель", 200, 0.05f, GoodsType.Electronics, 2, false, false));
        
        return goods;
    }
    
    private List<Goods> CreateDefaultClothingGoods()
    {
        List<Goods> goods = new List<Goods>();
        
        goods.Add(CreateGood("Футболка", "Хлопковая футболка", 800, 0.3f, GoodsType.Clothing, 3, false, false));
        goods.Add(CreateGood("Джинсы", "Классические джинсы", 2500, 0.8f, GoodsType.Clothing, 4, false, false));
        goods.Add(CreateGood("Кроссовки", "Спортивная обувь", 4000, 1f, GoodsType.Clothing, 5, false, true));
        
        return goods;
    }
    
    private List<Goods> CreateDefaultMedicineGoods()
    {
        List<Goods> goods = new List<Goods>();
        
        goods.Add(CreateGood("Аспирин", "Обезболивающее", 150, 0.1f, GoodsType.Medicine, 4, false, false));
        goods.Add(CreateGood("Витамины", "Комплекс витаминов", 500, 0.2f, GoodsType.Medicine, 3, false, false));
        goods.Add(CreateGood("Бинт", "Медицинский бинт", 80, 0.1f, GoodsType.Medicine, 2, false, false));
        
        return goods;
    }
    
    private List<Goods> CreateDefaultLuxuryGoods()
    {
        List<Goods> goods = new List<Goods>();
        
        goods.Add(CreateGood("Часы", "Дорогие часы", 25000, 0.1f, GoodsType.Luxury, 10, true, true));
        goods.Add(CreateGood("Кольцо", "Золотое кольцо", 15000, 0.05f, GoodsType.Luxury, 9, true, true));
        goods.Add(CreateGood("Духи", "Элитные духи", 8000, 0.3f, GoodsType.Luxury, 7, true, true));
        
        return goods;
    }
    
    private List<Goods> CreateDefaultRegularGoods()
    {
        List<Goods> goods = new List<Goods>();
        
        goods.Add(CreateGood("Мыло", "Туалетное мыло", 50, 0.2f, GoodsType.Regular, 1, false, false));
        goods.Add(CreateGood("Зубная паста", "Паста для зубов", 120, 0.3f, GoodsType.Regular, 1, false, false));
        goods.Add(CreateGood("Бумага", "Туалетная бумага", 200, 1f, GoodsType.Regular, 1, false, false));
        
        return goods;
    }
    
    private Goods CreateGood(string label, string description, int price, float weight, 
        GoodsType type, int stealingDifficulty, bool isFragile, bool isValuable)
    {
        Goods good = ScriptableObject.CreateInstance<Goods>();
        
        var labelField = typeof(Goods).GetField("_label", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        labelField?.SetValue(good, label);
        
        var descField = typeof(Goods).GetField("_description", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        descField?.SetValue(good, description);
        
        var priceField = typeof(Goods).GetField("_price", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        priceField?.SetValue(good, price);
        
        var weightField = typeof(Goods).GetField("_weight", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        weightField?.SetValue(good, weight);
        
        var typeField = typeof(Goods).GetField("_type", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        typeField?.SetValue(good, type);
        
        var difficultyField = typeof(Goods).GetField("_stealingDifficulty", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        difficultyField?.SetValue(good, stealingDifficulty);
        
        var fragileField = typeof(Goods).GetField("_isFragile", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        fragileField?.SetValue(good, isFragile);
        
        var valuableField = typeof(Goods).GetField("_isValuable", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        valuableField?.SetValue(good, isValuable);
        
        GameObject prefab = CreateItemPrefab(label, type);
        var prefabField = typeof(Goods).GetField("_prefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        prefabField?.SetValue(good, prefab);
        
        Debug.Log($"[GoodsGenerator] Создан товар: {label} ({price}₽, {weight}кг, сложность: {stealingDifficulty})");
        
        return good;
    }
    
    private GameObject CreateItemPrefab(string label, GoodsType type)
    {
        GameObject prefab = new GameObject($"Prefab_{label}");
        
        SimpleItemPrefab simpleItem = prefab.AddComponent<SimpleItemPrefab>();
        
        Color itemColor = GetItemColor(type);
        Vector3 itemSize = GetItemSize(label, type);
        
        var nameField = typeof(SimpleItemPrefab).GetField("_itemName", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        nameField?.SetValue(simpleItem, label);
        
        var colorField = typeof(SimpleItemPrefab).GetField("_itemColor", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorField?.SetValue(simpleItem, itemColor);
        
        var sizeField = typeof(SimpleItemPrefab).GetField("_itemSize", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        sizeField?.SetValue(simpleItem, itemSize);
        
        return prefab;
    }
    
    private Color GetItemColor(GoodsType type)
    {
        switch (type)
        {
            case GoodsType.Food:
                return new Color(0.8f, 0.6f, 0.4f); // Коричневый
            case GoodsType.Alcohol:
                return new Color(0.2f, 0.4f, 0.8f); // Синий
            case GoodsType.Electronics:
                return new Color(0.2f, 0.2f, 0.2f); // Темно-серый
            case GoodsType.Clothing:
                return new Color(0.8f, 0.4f, 0.6f); // Розовый
            case GoodsType.Medicine:
                return new Color(0.8f, 0.2f, 0.2f); // Красный
            case GoodsType.Luxury:
                return new Color(1f, 0.8f, 0.2f); // Золотой
            default:
                return Color.white;
        }
    }
    
    private Vector3 GetItemSize(string label, GoodsType type)
    {
        string lowerLabel = label.ToLower();
        
        if (lowerLabel.Contains("хлеб"))
            return new Vector3(0.3f, 0.1f, 0.2f);
        else if (lowerLabel.Contains("молоко"))
            return new Vector3(0.1f, 0.2f, 0.1f);
        else if (lowerLabel.Contains("сыр"))
            return new Vector3(0.2f, 0.1f, 0.15f);
        else if (lowerLabel.Contains("яйцо"))
            return new Vector3(0.08f, 0.08f, 0.08f);
        else if (lowerLabel.Contains("телефон"))
            return new Vector3(0.15f, 0.05f, 0.08f);
        else if (lowerLabel.Contains("часы"))
            return new Vector3(0.05f, 0.05f, 0.05f);
        else
            return new Vector3(0.2f, 0.2f, 0.2f);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < _shelfCount; i++)
        {
            Vector3 position = _startPosition + new Vector3(
                i * _shelfSpacing.x,
                0f,
                (i % 2) * _shelfSpacing.z
            );
            Gizmos.DrawWireCube(position, new Vector3(2f, 1f, 0.5f));
        }
    }
} 