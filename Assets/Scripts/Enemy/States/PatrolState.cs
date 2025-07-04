// D:/unity/projects/Kradylechka/Assets/Scripts/Enemy/States/PatrolState.cs
using UnityEngine;

public class PatrolState : IState
{
    private readonly GuardLogic _ai;
    private int _currentPatrolPointIndex;

    public PatrolState(GuardLogic ai)
    {
        _ai = ai;
    }

    public void Enter()
    {
        Debug.Log("Вхожу в состояние Патрулирования");
        _ai.Agent.isStopped = false;
        _ai.Agent.speed = _ai.PatrolSpeed;
        // _ai.Animator.SetBool("IsPatrolling", true);

        // Находим ближайшую точку для начала патрулирования
        float closestDist = float.MaxValue;
        for (int i = 0; i < _ai.PatrolPoints.Length; i++)
        {
            float dist = Vector3.Distance(_ai.transform.position, _ai.PatrolPoints[i].position);
            if (dist < closestDist)
            {
                closestDist = dist;
                _currentPatrolPointIndex = i;
            }
        }
        SetNextDestination();
    }

    public void Update()
    {
        if (!_ai.Agent.pathPending && _ai.Agent.remainingDistance < 0.5f)
        {
            SetNextDestination();
        }
    }

    public void Exit()
    {
        // _ai.Animator.SetBool("IsPatrolling", false);
        Debug.Log("Выхожу из состояния Патрулирования");
    }

    private void SetNextDestination()
    {
        if (_ai.PatrolPoints.Length == 0) return;
        _ai.Agent.destination = _ai.PatrolPoints[_currentPatrolPointIndex].position;
        _currentPatrolPointIndex = (_currentPatrolPointIndex + 1) % _ai.PatrolPoints.Length;
    }
}