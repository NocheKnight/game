using UnityEngine;

public class GameUITester : MonoBehaviour
{
    [Header("Тестирование UI")]
    [SerializeField] private KeyCode _testGameOverKey = KeyCode.G;
    [SerializeField] private KeyCode _testWinKey = KeyCode.W;
    [SerializeField] private KeyCode _hideAllKey = KeyCode.H;
    
    private SimpleGameUI _simpleGameUI;
    private GameOverUI _gameOverUI;
    private WinUI _winUI;
    
    private void Start()
    {
        // Ищем все типы UI
        _simpleGameUI = FindObjectOfType<SimpleGameUI>();
        _gameOverUI = FindObjectOfType<GameOverUI>();
        _winUI = FindObjectOfType<WinUI>();
        
        // Выводим информацию о найденных UI
        Debug.Log("=== UI ТЕСТЕР ЗАПУЩЕН ===");
        Debug.Log($"SimpleGameUI найден: {_simpleGameUI != null}");
        Debug.Log($"GameOverUI найден: {_gameOverUI != null}");
        Debug.Log($"WinUI найден: {_winUI != null}");
        Debug.Log("Нажмите G для теста Game Over, W для теста Win, H для скрытия всех");
    }
    
    private void Update()
    {
        // Тест Game Over
        if (Input.GetKeyDown(_testGameOverKey))
        {
            TestGameOver();
        }
        
        // Тест Win
        if (Input.GetKeyDown(_testWinKey))
        {
            TestWin();
        }
        
        // Скрыть все
        if (Input.GetKeyDown(_hideAllKey))
        {
            HideAll();
        }
    }
    
    private void TestGameOver()
    {
        Debug.Log("=== ТЕСТ GAME OVER ===");
        
        if (_simpleGameUI != null)
        {
            Debug.Log("Показываем Game Over через SimpleGameUI");
            _simpleGameUI.ShowCaughtByGuard();
        }
        else if (_gameOverUI != null)
        {
            Debug.Log("Показываем Game Over через GameOverUI");
            _gameOverUI.ShowCaughtByGuard();
        }
        else
        {
            Debug.LogWarning("Ни один Game Over UI не найден!");
        }
    }
    
    private void TestWin()
    {
        Debug.Log("=== ТЕСТ WIN ===");
        
        if (_simpleGameUI != null)
        {
            Debug.Log("Показываем Win через SimpleGameUI");
            _simpleGameUI.ShowEscapeWin();
        }
        else if (_winUI != null)
        {
            Debug.Log("Показываем Win через WinUI");
            _winUI.ShowEscapeWin();
        }
        else
        {
            Debug.LogWarning("Ни один Win UI не найден!");
        }
    }
    
    private void HideAll()
    {
        Debug.Log("=== СКРЫВАЕМ ВСЕ UI ===");
        
        if (_simpleGameUI != null)
        {
            _simpleGameUI.HideGameOver();
            _simpleGameUI.HideWin();
        }
        
        if (_gameOverUI != null)
        {
            _gameOverUI.HideGameOver();
        }
        
        if (_winUI != null)
        {
            _winUI.HideWin();
        }
        
        // Возвращаем нормальное время
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    // Методы для отладки в инспекторе
    [ContextMenu("Тест Game Over")]
    private void TestGameOverInspector()
    {
        TestGameOver();
    }
    
    [ContextMenu("Тест Win")]
    private void TestWinInspector()
    {
        TestWin();
    }
    
    [ContextMenu("Скрыть все")]
    private void HideAllInspector()
    {
        HideAll();
    }
} 