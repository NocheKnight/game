using UnityEngine;
using UnityEngine.UI;

public class ButtonTester : MonoBehaviour
{
    [Header("Тестирование кнопок")]
    [SerializeField] private Button _testButton;
    [SerializeField] private string _buttonName = "Test Button";
    
    private void Start()
    {
        if (_testButton != null)
        {
            _testButton.onClick.AddListener(OnButtonClick);
            Debug.Log($"Кнопка {_buttonName} настроена");
        }
        else
        {
            Debug.LogWarning($"Кнопка {_buttonName} не назначена!");
        }
    }
    
    private void OnButtonClick()
    {
        Debug.Log($"Кнопка {_buttonName} нажата!");
        
        // Можно добавить дополнительную логику здесь
        // Например, перезапуск игры
        RestartGame();
    }
    
    private void RestartGame()
    {
        Debug.Log("Перезапуск игры из ButtonTester...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
    
    // Методы для отладки в инспекторе
    [ContextMenu("Тест кнопки")]
    private void TestButton()
    {
        OnButtonClick();
    }
    
    [ContextMenu("Перезапустить игру")]
    private void TestRestart()
    {
        RestartGame();
    }
} 