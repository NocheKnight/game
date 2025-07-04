using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceCall : MonoBehaviour
{
    [SerializeField] private List<PoliceGroup> _policeGroups;
    [SerializeField] private Player _player;
    [SerializeField] private Transform _spawnPoint;
    
    private void Start()
    {
        // Автоматически ищем игрока если не назначен
        if (_player == null)
        {
            _player = FindObjectOfType<Player>();
        }
        
        // Автоматически создаём точку спавна если не назначена
        if (_spawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("PoliceSpawnPoint");
            spawnPoint.transform.position = transform.position + Vector3.forward * 10f;
            _spawnPoint = spawnPoint.transform;
        }
    }
    
    public void Call(int crimeRate)
    {
        // Проверяем, что игрок найден
        if (_player == null)
        {
            _player = FindObjectOfType<Player>();
        }
        
        if (_player != null)
        {
            _player.AddCrimeRate(crimeRate);
        }
        else
        {
            Debug.LogWarning("Player не найден! Не удалось добавить Crime Rate.");
        }
    }

    private void Spawn()
    {
        if (_spawnPoint == null)
        {
            Debug.LogWarning("Spawn Point не назначен!");
            return;
        }
        
        foreach (var police in _policeGroups)
        {
            if (police.Template != null)
            {
                Instantiate(police.Template, _spawnPoint.position, _spawnPoint.rotation, _spawnPoint);
            }
            else
            {
                Debug.LogWarning("Police Template не назначен!");
            }
        }
    }
    
    // Методы для отладки в инспекторе
    [ContextMenu("Найти игрока")]
    private void FindPlayer()
    {
        _player = FindObjectOfType<Player>();
        if (_player != null)
        {
            Debug.Log($"Игрок найден: {_player.name}");
        }
        else
        {
            Debug.LogWarning("Игрок не найден в сцене!");
        }
    }
    
    [ContextMenu("Тест вызова полиции")]
    private void TestCall()
    {
        Call(50);
    }
}

[System.Serializable]
public class PoliceGroup
{
    public GameObject Template;
    public float SpeedArrival;
}