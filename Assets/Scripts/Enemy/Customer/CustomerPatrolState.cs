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
        
        _customer.Agent.isStopped = false;
        _customer.Agent.speed = _customer.PatrolSpeed;
        
        GoToNextPoint();
    }

    public void Update()
    {
        if (_customer.PatrolPoints == null || _customer.PatrolPoints.Length == 0) return;

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

        if (!_customer.Agent.pathPending && _customer.Agent.remainingDistance < 0.5f)
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
        _currentPointIndex = (_currentPointIndex + 1) % _customer.PatrolPoints.Length;
        var destination = _customer.PatrolPoints[_currentPointIndex];
        if (destination != null)
        {
            _customer.Agent.SetDestination(destination.position);
        }
    }
}