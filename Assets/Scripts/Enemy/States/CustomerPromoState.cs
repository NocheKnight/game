// D:/unity/projects/Kradylechka/Assets/Scripts/Enemy/States/CustomerPromoState.cs
using UnityEngine;

public class CustomerPromoState : IState
{
    private readonly Customer _customer;
    private float _interestTimer;

    public CustomerPromoState(Customer customer)
    {
        _customer = customer;
    }

    public void Enter()
    {
        Debug.Log($"{_customer.name} услышал про акцию и бежит к {_customer.PromoTargetLocation}!");
        _interestTimer = 15f;

        _customer.Agent.isStopped = false;
        _customer.Agent.speed = _customer.PromoSpeed;
        if (_customer.Animator != null) _customer.Animator.SetBool("IsRunning", true);

        _customer.Agent.SetDestination(_customer.PromoTargetLocation);
    }

    public void Update()
    {
        _interestTimer -= Time.deltaTime;
        if (_interestTimer <= 0f)
        {
            _customer.LoseInterestInPromo();
        }
    }

    public void Exit()
    {
        if (_customer.Animator != null) _customer.Animator.SetBool("IsRunning", false);
    }
}