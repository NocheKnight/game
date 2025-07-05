using UnityEngine;

public class CustomerReportState : IState
{
    private readonly Customer _customer;
    private SecurityGuard _targetGuard;
    private float _reportTimer;
    private const float REPORT_DURATION = 15f;

    public CustomerReportState(Customer customer)
    {
        _customer = customer;
    }

    public void Enter()
    {
        Debug.Log($"{_customer.name} бежит к охраннику сообщить о краже!");
        _reportTimer = REPORT_DURATION;

        // Ищем ближайшего охранника
        _targetGuard = FindNearestGuard();
        
        if (_targetGuard != null)
        {
            // Проверяем, что агент активен и на NavMesh
            if (_customer.Agent != null && _customer.Agent.isActiveAndEnabled && _customer.Agent.isOnNavMesh)
            {
                _customer.Agent.isStopped = false;
                _customer.Agent.speed = _customer.FleeSpeed;
                
                // Включаем анимацию бега
                if (_customer.Animator != null)
                {
                    _customer.Animator.SetBool("IsWalking", false);
                    _customer.Animator.SetBool("IsRunning", true);
                    _customer.Animator.SetFloat("Speed", _customer.FleeSpeed);
                }

                // Идем к охраннику
                _customer.Agent.SetDestination(_targetGuard.transform.position);
            }
            else
            {
                Debug.LogWarning($"Customer {_customer.name}: NavMeshAgent не активен или не на NavMesh в ReportState");
            }
        }
        else
        {
            Debug.LogWarning($"Customer {_customer.name}: Не найден охранник для доклада!");
        }
    }

    public void Update()
    {
        _reportTimer -= Time.deltaTime;
        
        // Если время истекло, успокаиваемся
        if (_reportTimer <= 0f)
        {
            _customer.CalmDown();
            return;
        }
        
        // Обновляем анимацию на основе скорости движения
        UpdateAnimationBasedOnSpeed();
        
        // Проверяем, дошли ли до охранника
        if (_targetGuard != null && _customer.Agent != null && _customer.Agent.isActiveAndEnabled)
        {
            float distanceToGuard = Vector3.Distance(_customer.transform.position, _targetGuard.transform.position);
            
            if (distanceToGuard < 2f) // В пределах 2 метров от охранника
            {
                ReportToGuard();
            }
        }
    }

    public void Exit()
    {
        // Выключаем анимацию бега
        if (_customer.Animator != null)
        {
            _customer.Animator.SetBool("IsWalking", false);
            _customer.Animator.SetBool("IsRunning", false);
            _customer.Animator.SetFloat("Speed", 0f);
        }
        
        _targetGuard = null;
    }
    
    private SecurityGuard FindNearestGuard()
    {
        SecurityGuard[] guards = Object.FindObjectsOfType<SecurityGuard>();
        
        if (guards.Length == 0)
        {
            return null;
        }
        
        SecurityGuard nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (var guard in guards)
        {
            float distance = Vector3.Distance(_customer.transform.position, guard.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = guard;
            }
        }
        
        return nearest;
    }
    
    private void ReportToGuard()
    {
        if (_targetGuard == null) return;
        
        Debug.Log($"{_customer.name} сообщает охраннику о краже!");
        
        // Вызываем метод отвлечения у охранника
        _targetGuard.Distract(_customer.TheftLocation);
        
        // Также можно добавить подозрение через GuardLogic, если он используется
        var guardLogic = _targetGuard.GetComponent<GuardLogic>();
        if (guardLogic != null)
        {
            guardLogic.AddSuspicion(50f, _customer.TheftLocation);
        }
        
        // Успокаиваемся после доклада
        _customer.CalmDown();
    }
    
    private void UpdateAnimationBasedOnSpeed()
    {
        if (_customer.Animator == null || _customer.Agent == null) return;

        float currentSpeed = _customer.Agent.velocity.magnitude;
        float speedThreshold = 0.1f;

        if (currentSpeed > speedThreshold)
        {
            // Если движемся быстро, включаем анимацию бега
            _customer.Animator.SetBool("IsWalking", false);
            _customer.Animator.SetBool("IsRunning", true);
            _customer.Animator.SetFloat("Speed", currentSpeed);
        }
        else
        {
            // Если стоим, выключаем анимации движения
            _customer.Animator.SetBool("IsWalking", false);
            _customer.Animator.SetBool("IsRunning", false);
            _customer.Animator.SetFloat("Speed", 0f);
        }
    }
} 