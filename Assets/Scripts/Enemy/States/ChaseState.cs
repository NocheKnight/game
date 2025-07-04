// D:/unity/projects/Kradylechka/Assets/Scripts/Enemy/States/ChaseState.cs
using UnityEngine;

public class ChaseState : IState
{
    private readonly GuardLogic _ai;

    public ChaseState(GuardLogic ai)
    {
        _ai = ai;
    }

    public void Enter()
    {
        Debug.Log("Вхожу в состояние Погони!");
        _ai.Agent.isStopped = false;
        _ai.Agent.speed = _ai.ChaseSpeed;
        // _ai.Animator.SetBool("IsChasing", true);
    }

    public void Update()
    {
        if (_ai.Player == null) return;

        // Всегда преследуем актуальную позицию игрока
        _ai.Agent.destination = _ai.Player.position;

        // Проверяем, не поймали ли мы игрока
        if (Vector3.Distance(_ai.transform.position, _ai.Player.position) < _ai.CatchDistance)
        {
            CatchPlayer();
        }
    }

    public void Exit()
    {
        // _ai.Animator.SetBool("IsChasing", false);
        Debug.Log("Выхожу из состояния Погони");
    }

    private void CatchPlayer()
    {
        Debug.Log("ИГРОК ПОЙМАН!");
        // Здесь логика проигрыша
        _ai.Agent.isStopped = true;
        // _ai.Animator.SetTrigger("CatchPlayer");
        Object.Destroy(_ai.Player.gameObject); // Например, удаляем игрока
    }
}