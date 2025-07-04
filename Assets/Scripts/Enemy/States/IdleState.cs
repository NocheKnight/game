// D:/unity/projects/Kradylechka/Assets/Scripts/Enemy/States/IdleState.cs
using UnityEngine;

public class IdleState : IState
{
    private readonly GuardLogic _ai;

    public IdleState(GuardLogic ai)
    {
        _ai = ai;
    }

    public void Enter()
    {
        _ai.Agent.isStopped = true;
        // _ai.Animator.SetBool("IsIdle", true);
        Debug.Log("Вхожу в состояние Бездействия");
    }

    public void Update()
    {
        // В этом состоянии охранник ничего не делает, просто ждет.
    }

    public void Exit()
    {
        // _ai.Animator.SetBool("IsIdle", false);
        Debug.Log("Выхожу из состояния Бездействия");
    }
}