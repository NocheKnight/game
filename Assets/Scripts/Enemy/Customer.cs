using UnityEngine;
using System.Collections.Generic;

public class Customer : MonoBehaviour
{
    [Header("Патрульные точки покупателя")]
    [SerializeField] private Transform[] _patrolPoints;
    [SerializeField] private float _speed = 1.5f;
    [SerializeField] private float _waitTime = 2f;
    [Header("Параметры зрения")]
    [SerializeField] private float _viewRadius = 7f;
    [SerializeField] private float _viewAngle = 90f;
    [SerializeField] private float _eyeHeight = 1.0f;

    private int _currentPoint = 0;
    private float _waitTimer = 0f;
    private bool _isWaiting = false;

    // --- Механика акции ---
    private Vector3? _promoTarget = null;
    private float _promoTimer = 0f;
    private static readonly List<Customer> _allCustomers = new List<Customer>();

    private Animator _animator;

    private void Awake()
    {
        _allCustomers.Add(this);
        _animator = GetComponentInChildren<Animator>();
    }
    private void OnDestroy()
    {
        _allCustomers.Remove(this);
    }

    public static void AnnouncePromo(Vector3 promoPoint, float duration = 5f)
    {
        foreach (var customer in _allCustomers)
        {
            customer.GoToPromo(promoPoint, duration);
        }
    }

    public void GoToPromo(Vector3 promoPoint, float duration)
    {
        _promoTarget = promoPoint;
        _promoTimer = duration;
        Debug.Log($"Покупатель услышал про акцию! Бежит к {promoPoint}");
    }

    private void Start()
    {
        if (_patrolPoints == null || _patrolPoints.Length == 0)
        {
            _patrolPoints = new Transform[1];
            GameObject point = new GameObject("CustomerPatrolPoint");
            point.transform.position = transform.position;
            _patrolPoints[0] = point.transform;
        }
    }

    private void Update()
    {
        bool isMoving = false;
        if (_promoTarget.HasValue)
        {
            Vector3 dir = (_promoTarget.Value - transform.position);
            if (dir.magnitude > 0.2f)
            {
                // Плавный поворот к точке акции
                Quaternion targetRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 8f * Time.deltaTime);

                transform.position += dir.normalized * _speed * 1.5f * Time.deltaTime;
                isMoving = true;
            }
            else
            {
                // Стоит у полки, имитируя толпу
                isMoving = false;
            }
            _promoTimer -= Time.deltaTime;
            if (_promoTimer <= 0f)
            {
                _promoTarget = null;
            }
            SetAnimation(isMoving);
            return;
        }

        if (_isWaiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _isWaiting = false;
                _currentPoint = (_currentPoint + 1) % _patrolPoints.Length;
            }
            isMoving = false;
        }
        else
        {
            isMoving = MoveToPoint();
        }
        SetAnimation(isMoving);
    }

    private bool MoveToPoint()
    {
        Transform target = _patrolPoints[_currentPoint];
        Vector3 dir = (target.position - transform.position);
        if (dir.magnitude > 0.2f)
        {
            // Плавный поворот к точке патрулирования
            Quaternion targetRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 8f * Time.deltaTime);

            transform.position += dir.normalized * _speed * Time.deltaTime;
            return true;
        }
        else
        {
            _isWaiting = true;
            _waitTimer = _waitTime;
            return false;
        }
    }

    private void SetAnimation(bool isWalking)
    {
        if (_animator != null)
        {
            _animator.SetBool("IsWalking", isWalking);
        }
    }

    // --- Механика сдачи игрока ---
    public static void AlertNearbyCustomers(Vector3 theftPosition, Player player, float radius = 8f)
    {
        foreach (var customer in _allCustomers)
        {
            if (Vector3.Distance(customer.transform.position, theftPosition) <= radius)
            {
                if (customer.CanSeePlayer(player))
                {
                    customer.ReportTheft(player);
                }
            }
        }
    }

    private bool CanSeePlayer(Player player)
    {
        Vector3 toPlayer = (player.transform.position - transform.position);
        float distance = toPlayer.magnitude;
        if (distance > _viewRadius) return false;
        float angle = Vector3.Angle(transform.forward, toPlayer.normalized);
        if (angle > _viewAngle * 0.5f) return false;
        Vector3 eyePos = transform.position + Vector3.up * _eyeHeight;
        Vector3 playerPos = player.transform.position + Vector3.up * _eyeHeight;
        Ray ray = new Ray(eyePos, (playerPos - eyePos).normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, distance + 0.5f))
        {
            if (hit.collider.GetComponent<Player>() != null)
                return true;
        }
        return false;
    }

    public void ReportTheft(Player player)
    {
        Debug.Log("Покупатель заметил кражу и зовёт охрану!");
        // Ищем ближайшего охранника
        SecurityGuard[] guards = GameObject.FindObjectsOfType<SecurityGuard>();
        SecurityGuard nearest = null;
        float minDist = float.MaxValue;
        foreach (var guard in guards)
        {
            float dist = Vector3.Distance(transform.position, guard.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = guard;
            }
        }
        if (nearest != null)
        {
            nearest.CallForBackup();
        }
    }

    public bool IsBlockingVision => _promoTarget.HasValue;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        float viewRadius = _viewRadius;
        float viewAngle = _viewAngle;
        int segments = 30;
        Vector3 origin = transform.position + Vector3.up * _eyeHeight;
        Vector3 forward = transform.forward;
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        float halfAngle = viewAngle * 0.5f;
        Vector3 prevPoint = origin + Quaternion.Euler(0, -halfAngle, 0) * forward * viewRadius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + (viewAngle * i) / segments;
            Vector3 nextPoint = origin + Quaternion.Euler(0, angle, 0) * forward * viewRadius;
            Gizmos.DrawLine(origin, nextPoint);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
#endif
} 