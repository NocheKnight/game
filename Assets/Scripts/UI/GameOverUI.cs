using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _gameOverText;
    [SerializeField] private TextMeshProUGUI _reasonText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private Button _continueButton;
    
    [Header("Настройки")]
    [SerializeField] private float _showDuration = 3f;
    [SerializeField] private string _caughtText = "ВЫ ПОЙМАНЫ!";
    [SerializeField] private string _caughtReason = "Охранник догнал вас!";
    
    private float _timer = 0f;
    private bool _isShowing = false;
    
    private void Start()
    {
        // Скрываем панель в начале
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(false);
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
    }
    
    private void Update()
    {
        if (_isShowing)
        {
            _timer += Time.deltaTime;
            
            // Автоматически скрываем через заданное время
            if (_timer >= _showDuration)
            {
                HideGameOver();
            }
        }
    }
    
    public void ShowGameOver(string reason = "")
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
        }
        
        if (_gameOverText != null)
        {
            _gameOverText.text = _caughtText;
        }
        
        if (_reasonText != null)
        {
            _reasonText.text = string.IsNullOrEmpty(reason) ? _caughtReason : reason;
        }
        
        _isShowing = true;
        _timer = 0f;
        
        // Останавливаем время игры
        Time.timeScale = 0f;
        
        // Показываем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void HideGameOver()
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(false);
        }
        
        _isShowing = false;
        _timer = 0f;
        
        // Возвращаем нормальное время
        Time.timeScale = 1f;
        
        // Скрываем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        HideGameOver();
    }
    
    // Публичные методы для внешнего вызова
    public void ShowCaughtByGuard()
    {
        ShowGameOver("Охранник догнал вас!");
    }
    
    public void ShowCaughtByCashier()
    {
        ShowGameOver("Кассирша обнаружила вас!");
    }
    
    public void ShowCaughtByPolice()
    {
        ShowGameOver("Полиция арестовала вас!");
    }
    
    // Методы для отладки в инспекторе
    [ContextMenu("Показать Game Over")]
    private void TestShowGameOver()
    {
        ShowGameOver("Тестовое сообщение");
    }
    
    [ContextMenu("Скрыть Game Over")]
    private void TestHideGameOver()
    {
        HideGameOver();
    }
} 