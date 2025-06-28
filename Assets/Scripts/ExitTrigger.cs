using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    [Header("Настройки выхода")]
    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private bool _requireItems = true;
    [SerializeField] private int _minItemsRequired = 1;
    [SerializeField] private string _exitMessage = "Выход";
    
    [Header("UI")]
    [SerializeField] private WinUI _winUI;
    
    private void Start()
    {
        // Ищем WinUI если не назначен
        if (_winUI == null)
        {
            _winUI = FindObjectOfType<WinUI>();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_playerTag))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                CheckExitConditions(player);
            }
        }
    }
    
    private void CheckExitConditions(Player player)
    {
        var inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null) return;
        
        // Проверяем, есть ли украденные предметы
        if (_requireItems)
        {
            int stolenItems = inventory.GetStolenItemsCount();
            if (stolenItems < _minItemsRequired)
            {
                ShowNotEnoughItemsMessage();
                return;
            }
        }
        
        // Условия выполнены - показываем победу
        ShowWinScreen();
    }
    
    private void ShowNotEnoughItemsMessage()
    {
        // Можно показать сообщение игроку
        Debug.Log($"Нужно украсть минимум {_minItemsRequired} предметов для выхода!");
        
        // Здесь можно добавить UI сообщение
        // Например, временный текст на экране
    }
    
    private void ShowWinScreen()
    {
        if (_winUI != null)
        {
            _winUI.ShowEscapeWin();
        }
        else
        {
            Debug.LogWarning("WinUI не найден! Создайте WinUI в сцене.");
        }
    }
    
    // Методы для отладки в инспекторе
    [ContextMenu("Проверить выход")]
    private void TestExit()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            CheckExitConditions(player);
        }
    }
    
    [ContextMenu("Показать победу")]
    private void TestWin()
    {
        ShowWinScreen();
    }
    
    private void OnDrawGizmos()
    {
        // Визуализация триггера в редакторе
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // Подпись
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.2f);
    }
} 