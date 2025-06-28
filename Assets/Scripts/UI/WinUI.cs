using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class WinUI : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private TextMeshProUGUI _winText;
    [SerializeField] private TextMeshProUGUI _statsText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private Button _continueButton;
    
    [Header("Настройки")]
    [SerializeField] private float _showDuration = 5f;
    [SerializeField] private string _winMessage = "ПОБЕДА!";
    [SerializeField] private string _escapeMessage = "Вы успешно сбежали!";
    
    private float _timer = 0f;
    private bool _isShowing = false;
    private Player _player;
    
    private void Start()
    {
        // Скрываем панель в начале
        if (_winPanel != null)
        {
            _winPanel.SetActive(false);
        }
        
        // Настраиваем кнопки
        if (_restartButton != null)
        {
            _restartButton.onClick.AddListener(RestartGame);
        }
        
        if (_mainMenuButton != null)
        {
            _mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        
        if (_continueButton != null)
        {
            _continueButton.onClick.AddListener(ContinueGame);
        }
        
        _player = FindObjectOfType<Player>();
    }
    
    private void Update()
    {
        if (_isShowing)
        {
            _timer += Time.deltaTime;
            
            // Автоматически скрываем через заданное время
            if (_timer >= _showDuration)
            {
                HideWin();
            }
        }
    }
    
    public void ShowWin(string reason = "")
    {
        if (_winPanel != null)
        {
            _winPanel.SetActive(true);
        }
        
        if (_winText != null)
        {
            _winText.text = _winMessage;
        }
        
        if (_statsText != null)
        {
            UpdateStatsText();
        }
        
        _isShowing = true;
        _timer = 0f;
        
        // Останавливаем время игры
        Time.timeScale = 0f;
        
        // Показываем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void HideWin()
    {
        if (_winPanel != null)
        {
            _winPanel.SetActive(false);
        }
        
        _isShowing = false;
        _timer = 0f;
        
        // Возвращаем нормальное время
        Time.timeScale = 1f;
        
        // Скрываем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void UpdateStatsText()
    {
        if (_player == null) return;
        
        var inventory = _player.GetComponent<PlayerInventory>();
        if (inventory == null) return;
        
        int totalValue = inventory.GetTotalValue();
        int stolenItems = inventory.GetStolenItemsCount();
        int crimeRate = _player.CrimeRate;
        
        string stats = $"Статистика:\n" +
                      $"Украдено предметов: {stolenItems}\n" +
                      $"Общая стоимость: {totalValue}₽\n" +
                      $"Рейтинг преступности: {crimeRate}\n" +
                      $"Деньги: {_player.Money}₽";
        
        _statsText.text = stats;
    }
    
    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Замените на имя вашей главной сцены
    }
    
    private void ContinueGame()
    {
        HideWin();
    }
    
    // Публичные методы для внешнего вызова
    public void ShowEscapeWin()
    {
        ShowWin(_escapeMessage);
    }
    
    public void ShowMissionComplete()
    {
        ShowWin("Миссия выполнена!");
    }
    
    // Методы для отладки в инспекторе
    [ContextMenu("Показать Win")]
    private void TestShowWin()
    {
        ShowWin("Тестовая победа!");
    }
    
    [ContextMenu("Скрыть Win")]
    private void TestHideWin()
    {
        HideWin();
    }
} 