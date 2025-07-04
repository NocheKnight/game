using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimpleGameUI : MonoBehaviour
{
    [Header("Game Over")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Text _gameOverText;
    [SerializeField] private Button _restartButton;
    
    [Header("Win")]
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private Text _winText;
    [SerializeField] private Text _winStatsText;
    [SerializeField] private Button _winRestartButton;
    
    [Header("Настройки")]
    [SerializeField] private float _autoHideTime = 3f;
    [SerializeField] private bool _autoHide = false; // Отключаем автоскрытие для кнопок
    
    private float _timer = 0f;
    private bool _isShowingGameOver = false;
    private bool _isShowingWin = false;
    private Player _player;
    
    private void Start()
    {
        // Скрываем панели в начале
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        if (_winPanel != null) _winPanel.SetActive(false);
        
        // Настраиваем кнопки
        SetupButtons();
        
        _player = FindObjectOfType<Player>();
    }
    
    private void SetupButtons()
    {
        if (_restartButton != null)
        {
            _restartButton.onClick.RemoveAllListeners();
            _restartButton.onClick.AddListener(RestartGame);
            Debug.Log("Кнопка Restart настроена");
        }
        else
        {
            Debug.LogWarning("Restart Button не назначен!");
        }
        
        if (_winRestartButton != null)
        {
            _winRestartButton.onClick.RemoveAllListeners();
            _winRestartButton.onClick.AddListener(RestartGame);
            Debug.Log("Кнопка Win Restart настроена");
        }
        else
        {
            Debug.LogWarning("Win Restart Button не назначен!");
        }
    }
    
    private void Update()
    {
        // Автоматическое скрытие только если включено
        if (_autoHide && (_isShowingGameOver || _isShowingWin))
        {
            _timer += Time.unscaledDeltaTime; // Используем unscaledDeltaTime
            
            if (_timer >= _autoHideTime)
            {
                if (_isShowingGameOver) HideGameOver();
                if (_isShowingWin) HideWin();
            }
        }
        
        // Обработка клавиш для тестирования
        if (_isShowingGameOver || _isShowingWin)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("Нажата клавиша R - перезапуск игры");
                RestartGame();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Нажата клавиша Escape - скрытие UI");
                if (_isShowingGameOver) HideGameOver();
                if (_isShowingWin) HideWin();
            }
        }
    }
    
    // Game Over методы
    public void ShowGameOver(string reason = "ВЫ ПОЙМАНЫ!")
    {
        Debug.Log($"Показываем Game Over: {reason}");
        
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Game Over Panel не назначен!");
        }
        
        if (_gameOverText != null)
        {
            _gameOverText.text = reason;
        }
        else
        {
            Debug.LogError("Game Over Text не назначен!");
        }
        
        _isShowingGameOver = true;
        _timer = 0f;
        
        // Останавливаем время
        Time.timeScale = 0f;
        
        // Показываем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("Game Over показан. Нажмите R для перезапуска или Escape для скрытия.");
    }
    
    public void HideGameOver()
    {
        Debug.Log("Скрываем Game Over");
        
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(false);
        }
        
        _isShowingGameOver = false;
        _timer = 0f;
        
        // Возвращаем время
        Time.timeScale = 1f;
        
        // Скрываем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    // Win методы
    public void ShowWin(string reason = "ПОБЕДА!")
    {
        Debug.Log($"Показываем Win: {reason}");
        
        if (_winPanel != null)
        {
            _winPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Win Panel не назначен!");
        }
        
        if (_winText != null)
        {
            _winText.text = reason;
        }
        else
        {
            Debug.LogError("Win Text не назначен!");
        }
        
        if (_winStatsText != null)
        {
            UpdateWinStats();
        }
        
        _isShowingWin = true;
        _timer = 0f;
        
        // Останавливаем время
        Time.timeScale = 0f;
        
        // Показываем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("Win показан. Нажмите R для перезапуска или Escape для скрытия.");
    }
    
    public void HideWin()
    {
        Debug.Log("Скрываем Win");
        
        if (_winPanel != null)
        {
            _winPanel.SetActive(false);
        }
        
        _isShowingWin = false;
        _timer = 0f;
        
        // Возвращаем время
        Time.timeScale = 1f;
        
        // Скрываем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void UpdateWinStats()
    {
        if (_player == null) return;
        
        var inventory = _player.GetComponent<PlayerInventory>();
        if (inventory == null) return;
        
        int totalValue = inventory.GetTotalValue();
        int stolenItems = inventory.GetStolenItemsCount();
        
        string stats = $"Украдено: {stolenItems} предметов\n" +
                      $"Стоимость: {totalValue}₽\n" +
                      $"Деньги: {_player.Money}₽";
        
        _winStatsText.text = stats;
    }
    
    public void RestartGame()
    {
        Debug.Log("Перезапуск игры...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Публичные методы для внешнего вызова
    public void ShowCaughtByGuard()
    {
        ShowGameOver("Охранник догнал вас!");
    }
    
    public void ShowEscapeWin()
    {
        ShowWin("Вы успешно сбежали!");
    }
    
    // Методы для отладки в инспекторе
    [ContextMenu("Показать Game Over")]
    private void TestGameOver()
    {
        ShowGameOver("Тест Game Over");
    }
    
    [ContextMenu("Показать Win")]
    private void TestWin()
    {
        ShowWin("Тест Win");
    }
    
    [ContextMenu("Скрыть все")]
    private void HideAll()
    {
        HideGameOver();
        HideWin();
    }
    
    [ContextMenu("Перезапустить игру")]
    private void TestRestart()
    {
        RestartGame();
    }
} 