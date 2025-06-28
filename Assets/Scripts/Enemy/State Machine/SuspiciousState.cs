using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class SuspiciousState : State
{
    [Header("Настройки подозрительного состояния")]
    [SerializeField] private float _investigationSpeed = 1.0f;
    [SerializeField] private float _rotationSpeed = 6f;
    [SerializeField] private float _investigationTime = 5f;
    [SerializeField] private float _lookAroundSpeed = 2f;
    
    private Enemy _enemy;
    private Vector3 _suspiciousPosition;
    private float _investigationTimer;
    private bool _isLookingAround = false;
    private float _lookTimer = 0f;
    private int _lookDirection = 0;
    
    protected override void OnEnter()
    {
        _enemy = GetComponent<Enemy>();
        
        // Запоминаем позицию, где возникло подозрение
        if (_enemy.LastKnownPosition != null)
        {
            _suspiciousPosition = _enemy.LastKnownPosition.position;
        }
        else if (Target != null)
        {
            _suspiciousPosition = Target.transform.position;
        }
        else
        {
            _suspiciousPosition = transform.position;
        }
        
        _investigationTimer = _investigationTime;
        _isLookingAround = false;
        _lookTimer = 0f;
        _lookDirection = 0;
        
        Debug.Log($"{gameObject.name} стал подозрительным в позиции {_suspiciousPosition}");
    }
    
    private void Update()
    {
        if (_enemy == null) return;
        
        // Если игрок обнаружен, сразу переходим в преследование
        if (_enemy.IsAlerted)
        {
            return;
        }
        
        // Если подозрения исчезли, возвращаемся к патрулированию
        if (!_enemy.IsSuspicious)
        {
            return;
        }
        
        _investigationTimer -= Time.deltaTime;
        
        if (_investigationTimer <= 0f)
        {
            // Время расследования истекло, возвращаемся к патрулированию
            _enemy.ReduceSuspicion(0.5f);
            return;
        }
        
        InvestigateArea();
    }
    
    private void InvestigateArea()
    {
        if (_isLookingAround)
        {
            LookAround();
        }
        else
        {
            MoveToSuspiciousPosition();
        }
    }
    
    private void MoveToSuspiciousPosition()
    {
        Vector3 direction = (_suspiciousPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, _suspiciousPosition);
        
        if (distance > 1f)
        {
            // Двигаемся к подозрительной позиции медленно и осторожно
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
            
            transform.position += direction * _investigationSpeed * Time.deltaTime;
        }
        else
        {
            // Достигли позиции, начинаем осматриваться
            _isLookingAround = true;
            _lookTimer = 0f;
        }
    }
    
    private void LookAround()
    {
        _lookTimer += Time.deltaTime * _lookAroundSpeed;
        
        if (_lookTimer >= 1f)
        {
            _lookDirection = (_lookDirection + 1) % 4;
            _lookTimer = 0f;
        }
        
        // Поворачиваемся в разные стороны
        Vector3 lookDirection = Vector3.zero;
        switch (_lookDirection)
        {
            case 0: lookDirection = transform.forward; break;
            case 1: lookDirection = transform.right; break;
            case 2: lookDirection = -transform.forward; break;
            case 3: lookDirection = -transform.right; break;
        }
        
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }
    
    // Методы для отладки
    private void OnDrawGizmosSelected()
    {
        if (_enemy != null)
        {
            // Подозрительная позиция
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_suspiciousPosition, 0.5f);
            
            // Зона расследования
            Gizmos.color = new Color(1f, 0.5f, 0f); // Оранжевый цвет
            Gizmos.DrawWireSphere(_suspiciousPosition, 3f);
        }
    }
} 