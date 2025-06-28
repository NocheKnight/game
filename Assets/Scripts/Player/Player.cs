using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerMover))]
[RequireComponent(typeof(PlayerInventory))]
public class Player : MonoBehaviour
{
    [Header("Основные характеристики")]
    [SerializeField] private int _health = 100;
    [SerializeField] private int _satiety = 100;
    [SerializeField] private int _maxInventoryWeight = 10;
    
    [Header("Навыки воровства")]
    [SerializeField] private int _stealthLevel = 1;
    [SerializeField] private int _pickpocketLevel = 1;
    [SerializeField] private int _distractionLevel = 1;
    
    [Header("Экономика")]
    [SerializeField] private int _money = 0;
    [SerializeField] private int _fine = 0;
    
    private int _crimeRate;
    private PlayerInventory _inventory;
    private PlayerMover _mover;
    
    public int Money => _money;
    public int Fine => _fine;
    public int CrimeRate => _crimeRate;
    public int StealthLevel => _stealthLevel;
    public int PickpocketLevel => _pickpocketLevel;
    public int DistractionLevel => _distractionLevel;
    public int MaxInventoryWeight => _maxInventoryWeight;
    
    public bool IsStealing { get; private set; }
    
    public event UnityAction<int> MoneyChanged;
    public event UnityAction<int> FineChanged;
    public event UnityAction<int> CrimeRateChanged;
    public event UnityAction<int> StealthLevelChanged;
    public event UnityAction<int> PickpocketLevelChanged;
    public event UnityAction<int> DistractionLevelChanged;
    public event UnityAction PlayerDied;
    
    private void Awake()
    {
        _inventory = GetComponent<PlayerInventory>();
        _mover = GetComponent<PlayerMover>();
    }
    
    private void Start()
    {
        UpdateInventoryWeight();
    }

    public void ApplyDamage(int damage)
    {
        _health -= damage;

        if (_health <= 0)
            Die();
    }

    public void AddMoney(int money)
    {
        _money += money;
        MoneyChanged?.Invoke(_money);
    }
    
    public bool SpendMoney(int amount)
    {
        if (_money >= amount)
        {
            _money -= amount;
            MoneyChanged?.Invoke(_money);
            return true;
        }
        return false;
    }
    
    public void AddFine(int fineAmount)
    {
        _fine += fineAmount;
        FineChanged?.Invoke(_fine);
        
        if (_money >= _fine)
        {
            PayFine();
        }
    }
    
    public void PayFine()
    {
        if (_money >= _fine)
        {
            _money -= _fine;
            _fine = 0;
            MoneyChanged?.Invoke(_money);
            FineChanged?.Invoke(_fine);
        }
    }

    public void AddCrimeRate(int crimePoint)
    {
        _crimeRate += crimePoint;
        CrimeRateChanged?.Invoke(_crimeRate);
        

        if (_crimeRate >= 100)
        {
            CallPolice();
        }
    }
    
    public void ReduceCrimeRate(int reduction)
    {
        _crimeRate = Mathf.Max(0, _crimeRate - reduction);
        CrimeRateChanged?.Invoke(_crimeRate);
    }

    public void OnEnemyDied(int crimePoint)
    {
        AddCrimeRate(crimePoint);
    }
    
    public bool TryUpgradeStealth()
    {
        int cost = GetStealthUpdateCost();
        if (SpendMoney(cost))
        {
            _stealthLevel++;
            StealthLevelChanged?.Invoke(_stealthLevel);
            UpdateInventoryWeight();
            return true;
        }
        return false;
    }

    public int GetStealthUpdateCost() {
        return _stealthLevel * 100;
    }
    
    public bool TryUpgradePickpocket()
    {
        int cost = GetPickpocketUpdateCost();
        if (SpendMoney(cost))
        {
            _pickpocketLevel++;
            PickpocketLevelChanged?.Invoke(_pickpocketLevel);
            return true;
        }
        return false;
    }

    public int GetPickpocketUpdateCost() {
        return _pickpocketLevel * 150;
    }
    
    public bool TryUpgradeDistraction()
    {
        int cost = GetDistractionUpdateCost();
        if (SpendMoney(cost))
        {
            _distractionLevel++;
            DistractionLevelChanged?.Invoke(_distractionLevel);
            return true;
        }
        return false;
    }

    public int GetDistractionUpdateCost() {
        return _distractionLevel * 120;;
    }
    
    // Методы для стелс-механик
    public float GetStealthBonus()
    {
        return _stealthLevel * 0.1f; // +10% к стелсу за уровень
    }
    
    public float GetPickpocketChance()
    {
        return Mathf.Min(0.9f, _pickpocketLevel * 0.15f); // Максимум 90% шанс
    }
    
    public float GetDistractionEffectiveness()
    {
        return _distractionLevel * 0.2f; // +20% эффективности за уровень
    }
    
    private void UpdateInventoryWeight()
    {
        if (_inventory != null)
        {
            _maxInventoryWeight = 10 + (_stealthLevel - 1) * 2; // +2 к вместимости за уровень стелса
            _inventory.UpdateMaxWeight(_maxInventoryWeight);
        }
    }
    
    private void CallPolice()
    {
        var policeCall = FindObjectOfType<PoliceCall>();
        if (policeCall != null)
        {
            policeCall.Call(_crimeRate);
        }
        
        _crimeRate = 0;
        CrimeRateChanged?.Invoke(_crimeRate);
    }

    private void Die()
    {
        PlayerDied?.Invoke();
        Destroy(gameObject);
    }

    public void TrySteal()
    {
        IsStealing = true;
        Invoke(nameof(ResetStealing), 0.5f); // сброс через 0.5 сек
    }

    private void ResetStealing()
    {
        IsStealing = false;
    }
}