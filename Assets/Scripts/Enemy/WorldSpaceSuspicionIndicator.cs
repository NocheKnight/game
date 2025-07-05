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
    private SecurityGuard _securityGuard;
    private Camera _mainCamera;
    private bool _isInitialized = false;
    
    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _securityGuard = GetComponent<SecurityGuard>();
        _mainCamera = Camera.main;
    }
    
    private void Start()
    {
        InitializeIndicator();
    }
    
    private void InitializeIndicator()
    {
        if (_isInitialized) return;
        
        if (_enemy == null)
        {
            _enemy = GetComponent<Enemy>();
        }
        
        if (_securityGuard == null)
        {
            _securityGuard = GetComponent<SecurityGuard>();
        }
        
        if (_enemy != null)
        {
            _enemy.SuspicionLevelChanged += OnSuspicionLevelChanged;
            _enemy.AlertedChanged += OnAlertedChanged;
            _isInitialized = true;
        UpdateBar();
        }
        else
        {
            Debug.LogWarning($"WorldSpaceSuspicionIndicator: Enemy компонент не найден на {gameObject.name}");
        }
    }
    
    private void Update()
    {
        if (!_isInitialized)
        {
            InitializeIndicator();
            return;
        }
        
        if (_canvas != null)
        {
            Vector3 targetPosition = transform.position + Vector3.up * _heightOffset;
            _canvas.transform.position = targetPosition;
            
            if (_mainCamera != null)
            {
                _canvas.transform.LookAt(_mainCamera.transform);
                _canvas.transform.Rotate(0, 180, 0);
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
        
        // Для SecurityGuard: если подозрительность ниже порога внимания, показываем 0%
        if (_securityGuard != null)
        {
            // Получаем порог внимания через рефлексию, так как он приватный
            var attentionThresholdField = typeof(SecurityGuard).GetField("_attentionThreshold", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (attentionThresholdField != null)
            {
                float attentionThreshold = (float)attentionThresholdField.GetValue(_securityGuard);
                
                // Если подозрительность ниже порога, показываем 0%
                if (suspicion < attentionThreshold)
                {
                    _suspicionBar.fillAmount = 0f;
                    _suspicionBar.color = _normalColor;
                    return;
                }
            }
        }
        
        // Обычная логика отображения
        _suspicionBar.fillAmount = suspicion;
        
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