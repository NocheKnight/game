using UnityEngine;
using UnityEngine.UI;

public class SimplePlayerStatsUI : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private Text _moneyText;
    [SerializeField] private Text _crimeRateText;
    [SerializeField] private Text _stealthLevelText;
    [SerializeField] private Text _inventoryText;
    
    [Header("Настройки")]
    [SerializeField] private string _moneyPrefix = "Деньги: ";
    [SerializeField] private string _crimePrefix = "Рейтинг: ";
    [SerializeField] private string _stealthPrefix = "Уровень: ";
    [SerializeField] private string _inventoryPrefix = "Инвентарь: ";
    
    private Player _player;
    private PlayerInventory _inventory;
    
    private void Start()
    {
        _player = FindObjectOfType<Player>();
        if (_player != null)
        {
            _inventory = _player.GetComponent<PlayerInventory>();
        }
    }
    
    private void Update()
    {
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (_player == null) return;
        
        // Обновляем деньги
        if (_moneyText != null)
        {
            _moneyText.text = _moneyPrefix + _player.Money.ToString() + "₽";
        }
        
        // Обновляем рейтинг преступности
        if (_crimeRateText != null)
        {
            string crimeLevel = GetCrimeLevel(_player.CrimeRate);
            _crimeRateText.text = _crimePrefix + crimeLevel + " (" + _player.CrimeRate + ")";
        }
        
        // Обновляем уровень воровства
        if (_stealthLevelText != null)
        {
            int stealthLevel = GetStealthLevel();
            _stealthLevelText.text = _stealthPrefix + stealthLevel.ToString();
        }
        
        // Обновляем информацию об инвентаре
        if (_inventoryText != null && _inventory != null)
        {
            int stolenCount = _inventory.GetStolenItemsCount();
            int totalItems = _inventory.ItemsCount;
            int totalValue = _inventory.GetTotalValue();
            
            _inventoryText.text = _inventoryPrefix + 
                totalItems + "/" + _inventory.MaxItems + 
                " (Украдено: " + stolenCount + 
                ", Цена: " + totalValue + "₽)";
        }
    }
    
    private string GetCrimeLevel(int crimeRate)
    {
        if (crimeRate <= 10) return "Невинный";
        if (crimeRate <= 25) return "Подозрительный";
        if (crimeRate <= 50) return "Вор";
        if (crimeRate <= 75) return "Опасный преступник";
        return "Мастер воровства";
    }
    
    private int GetStealthLevel()
    {
        if (_inventory == null) return 1;
        
        int stolenItems = _inventory.GetStolenItemsCount();
        
        if (stolenItems <= 5) return 1;
        if (stolenItems <= 15) return 2;
        if (stolenItems <= 30) return 3;
        if (stolenItems <= 50) return 4;
        return 5;
    }
    
    // Методы для отладки в инспекторе
    [ContextMenu("Обновить UI")]
    private void RefreshUI()
    {
        UpdateUI();
    }
    
    [ContextMenu("Показать статистику в консоли")]
    private void ShowStatsInConsole()
    {
        if (_player == null) return;
        
        Debug.Log("=== СТАТИСТИКА ИГРОКА ===");
        Debug.Log($"Деньги: {_player.Money}₽");
        Debug.Log($"Рейтинг преступности: {_player.CrimeRate}");
        Debug.Log($"Уровень воровства: {GetStealthLevel()}");
        
        if (_inventory != null)
        {
            Debug.Log($"Предметов в инвентаре: {_inventory.ItemsCount}/{_inventory.MaxItems}");
            Debug.Log($"Украдено предметов: {_inventory.GetStolenItemsCount()}");
            Debug.Log($"Общая стоимость: {_inventory.GetTotalValue()}₽");
        }
    }
} 