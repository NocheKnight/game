using UnityEngine;

public class PlayerDetectedTransition : Transition
{
    [SerializeField] private float _detectionThreshold = 0.8f;
    
    private Enemy _enemy;
    
    protected void OnEnable()
    {
        base.OnEnable();
        _enemy = GetComponent<Enemy>();
    }
    
    private void Update()
    {
        if (_enemy != null)
        {
            // Переходим в преследование, если игрок обнаружен (уровень подозрений достиг порога)
            NeedTransit = _enemy.IsAlerted && _enemy.SuspicionLevel >= _detectionThreshold;
        }
    }
} 