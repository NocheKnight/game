using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceCall : MonoBehaviour
{
    [SerializeField] private List<PoliceGroup> _policeGroups;
    [SerializeField] private Player _player;
    [SerializeField] private Transform _spawnPoint;
    
    public void Call(int crimeRate)
    {
        _player.AddCrimeRate(crimeRate);
    }

    private void Spawn()
    {
        foreach (var police in _policeGroups)
        {
            Instantiate(police.Template, _spawnPoint.position, _spawnPoint.rotation, _spawnPoint);
        }
    }

}

[System.Serializable]
public class PoliceGroup
{
    public GameObject Template;
    public float SpeedArrival;
}