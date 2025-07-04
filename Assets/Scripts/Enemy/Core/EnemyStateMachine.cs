// D:/unity/projects/Kradylechka/Assets/Scripts/Enemy/EnemyStateMachine.cs

using System;
using System.Collections.Generic;

public class EnemyStateMachine
{
    public IState CurrentState { get; private set; }

    public event Action<IState> OnStateChanged;

    private List<Transition> _transitions = new List<Transition>();

    private List<Transition> _anyTransitions = new List<Transition>();

    private class Transition
    {
        public IState From { get; }
        public IState To { get; }
        public Func<bool> Condition { get; }

        public Transition(IState from, IState to, Func<bool> condition)
        {
            From = from;
            To = to;
            Condition = condition;
        }
    }

    public void Update()
    {
        var transition = GetTransition();
        if (transition != null)
        {
            SetState(transition.To);
        }
        
        CurrentState?.Update();
    }

    public void SetState(IState state)
    {
        if (CurrentState == state) return;

        CurrentState?.Exit();
        CurrentState = state;
        
        OnStateChanged?.Invoke(CurrentState);
        
        CurrentState.Enter();
    }

    public void AddTransition(IState from, IState to, Func<bool> condition)
    {
        var transition = new Transition(from, to, condition);
        _transitions.Add(transition);
    }
    
    public void AddAnyTransition(IState to, Func<bool> condition)
    {
        var transition = new Transition(null, to, condition); // 'from' is null for any state
        _anyTransitions.Add(transition);
    }

    private Transition GetTransition()
    {
        foreach (var transition in _anyTransitions)
        {
            if (transition.Condition())
                return transition;
        }
        
        foreach (var transition in _transitions)
        {
            if (transition.From == CurrentState && transition.Condition())
                return transition;
        }

        return null;
    }
}