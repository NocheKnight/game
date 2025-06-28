using UnityEngine;
using UnityEngine.UI;

public class SimpleSuspicionIndicator : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private Image _suspicionBar;
    [SerializeField] private Canvas _canvas;
    
    [Header("Настройки отображения")]
    [SerializeField] private float _heightOffset = 2f; // Высота над головой
    [SerializeField] private Vector2 _barSize = new Vector2(100f, 10f);
    
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
        
        // Создаем UI элементы если их нет
        if (_suspicionBar == null)
        {
            CreateUIElements();
        }
        
        UpdateBar();
    }
    
    private void CreateUIElements()
    {
        // Создаем Canvas если его нет
        if (_canvas == null)
        {
            GameObject canvasGO = new GameObject("SuspicionCanvas");
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.worldCamera = _mainCamera;
            
            // Добавляем CanvasScaler
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Добавляем GraphicRaycaster
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Создаем полоску подозрений
        GameObject barGO = new GameObject("SuspicionBar");
        barGO.transform.SetParent(_canvas.transform, false);
        
        // Создаем фон полоски
        GameObject backgroundGO = new GameObject("Background");
        backgroundGO.transform.SetParent(barGO.transform, false);
        Image backgroundImage = backgroundGO.AddComponent<Image>();
        backgroundImage.color = new Color(0, 0, 0, 0.5f);
        RectTransform backgroundRect = backgroundImage.rectTransform;
        backgroundRect.sizeDelta = _barSize;
        backgroundRect.anchoredPosition = Vector2.zero;
        
        // Создаем заполняющую полоску
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(barGO.transform, false);
        _suspicionBar = fillGO.AddComponent<Image>();
        _suspicionBar.color = _normalColor;
        _suspicionBar.type = Image.Type.Filled;
        _suspicionBar.fillMethod = Image.FillMethod.Horizontal;
        _suspicionBar.fillAmount = 0f;
        
        RectTransform fillRect = _suspicionBar.rectTransform;
        fillRect.sizeDelta = _barSize;
        fillRect.anchoredPosition = Vector2.zero;
        
        // Настраиваем позицию полоски
        RectTransform barRect = barGO.GetComponent<RectTransform>();
        barRect.sizeDelta = _barSize;
        barRect.anchoredPosition = Vector2.zero;
    }
    
    private void Update()
    {
        // Обновляем позицию полоски чтобы она следовала за охранником
        if (_canvas != null)
        {
            Vector3 worldPosition = transform.position + Vector3.up * _heightOffset;
            Vector3 screenPosition = _mainCamera.WorldToScreenPoint(worldPosition);
            
            // Проверяем, находится ли полоска перед камерой
            if (screenPosition.z > 0)
            {
                _canvas.gameObject.SetActive(true);
                
                // Конвертируем экранные координаты в позицию Canvas
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvas.transform as RectTransform,
                    screenPosition,
                    _canvas.worldCamera,
                    out Vector2 localPoint);
                
                _canvas.transform.position = worldPosition;
            }
            else
            {
                _canvas.gameObject.SetActive(false);
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
    
    // Методы для настройки в инспекторе
    public void SetBarSize(Vector2 size)
    {
        _barSize = size;
        if (_suspicionBar != null)
        {
            _suspicionBar.rectTransform.sizeDelta = size;
        }
    }
    
    public void SetHeightOffset(float height)
    {
        _heightOffset = height;
    }
} 