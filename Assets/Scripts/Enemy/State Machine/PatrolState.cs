using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class PatrolState : State
{
    [Header("Настройки патрулирования")]
    [SerializeField] private Transform[] _patrolPoints;
    [SerializeField] private float _patrolSpeed = 2f;
    [SerializeField] private float _waitTimeAtPoint = 2f;
    [SerializeField] private float _rotationSpeed = 5f;
    
    private int _currentPointIndex = 0;
    private float _waitTimer = 0f;
    private bool _isWaiting = false;
    private Enemy _enemy;
    
    protected override void OnEnter()
    {
        _enemy = GetComponent<Enemy>();
        
        if (_patrolPoints == null || _patrolPoints.Length == 0)
        {
            _patrolPoints = new Transform[1];
            GameObject point = new GameObject("PatrolPoint");
            point.transform.position = transform.position;
            _patrolPoints[0] = point.transform;
        }
        
        _currentPointIndex = 0;
        _waitTimer = 0f;
        _isWaiting = false;
    }
    
    private void Update()
    {
        if (_enemy.IsAlerted)
            return;
            
        if (_isWaiting)
        {
            WaitAtPoint();
        }
        else
        {
            MoveToNextPoint();
        }
    }
    
    private void MoveToNextPoint()
    {
        if (_patrolPoints.Length == 0) return;
        
        Transform targetPoint = _patrolPoints[_currentPointIndex];
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
        
        float distance = Vector3.Distance(transform.position, targetPoint.position);
        if (distance > 0.5f)
        {
            transform.position += direction * _patrolSpeed * Time.deltaTime;
        }
        else
        {
            // Достигли точки, начинаем ждать
            _isWaiting = true;
            _waitTimer = _waitTimeAtPoint;
        }
    }
    
    private void WaitAtPoint()
    {
        _waitTimer -= Time.deltaTime;
        
        if (_waitTimer <= 0f)
        {
            _currentPointIndex = (_currentPointIndex + 1) % _patrolPoints.Length;
            _isWaiting = false;
        }
    }
    
    public void SetPatrolPoints(Transform[] points)
    {
        _patrolPoints = points;
    }
    
    // Методы для отладки
    private void OnDrawGizmosSelected()
    {
        if (_patrolPoints == null) return;
        
        Gizmos.color = Color.green;
        for (int i = 0; i < _patrolPoints.Length; i++)
        {
            if (_patrolPoints[i] != null)
            {
                Gizmos.DrawWireSphere(_patrolPoints[i].position, 0.5f);
                
                // Линии между точками
                if (i < _patrolPoints.Length - 1 && _patrolPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(_patrolPoints[i].position, _patrolPoints[i + 1].position);
                }
                else if (i == _patrolPoints.Length - 1 && _patrolPoints[0] != null)
                {
                    Gizmos.DrawLine(_patrolPoints[i].position, _patrolPoints[0].position);
                }
            }
        }
    }
} 