using UnityEngine;

public class ChaseState : State
{
    [Header("Настройки преследования")]
    [SerializeField] private float _chaseSpeed = 5f; // Увеличено для погони
    [SerializeField] private float _rotationSpeed = 10f; // Увеличено для быстрого поворота
    [SerializeField] private float _catchDistance = 2f;
    [SerializeField] private float _loseDistance = 15f;
    [SerializeField] private float _chaseTimeout = 15f; // Таймаут погони
    
    private Enemy _enemy;
    private Vector3 _lastKnownPosition;
    private bool _hasReachedLastPosition = false;
    private float _chaseTimer = 0f;
    
    protected override void OnEnter()
    {
        _enemy = GetComponent<Enemy>();
        _hasReachedLastPosition = false;
        _chaseTimer = 0f;
        
        // Запоминаем последнюю известную позицию игрока
        if (_enemy.LastKnownPosition != null)
        {
            _lastKnownPosition = _enemy.LastKnownPosition.position;
        }
        else if (Target != null)
        {
            _lastKnownPosition = Target.transform.position;
        }
    }
    
    private void Update()
    {
        if (_enemy == null) return;
        
        _chaseTimer += Time.deltaTime;
        
        // Если игрок все еще виден, обновляем позицию
        if (_enemy.IsAlerted && Target != null)
        {
            _lastKnownPosition = Target.transform.position;
            _hasReachedLastPosition = false;
            _chaseTimer = 0f; // Сбрасываем таймер если видим игрока
        }
        
        // Проверяем таймаут погони
        if (_chaseTimer >= _chaseTimeout)
        {
            _enemy.ReduceSuspicion(0.3f); // Снижаем подозрения
            return;
        }
        
        // Если игрок потерян и мы достигли последней позиции, переходим в подозрительное состояние
        if (!_enemy.IsAlerted && _hasReachedLastPosition)
        {
            return;
        }
        
        MoveToTarget();
    }
    
    private void MoveToTarget()
    {
        Vector3 targetPosition = _lastKnownPosition;
        
        if (_enemy.IsAlerted && Target != null)
        {
            targetPosition = Target.transform.position;
            
            float distanceToPlayer = Vector3.Distance(transform.position, Target.transform.position);
            
            // Отладочная информация о расстоянии
            if (distanceToPlayer <= _catchDistance + 1f) // Показываем когда близко
            {
                Debug.Log($"Расстояние до игрока: {distanceToPlayer:F2}, порог поимки: {_catchDistance}");
            }
            
            if (distanceToPlayer <= _catchDistance)
            {
                Debug.Log($"Условие поимки выполнено! Расстояние: {distanceToPlayer:F2}");
                CatchPlayer();
                return;
            }
        }
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
        
        // Двигаемся к цели на высокой скорости, но с ограничением по Y
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance > 0.5f)
        {
            Vector3 movement = direction * _chaseSpeed * Time.deltaTime;
            // Сохраняем текущую Y позицию, чтобы избежать взлёта
            movement.y = 0;
            transform.position += movement;
        }
        else
        {
            // Достигли последней известной позиции
            _hasReachedLastPosition = true;
        }
    }
    
    private void CatchPlayer()
    {
        Debug.Log("CatchPlayer вызван! Охранник ловит игрока.");
        
        if (Target != null)
        {
            Debug.Log($"Игрок пойман: {Target.name}");
            
            // Игрок пойман - добавляем штраф
            Target.AddFine(1000);
            Target.AddCrimeRate(100);
            
            // Показываем Game Over
            ShowGameOver();
            
            // Сбрасываем состояние врага
            _enemy.ResetToStartPosition();
        }
        else
        {
            Debug.LogWarning("Target равен null в CatchPlayer!");
        }
    }
    
    private void ShowGameOver()
    {
        // Сначала ищем SimpleGameUI (упрощённая версия)
        var simpleGameUI = FindObjectOfType<SimpleGameUI>();
        if (simpleGameUI != null)
        {
            simpleGameUI.ShowCaughtByGuard();
            return;
        }
        
        // Если не найден, ищем GameOverUI (полная версия)
        var gameOverUI = FindObjectOfType<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.ShowCaughtByGuard();
            return;
        }
        
        // Если ничего не найдено, выводим предупреждение
        Debug.LogWarning("GameOverUI или SimpleGameUI не найден! Создайте один из них в сцене.");
        
        // Альтернативно, можно показать простой Game Over через Debug
        Debug.Log("GAME OVER: Охранник догнал вас!");
    }
    
    // Методы для отладки
    private void OnDrawGizmosSelected()
    {
        if (_enemy != null && _enemy.LastKnownPosition != null)
        {
            // Последняя известная позиция
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_lastKnownPosition, 0.5f);
            
            // Расстояние поимки
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, _catchDistance);
            
            // Расстояние потери
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _loseDistance);
        }
    }
} 