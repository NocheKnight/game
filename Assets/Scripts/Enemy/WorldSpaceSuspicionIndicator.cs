using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceSuspicionIndicator : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private Image _suspicionBar;
    [SerializeField] private Canvas _canvas;
    
    [Header("Настройки")]
    [SerializeField] private float _heightOffset = 2f;
    
    [Header("Цвета")]
    [SerializeField] private Color _normalColor = Color.green;
    [SerializeField] private Color _suspiciousColor = Color.yellow;
    [SerializeField] private Color _alertColor = Color.red;
    
    private Enemy _enemy;
    private Camera _mainCamera;
    
    private void Start()
    {
        _enemy = GetComponent<Enemy>();
        _mainCamera = Camera.main;
        
        if (_enemy != null)
        {
            _enemy.SuspicionLevelChanged += OnSuspicionLevelChanged;
            _enemy.AlertedChanged += OnAlertedChanged;
        }
        
        UpdateBar();
    }
    
    private void Update()
    {
        // Обновляем позицию полоски
        if (_canvas != null)
        {
            Vector3 targetPosition = transform.position + Vector3.up * _heightOffset;
            _canvas.transform.position = targetPosition;
            
            // Поворачиваем полоску к камере
            if (_mainCamera != null)
            {
                _canvas.transform.LookAt(_mainCamera.transform);
                _canvas.transform.Rotate(0, 180, 0); // Разворачиваем лицом к камере
            }
        }
    }
    
    private void OnSuspicionLevelChanged(float suspicionLevel)
    {
        UpdateBar();
    }
    
    private void OnAlertedChanged(bool isAlerted)
    {
        UpdateBar();
    }
    
    private void UpdateBar()
    {
        if (_suspicionBar == null || _enemy == null) return;
        
        float suspicion = Mathf.Clamp01(_enemy.SuspicionLevel);
        _suspicionBar.fillAmount = suspicion;
        
        // Меняем цвет в зависимости от состояния
        if (_enemy.IsAlerted)
        {
            _suspicionBar.color = _alertColor;
        }
        else if (suspicion > 0.3f)
        {
            _suspicionBar.color = _suspiciousColor;
        }
        else
        {
            _suspicionBar.color = _normalColor;
        }
    }
    
    private void OnDestroy()
    {
        if (_enemy != null)
        {
            _enemy.SuspicionLevelChanged -= OnSuspicionLevelChanged;
            _enemy.AlertedChanged -= OnAlertedChanged;
        }
    }
    
    // Публичные методы для настройки
    public void SetBarColor(Color color)
    {
        if (_suspicionBar != null)
        {
            _suspicionBar.color = color;
        }
    }
    
    public void SetBarFillAmount(float amount)
    {
        if (_suspicionBar != null)
        {
            _suspicionBar.fillAmount = Mathf.Clamp01(amount);
        }
    }
} 