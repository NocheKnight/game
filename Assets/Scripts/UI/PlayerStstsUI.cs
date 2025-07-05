using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private TextMeshProUGUI _crimeRateText;
    [SerializeField] private TextMeshProUGUI _stealthLevelText;
    
    [Header("Настройки отображения")]
    [SerializeField] private string _moneyFormat = "₽{0:N0}";
    [SerializeField] private string _crimeRateFormat = "Рейтинг: {0}";
    [SerializeField] private string _stealthLevelFormat = "Уровень: {0}";
    
    [Header("Цвета")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _warningColor = Color.yellow;
    [SerializeField] private Color _dangerColor = Color.red;
    
    private Player _player;
    private PlayerInventory _inventory;
    
    private void Start()
    {
        _player = FindObjectOfType<Player>();
        if (_player != null)
        {
            _inventory = _player.GetComponent<PlayerInventory>();
        }
        
        UpdateUI();
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
            int money = _player.Money;
            _moneyText.text = string.Format(_moneyFormat, money);
            
            // Меняем цвет в зависимости от количества денег
            if (money < 0)
            {
                _moneyText.color = _dangerColor;
            }
            else if (money < 1000)
            {
                _moneyText.color = _warningColor;
            }
            else
            {
                _moneyText.color = _normalColor;
            }
        }
        
        // Обновляем рейтинг преступности
        if (_crimeRateText != null)
        {
            int crimeRate = _player.CrimeRate;
            string crimeLevel = GetCrimeLevel(crimeRate);
            _crimeRateText.text = string.Format(_crimeRateFormat, crimeLevel);
            
            // Меняем цвет в зависимости от рейтинга
            if (crimeRate > 80)
            {
                _crimeRateText.color = _dangerColor;
            }
            else if (crimeRate > 50)
            {
                _crimeRateText.color = _warningColor;
            }
            else
            {
                _crimeRateText.color = _normalColor;
            }
        }
        
        // Обновляем уровень воровства
        if (_stealthLevelText != null)
        {
            int stealthLevel = GetStealthLevel();
            _stealthLevelText.text = string.Format(_stealthLevelFormat, stealthLevel);
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
        
        // Уровень воровства зависит от количества украденных предметов
        int stolenItems = _inventory.GetStolenItemsCount();
        
        if (stolenItems <= 5) return 1;
        if (stolenItems <= 15) return 2;
        if (stolenItems <= 30) return 3;
        if (stolenItems <= 50) return 4;
        return 5;
    }
    
    // Публичные методы для внешнего обновления
    public void RefreshUI()
    {
        UpdateUI();
    }
    
    // Методы для анимации изменений
    public void AnimateMoneyChange(int oldValue, int newValue)
    {
        if (_moneyText != null)
        {
            // Можно добавить анимацию изменения значения
            StartCoroutine(AnimateValue(oldValue, newValue, _moneyText, _moneyFormat));
        }
    }
    
    private System.Collections.IEnumerator AnimateValue(int from, int to, TextMeshProUGUI text, string format)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(from, to, progress));
            text.text = string.Format(format, currentValue);
            yield return null;
        }
        
        text.text = string.Format(format, to);
    }
} 