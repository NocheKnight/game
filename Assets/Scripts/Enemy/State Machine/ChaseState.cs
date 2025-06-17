using UnityEngine;

public class ChaseState : State
{
    [Header("Настройки преследования")]
    [SerializeField] private float _chaseSpeed = 4f;
    [SerializeField] private float _rotationSpeed = 8f;
    [SerializeField] private float _catchDistance = 2f;
    [SerializeField] private float _loseDistance = 15f;
    
    private Enemy _enemy;
    private Vector3 _lastKnownPosition;
    private bool _hasReachedLastPosition = false;
    
    protected override void OnEnter()
    {
        _enemy = GetComponent<Enemy>();
        _hasReachedLastPosition = false;
        
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
        
        // Если игрок все еще виден, обновляем позицию
        if (_enemy.IsAlerted && Target != null)
        {
            _lastKnownPosition = Target.transform.position;
            _hasReachedLastPosition = false;
        }
        
        // Если игрок потерян и мы достигли последней позиции, переходим в поиск
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
            if (distanceToPlayer <= _catchDistance)
            {
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
        
        // Двигаемся к цели
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance > 0.5f)
        {
            transform.position += direction * _chaseSpeed * Time.deltaTime;
        }
        else
        {
            // Достигли последней известной позиции
            _hasReachedLastPosition = true;
        }
    }
    
    private void CatchPlayer()
    {
        if (Target != null)
        {
            // Игрок пойман - добавляем штраф
            Target.AddFine(1000);
            Target.AddCrimeRate(100);
            
            // Сбрасываем состояние врага
            _enemy.ResetToStartPosition();
            
            Debug.Log("Игрок пойман! Штраф: 1000₽");
        }
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