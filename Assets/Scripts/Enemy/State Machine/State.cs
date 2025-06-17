using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    [SerializeField] private List<Transition> _transitions;

    protected Player Target { get; set; }

    public void Enter(Player target)
    {
        if (enabled) return;

        Target = target;
        enabled = true;
        foreach (var transition in _transitions)
        {
            transition.enabled = true;
            transition.Init(target);
        }
        
        OnEnter();
    }
    
    public void Exit()
    {
        if (!enabled) return;
        
        OnExit();
        enabled = false;
        foreach (var transition in _transitions)
        {
            transition.enabled = false;
        }
    }

    public State GetNext()
    {
        return _transitions.Where(transition => transition.NeedTransit).Select(transition => transition.TargetState).FirstOrDefault();
    }
    
    protected virtual void OnEnter() { }
    protected virtual void OnExit() { }
}

