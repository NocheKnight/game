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

        _customer.Agent.isStopped = false;
        _customer.Agent.speed = _customer.FleeSpeed;
        _customer.Animator.SetBool("IsRunning", true);

        Vector3 fleeDirection = (_customer.transform.position - _customer.TheftLocation).normalized;
        Vector3 fleePoint = _customer.transform.position + fleeDirection * 15f;

        if (UnityEngine.AI.NavMesh.SamplePosition(fleePoint, out var hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            _customer.Agent.SetDestination(hit.position);
        }
    }

    public void Update()
    {
        _fleeTimer -= Time.deltaTime;
        if (_fleeTimer <= 0f)
        {
            _customer.CalmDown();
        }
    }

    public void Exit()
    {
        _customer.Animator.SetBool("IsRunning", false);
    }
}