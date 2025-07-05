using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitTrigger : MonoBehaviour
{
    [Header("Настройки выхода")]
    [SerializeField] private string _endSceneName = "EndScene";
    [SerializeField] private LayerMask _playerLayer = 1;
    [SerializeField] private bool _requireStealth = true;
    
    [Header("Сообщения")]
    [SerializeField] private string _successMessage = "Вы успешно вышли из магазина!";
    [SerializeField] private string _caughtMessage = "Вас поймали! Попробуйте снова.";
    
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что это игрок
        if (((1 << other.gameObject.layer) & _playerLayer) == 0) return;
        
        Player player = other.GetComponent<Player>();
        if (player == null) return;
        
        // Проверяем условия выхода
        if (CanExit(player))
        {
            ExitSuccessfully(player);
        }
        else
        {
            ExitFailed(player);
        }
    }
    
    private bool CanExit(Player player)
    {
        // Если не требуется стелс, то можно выйти всегда
        if (!   ) return true;
        
        // Проверяем, что игрок не в стелс-режиме (т.е. его не поймали)
        PlayerMover mover = player.GetComponent<PlayerMover>();
        if (mover != null && mover.IsStealthMode)
        {
            return true;
        }
        
        // Проверяем уровень преступности
        if (player.CrimeRate < 100) // Можно настроить порог
        {
            return true;
        }
        
        return false;
    }
    
    private void ExitSuccessfully(Player player)
    {
        Debug.Log(_successMessage);
        
        // Сохраняем статистику игрока (можно расширить)
        PlayerPrefs.SetInt("FinalCrimeRate", player.CrimeRate);
        PlayerPrefs.SetFloat("StealthBonus", player.GetStealthBonus());
        PlayerPrefs.Save();
        
        LoadEndScene();
    }
    
    private void ExitFailed(Player player)
    {
        Debug.Log(_caughtMessage);
        
        // Можно добавить эффекты или звуки
        // Например, включить сирену, вызвать полицию и т.д.
        
        // Пока что просто перезагружаем текущую сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void LoadEndScene()
    {
        SceneManager.LoadScene(_endSceneName);
    }
    
    // Метод для тестирования
    [ContextMenu("Test Exit")]
    private void TestExit()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            ExitSuccessfully(player);
        }
    }
} 