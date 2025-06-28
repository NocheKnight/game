using UnityEngine;

public class PlayerLostTransition : Transition
{
    [SerializeField] private float _loseThreshold = 0.2f;
    [SerializeField] private float _returnToPatrolDelay = 3f; // Задержка перед возвратом к патрулированию
    
    private Enemy _enemy;
    private float _lostTimer = 0f;
    private bool _playerWasLost = false;
    
    protected void OnEnable()
    {
        base.OnEnable();
        _enemy = GetComponent<Enemy>();
        _lostTimer = 0f;
        _playerWasLost = false;
    }
    
    private void Update()
    {
        if (_enemy != null)
        {
            // Если игрок потерян
            if (!_enemy.IsAlerted)
            {
                if (!_playerWasLost)
                {
                    _playerWasLost = true;
                    _lostTimer = 0f;
                }
                
                _lostTimer += Time.deltaTime;
                
                // Возвращаемся к патрулированию только после задержки и если подозрения низкие
                NeedTransit = _lostTimer >= _returnToPatrolDelay && 
                             _enemy.SuspicionLevel <= _loseThreshold;
            }
            else
            {
                // Игрок снова виден, сбрасываем таймер
                _playerWasLost = false;
                _lostTimer = 0f;
                NeedTransit = false;
            }
        }
    }
} 