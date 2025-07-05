using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitTrigger : MonoBehaviour
{
    [Header("Настройки выхода")]
    [SerializeField] private string _endSceneFailedName = "GameOver";
    [SerializeField] private string _endSceneSuccessName = "GameResults";
    [SerializeField] private LayerMask _playerLayer = 1;
    [SerializeField] private bool _requireStealth = true;
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger");
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
        // Сохраняем статистику игрока (можно расширить)
        PlayerPrefs.SetInt("FinalCrimeRate", player.CrimeRate);
        PlayerPrefs.SetFloat("StealthBonus", player.GetStealthBonus());
        PlayerPrefs.Save();
        
        SceneManager.LoadScene(_endSceneSuccessName);
    }
    
    private void ExitFailed(Player player)
    {
        // Можно добавить эффекты или звуки
        // Например, включить сирену, вызвать полицию и т.д.
        
        // Пока что просто перезагружаем текущую сцену
        SceneManager.LoadScene(_endSceneFailedName);
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