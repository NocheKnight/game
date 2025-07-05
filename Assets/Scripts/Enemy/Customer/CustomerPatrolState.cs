using UnityEngine;

public class CustomerPatrolState : IState
{
    private readonly Customer _customer;
    private int _currentPointIndex;
    private float _waitTimer;

    public bool IsWaiting => _waitTimer > 0;

    public CustomerPatrolState(Customer customer)
    {
        _customer = customer;
    }

    public void Enter()
    {
        if (_customer.PatrolPoints == null || _customer.PatrolPoints.Length == 0) return;
        
        // Проверяем, что агент активен и на NavMesh
        if (_customer.Agent != null && _customer.Agent.isActiveAndEnabled && _customer.Agent.isOnNavMesh)
        {
            _customer.Agent.isStopped = false;
            _customer.Agent.speed = _customer.PatrolSpeed;
            
            GoToNextPoint();
        }
        else
        {
            Debug.LogWarning($"Customer {_customer.name}: NavMeshAgent не активен или не на NavMesh");
        }
    }

    public void Update()
    {
        if (_customer.PatrolPoints == null || _customer.PatrolPoints.Length == 0) return;

        // Проверяем, что агент активен и на NavMesh
        if (_customer.Agent == null || !_customer.Agent.isActiveAndEnabled || !_customer.Agent.isOnNavMesh)
        {
            return;
        }

        if (IsWaiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0)
            {
                _customer.Agent.isStopped = false;
                GoToNextPoint();
            }
            return;
        }

        // Безопасная проверка remainingDistance
        if (!_customer.Agent.pathPending && _customer.Agent.hasPath && _customer.Agent.remainingDistance < 0.5f)
        {
            _customer.Agent.isStopped = true;
            _waitTimer = _customer.WaitTime;
            // _customer.Animator.SetBool("IsWalking", false);
        }
    }

    public void Exit()
    {
        // Clean up animator state if you have one
        // if (_customer.Animator != null) _customer.Animator.SetBool("IsWalking", false);
    }

    private void GoToNextPoint()
    {
        if (_customer.Agent == null || !_customer.Agent.isActiveAndEnabled || !_customer.Agent.isOnNavMesh)
        {
            return;
        }

        _currentPointIndex = (_currentPointIndex + 1) % _customer.PatrolPoints.Length;
        var destination = _customer.PatrolPoints[_currentPointIndex];
        if (destination != null)
        {
            _customer.Agent.SetDestination(destination.position);
        }
    }
}