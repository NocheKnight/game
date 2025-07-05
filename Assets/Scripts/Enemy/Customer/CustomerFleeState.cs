using UnityEngine;

public class CustomerFleeState : IState
{
    private readonly Customer _customer;
    private float _fleeTimer;

    public CustomerFleeState(Customer customer)
    {
        _customer = customer;
    }

    public void Enter()
    {
        Debug.Log($"{_customer.name} в панике убегает!");
        _fleeTimer = 10f;

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

        Vector3 fleeDirection = (_customer.transform.position - _customer.TheftLocation).normalized;
        Vector3 fleePoint = _customer.transform.position + fleeDirection * 15f;

        if (UnityEngine.AI.NavMesh.SamplePosition(fleePoint, out var hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            _customer.Agent.SetDestination(hit.position);
            }
        }
        else
        {
            Debug.LogWarning($"Customer {_customer.name}: NavMeshAgent не активен или не на NavMesh в FleeState");
        }
    }

    public void Update()
    {
        _fleeTimer -= Time.deltaTime;
        if (_fleeTimer <= 0f)
        {
            _customer.CalmDown();
        }
        
        // Обновляем анимацию на основе скорости движения
        UpdateAnimationBasedOnSpeed();
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