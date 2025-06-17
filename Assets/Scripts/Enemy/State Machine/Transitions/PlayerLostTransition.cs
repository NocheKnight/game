using UnityEngine;

public class PlayerLostTransition : Transition
{
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
            NeedTransit = !_enemy.IsAlerted;
        }
    }
} 