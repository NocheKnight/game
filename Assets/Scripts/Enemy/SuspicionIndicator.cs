using UnityEngine;
using UnityEngine.UI;

public class SuspicionIndicator : MonoBehaviour
{
    [SerializeField] private Enemy _enemy;
    [SerializeField] private Image _suspicionBar;
    [SerializeField] private Image _alertBar;
    [SerializeField] private GameObject _suspicionIcon;
    [SerializeField] private GameObject _alertIcon;
    
    [Header("Цвета")]
    [SerializeField] private Color _lowSuspicionColor = Color.green;
    [SerializeField] private Color _mediumSuspicionColor = Color.yellow;
    [SerializeField] private Color _highSuspicionColor = Color.red;
    [SerializeField] private Color _alertColor = Color.red;

    private void Start()
    {
        if (_enemy == null)
        {
            _enemy = GetComponent<Enemy>();
        }
        
        if (_enemy != null)
        {
            _enemy.SuspicionLevelChanged += OnSuspicionLevelChanged;
            _enemy.SuspiciousChanged += OnSuspiciousChanged;
            _enemy.AlertedChanged += OnAlertedChanged;
        }
        
        UpdateVisuals();
    }

    private void Update()
    {
        // Обновляем визуал в Update для плавности
        if (_enemy != null)
        {
            UpdateSuspicionBar();
        }
    }
    
    private void OnSuspicionLevelChanged(float suspicionLevel)
    {
        UpdateSuspicionBar();
    }
    
    private void OnSuspiciousChanged(bool isSuspicious)
    {
        UpdateVisuals();
    }
    
    private void OnAlertedChanged(bool isAlerted)
    {
        UpdateVisuals();
    }
    
    private void UpdateSuspicionBar()
    {
        if (_suspicionBar != null && _enemy != null)
        {
            float suspicion = Mathf.Clamp01(_enemy.SuspicionLevel);
            _suspicionBar.fillAmount = suspicion;
            
            // Меняем цвет в зависимости от уровня подозрений
            if (suspicion < 0.3f)
            {
                _suspicionBar.color = _lowSuspicionColor;
            }
            else if (suspicion < 0.7f)
            {
                _suspicionBar.color = _mediumSuspicionColor;
            }
            else
            {
                _suspicionBar.color = _highSuspicionColor;
            }
        }
    }
    
    private void UpdateVisuals()
    {
        if (_enemy == null) return;
        
        // Показываем/скрываем иконки в зависимости от состояния
        if (_suspicionIcon != null)
        {
            _suspicionIcon.SetActive(_enemy.IsSuspicious && !_enemy.IsAlerted);
        }
        
        if (_alertIcon != null)
        {
            _alertIcon.SetActive(_enemy.IsAlerted);
        }
        
        // Обновляем полосу тревоги
        if (_alertBar != null)
        {
            _alertBar.fillAmount = _enemy.IsAlerted ? 1f : 0f;
            _alertBar.color = _alertColor;
        }
    }
    
    private void OnDestroy()
    {
        if (_enemy != null)
        {
            _enemy.SuspicionLevelChanged -= OnSuspicionLevelChanged;
            _enemy.SuspiciousChanged -= OnSuspiciousChanged;
            _enemy.AlertedChanged -= OnAlertedChanged;
        }
    }
} 