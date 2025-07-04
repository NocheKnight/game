// D:/unity/projects/Kradylechka/Assets/Scripts/Enemy/State Machine/InvestigateState.cs
using UnityEngine;

public class InvestigateState : IState
{
    private readonly GuardLogic _guard;
    private float _waitTimer;
    private const float WaitTimeAtDestination = 4f;

    public InvestigateState(GuardLogic guard)
    {
        _guard = guard;
    }

    public void Enter()
    {
        Debug.Log($"{_guard.name} is investigating a noise at {_guard.LastKnownPosition}.");
        _guard.Agent.isStopped = false;
        _guard.Agent.speed = _guard.InvestigateSpeed;
        _waitTimer = WaitTimeAtDestination;

        _guard.Agent.SetDestination(_guard.LastKnownPosition);
    }

    public void Update()
    {
        if (!_guard.Agent.pathPending && _guard.Agent.remainingDistance < 0.5f)
        {
            _guard.Agent.isStopped = true;
            _waitTimer -= Time.deltaTime;

            if (_waitTimer <= 0)
            {
                _guard.Enemy.ReduceSuspicion(0.3f); 
            }
        }
    }

    public void Exit()
    {
        if (_guard.Agent != null && _guard.Agent.isOnNavMesh)
        {
            _guard.Agent.isStopped = false;
        }
    }
}