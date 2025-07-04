using UnityEngine;

public class SecurityGuard : Enemy
{
    [Header("Охранник")]
    [SerializeField] private float _backupCallRadius = 10f;
    [SerializeField] private AudioClip _backupCallSound;

    [Header("Особенности охранника")]
    [SerializeField] private float _enhancedSuspicionGrowthSpeed = 0.3f;
    [SerializeField] private float _stealthSuspicionMultiplier = 2.0f;
    [SerializeField] private float _theftSuspicionMultiplier = 5.0f;

    [Header("Порог внимания")]
    [SerializeField, Range(0f, 1f)] private float _attentionThreshold = 0.12f; // 12%

    public void CallForBackup()
    {
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
    }

    public void OnBackupCalled(Vector3 point)
    {
        // Можно реализовать: идти к точке, повышать тревогу и т.д.
    }

    public void Distract(Vector3 distractionPoint)
    {
        // Реакция на отвлечение (аналогично кассирше)
    }

    private new void CheckForPlayer()
    {
        if (SuspicionLevel < _attentionThreshold)
            return;
    }

    protected override void OnPlayerSeen()
    {
        if (SuspicionLevel < _attentionThreshold)
            return;
    }
    
    protected override void OnPlayerHeard(float distance, float noiseLevel)
    {
        float hearingChance = CalculateHearingChance(distance, noiseLevel) * 1.5f;
        if (UnityEngine.Random.value <= hearingChance)
        {
            AddSuspicion(_enhancedSuspicionGrowthSpeed * 0.7f * Time.deltaTime);
        }
    }
    
    protected override void OnPlayerOutOfRange()
    {
        // Подозрения спадают автоматически в базовом классе
    }
} 