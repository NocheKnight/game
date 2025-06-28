using UnityEngine;

public class ReturnToPatrolTransition : Transition
{
    [SerializeField] private float _clearThreshold = 0.1f;
    [SerializeField] private float _returnDelay = 2f; // Задержка перед возвратом
    
    private Enemy _enemy;
    private float _returnTimer = 0f;
    private bool _shouldReturn = false;
    
    protected void OnEnable()
    {
        base.OnEnable();
        _enemy = GetComponent<Enemy>();
        _returnTimer = 0f;
        _shouldReturn = false;
    }
    
    private void Update()
    {
        if (_enemy != null)
        {
            // Проверяем, нужно ли возвращаться к патрулированию
            if (!_enemy.IsAlerted && _enemy.SuspicionLevel <= _clearThreshold)
            {
                if (!_shouldReturn)
                {
                    _shouldReturn = true;
                    _returnTimer = 0f;
                }
                
                _returnTimer += Time.deltaTime;
                
                // Возвращаемся к патрулированию после задержки
                NeedTransit = _returnTimer >= _returnDelay;
            }
            else
            {
                // Подозрения снова выросли, сбрасываем таймер
                _shouldReturn = false;
                _returnTimer = 0f;
                NeedTransit = false;
            }
        }
    }
} 