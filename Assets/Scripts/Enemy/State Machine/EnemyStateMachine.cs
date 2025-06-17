using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyStateMachine : MonoBehaviour
{
    [SerializeField] private State _firstState;
    
    private Player _target;
    private State _currentState;
    private Enemy _enemy;

    public State Current => _currentState;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
    }

    public void Initialize(Player target)
    {
        _target = target;
        ResetState(_firstState);
    }

    private void Update()
    {
        if (_currentState == null)
            return;

        var nextState = _currentState.GetNext();
        if (nextState != null)
        {
            Transit(nextState);
        }
    }

    private void Transit(State nextState)
    {
        if (_currentState != null)
        {
            _currentState.Exit();
        }

        _currentState = nextState;
        if (_currentState != null)
        {
            _currentState.Enter(_target);
        }
    }

    private void ResetState(State startState)
    {
        _currentState = startState;

        if (_currentState != null)
        {
            _currentState.Enter(_target);
        }
    }
    
    public void ForceState(State newState)
    {
        if (newState != null)
        {
            Transit(newState);
        }
    }
    
    public void ResetToFirstState()
    {
        ResetState(_firstState);
    }
}
