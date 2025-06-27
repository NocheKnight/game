using UnityEngine;

public class SecurityGuard : Enemy
{
    [Header("Охранник")]
    [SerializeField] private float _backupCallRadius = 10f;
    [SerializeField] private AudioClip _backupCallSound;

    public void CallForBackup()
    {
        // Находим всех охранников в радиусе и оповещаем их
        Collider[] hits = Physics.OverlapSphere(transform.position, _backupCallRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<SecurityGuard>(out var guard) && guard != this)
            {
                guard.OnBackupCalled(transform.position);
            }
        }
        if (_backupCallSound != null)
        {
            var audio = GetComponent<AudioSource>();
            if (audio != null) audio.PlayOneShot(_backupCallSound);
        }
        Debug.Log("Охранник зовёт подмогу!");
    }

    public void OnBackupCalled(Vector3 point)
    {
        // Можно реализовать: идти к точке, повышать тревогу и т.д.
        Debug.Log($"Охранник получил сигнал подмоги! Бежит к {point}");
    }

    public void Distract(Vector3 distractionPoint)
    {
        // Реакция на отвлечение (аналогично кассирше)
        var stateMachine = GetComponent<EnemyStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.SetPatrolDestination(distractionPoint);
        }
        Debug.Log("Охранник отвлечён!");
    }
} 