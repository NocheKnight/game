using System;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int _health;
    // [SerializeField] private int _crimePoint;
    [SerializeField] private Player _target;

    public Player Target => _target;
    public event UnityAction Dying;

    private void TakeDamage(int damage)
    {
        _health -= damage;

        if (_health <= 0)
            Destroy(gameObject);
    }
}
