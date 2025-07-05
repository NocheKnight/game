using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndSceneController : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private Text _titleText;
    [SerializeField] private Text _statsText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;
    
    [Header("Настройки")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu";
    [SerializeField] private string _gameSceneName = "SampleScene";
    
    private void Start()
    {
        InitializeUI();
        LoadPlayerStats();
        SetupButtons();
    }
    
    private void InitializeUI()
    {
        // Устанавливаем заголовок
        if (_titleText != null)
        {
            _titleText.text = "Игра завершена!";
        }
    }
    
    private void LoadPlayerStats()
    {
        if (_statsText == null) return;
        
        // Загружаем сохраненную статистику
        int crimeRate = PlayerPrefs.GetInt("FinalCrimeRate", 0);
        float stealthBonus = PlayerPrefs.GetFloat("StealthBonus", 0f);
        
        string stats = $"Статистика:\n";
        stats += $"Уровень преступности: {crimeRate}%\n";
        stats += $"Бонус стелса: {stealthBonus}%\n";
        
        // Определяем результат
        if (crimeRate < 50)
        {
            stats += "\nОтличная работа! Вы почти не привлекали внимания.";
        }
        else if (crimeRate < 100)
        {
            stats += "\nХорошая работа! Вы были осторожны.";
        }
        else
        {
            stats += "\nВас поймали! Попробуйте быть более осторожным.";
        }
        
        _statsText.text = stats;
    }
    
    private void SetupButtons()
    {
        if (_restartButton != null)
        {
            _restartButton.onClick.AddListener(RestartGame);
        }
        
        if (_mainMenuButton != null)
        {
            _mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }
    
    public void RestartGame()
    {
        // Очищаем сохраненные данные
        PlayerPrefs.DeleteKey("FinalCrimeRate");
        PlayerPrefs.DeleteKey("StealthBonus");
        
        // Загружаем игровую сцену
        SceneManager.LoadScene(_gameSceneName);
    }
    
    public void GoToMainMenu()
    {
        // Очищаем сохраненные данные
        PlayerPrefs.DeleteKey("FinalCrimeRate");
        PlayerPrefs.DeleteKey("StealthBonus");
        
        // Загружаем главное меню
        SceneManager.LoadScene(_mainMenuSceneName);
    }
    
    // Методы для кнопок (если они вызываются через OnClick в инспекторе)
    public void OnRestartButtonClicked()
    {
        RestartGame();
    }
    
    public void OnMainMenuButtonClicked()
    {
        GoToMainMenu();
    }
} 