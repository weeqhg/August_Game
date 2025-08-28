using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Настройки патрулирования")]
    [SerializeField] private float _patrolRadius = 10f;
    [SerializeField] private float _waitTimeAtPoint = 2f;
    [SerializeField] private float _patrolSpeed = 2f;
    [SerializeField] private float _chaseSpeed = 4f;
    [SerializeField] private float _pointReachedDistance = 0.5f;

    [Header("Настройки обнаружения")]
    [SerializeField] private float _detectionRadius = 5f;
    [SerializeField] private LayerMask _playerLayer = 1;
    [SerializeField] private LayerMask _obstacleLayer = 1;

    [Header("Визуальные настройки")]
    [SerializeField] private bool _faceMovementDirection = true;
    [SerializeField] private string _runAnimation = "isRun";

    private Animator _animator;
    private NavMeshAgent _agent;
    private Transform _player;
    private Vector3 _currentTarget;
    private bool _isWaiting = false;
    private bool _isChasing = false;
    private float _currentWaitTime;
    private SpriteRenderer _spriteRenderer;

    // Новые переменные для бродячего поведения
    private float _wanderTimer = 0f;
    private float _wanderTime = 3f; // Время между сменой точек при бродяжничестве


    private bool _isInitialized = false;
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        SetupNavMeshAgent();
        FindRandomPoint();
    }


    private void SetupNavMeshAgent()
    {
        _agent.speed = _patrolSpeed;
        _agent.stoppingDistance = _pointReachedDistance;
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    private void Update()
    {
        if (_isChasing)
        {
            HandleChaseState();
        }
        else
        {
            HandleWanderState();
            CheckForPlayer();
        }

        UpdateAnimations();
        UpdateFacingDirection();
    }

    private void HandleWanderState()
    {
        // Бродяжничество: меняем точку через случайные интервалы
        _wanderTimer += Time.deltaTime;
        if (_wanderTimer >= _wanderTime)
        {
            _wanderTimer = 0f;
            _wanderTime = Random.Range(2f, 5f); // Случайный интервал
            FindRandomPoint();
        }
    }

    private void HandleChaseState()
    {
        if (_player == null)
        {
            ReturnToWander();
            return;
        }

        // Проверяем, видим ли еще игрока
        if (!CanSeePlayer())
        {
            // Ищем игрока непродолжительное время перед возвратом к бродяжничеству
            if (!SearchForPlayer())
            {
                ReturnToWander();
                return;
            }
        }

        // Продолжаем преследование
        _agent.SetDestination(_player.position);
    }

    private bool SearchForPlayer()
    {
        // Краткий поиск игрока в последней известной позиции
        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            return false;
        }
        return true;
    }

    private void CheckForPlayer()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        if (distanceToPlayer <= _detectionRadius && CanSeePlayer())
        {
            StartChase();
        }
    }

    private bool CanSeePlayer()
    {
        if (_player == null) return false;

        Debug.Log("Проверка на видимость");
        Vector2 directionToPlayer = (_player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        // Проверка линии зрения
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToPlayer,
            distanceToPlayer,
            _obstacleLayer
        );

        // Если луч не наткнулся на препятствие - видим игрока
        return hit.collider == null || hit.collider.CompareTag("Player");
    }

    private void FindRandomPoint()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * _patrolRadius;
        randomPoint.z = 0;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, _patrolRadius, NavMesh.AllAreas))
        {
            _currentTarget = hit.position;
            _agent.SetDestination(_currentTarget);
        }
        else
        {
            // Если не нашли валидную точку, пробуем еще раз
            Invoke("FindRandomPoint", 1f);
        }
    }

    private void StartChase()
    {
        _isChasing = true;
        _isWaiting = false;
        _agent.speed = _chaseSpeed;
        _agent.isStopped = false;

        Debug.Log("Игрок обнаружен! Начинаю преследование!");
    }

    private void ReturnToWander()
    {
        _isChasing = false;
        _agent.speed = _patrolSpeed;
        FindRandomPoint();

        Debug.Log("Игрок потерян. Возвращаюсь к бродяжничеству.");
    }

    private void UpdateAnimations()
    {
        if (_animator == null) return;

        if (_agent.velocity.magnitude > 0.1f && !_isWaiting)
        {
            _animator.SetBool(_runAnimation, true);
        }
        else
        {
            _animator.SetBool(_runAnimation, false);
        }
    }

    private void UpdateFacingDirection()
    {
        if (!_faceMovementDirection || _spriteRenderer == null) return;

        if (_agent.velocity.x > 0.1f)
        {
            _spriteRenderer.flipX = false;
        }
        else if (_agent.velocity.x < -0.1f)
        {
            _spriteRenderer.flipX = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _player = collision.gameObject.transform;
        }
    }

    // Методы для визуальной отладки
    private void OnDrawGizmosSelected()
    {
        // Радиус патрулирования
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _patrolRadius);

        // Радиус обнаружения
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        // Текущая цель
        if (Application.isPlaying)
        {
            Gizmos.color = _isChasing ? Color.magenta : Color.green;
            Gizmos.DrawLine(transform.position, _currentTarget);
            Gizmos.DrawWireSphere(_currentTarget, 0.3f);
        }
    }

    public void SetPatrolRadius(float radius)
    {
        _patrolRadius = Mathf.Max(1f, radius);
    }

    public void SetDetectionRadius(float radius)
    {
        _detectionRadius = Mathf.Max(1f, radius);
    }

    // Для внешнего управления
    public void StopPatrol()
    {
        _agent.isStopped = true;
        _isWaiting = true;
    }

    public void ResumePatrol()
    {
        _agent.isStopped = false;
        _isWaiting = false;
    }

    // Новые методы для управления состоянием
    public bool IsChasing()
    {
        return _isChasing;
    }

    public void SetChaseTarget(Transform target)
    {
        _player = target;
        StartChase();
    }
}