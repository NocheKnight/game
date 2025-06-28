using UnityEngine;

public class SuspiciousTransition : Transition
{
    [SerializeField] private float _suspicionThreshold = 0.3f;
    
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
            // Переходим в подозрительное состояние, если уровень подозрений выше порога
            // но игрок еще не обнаружен
            NeedTransit = _enemy.SuspicionLevel >= _suspicionThreshold && 
                         !_enemy.IsAlerted && 
                         _enemy.IsSuspicious;
        }
    }
} 