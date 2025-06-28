using UnityEngine;

public class SuspicionClearedTransition : Transition
{
    [SerializeField] private float _clearThreshold = 0.1f;
    
    private Enemy _enemy;
    
    protected void OnEnable()
    {
        base.OnEnable();
        _enemy = GetComponent<Enemy>();
    }
    
    private void Update()
    {
        if (_enemy != null)
        {
            // Возвращаемся к патрулированию, если подозрения исчезли
            // и игрок не обнаружен
            NeedTransit = _enemy.SuspicionLevel <= _clearThreshold && 
                         !_enemy.IsAlerted && 
                         !_enemy.IsSuspicious;
        }
    }
} 