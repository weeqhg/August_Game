using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyMove : MonoBehaviour
{
    [Header("Настройки перемещения")]
    [SerializeField] private float _patrolRadius = 10f;
    [SerializeField] private float _waitTimeAtPoint = 2f;
    [SerializeField] private float _patrolSpeed = 2f;
    [SerializeField] private float _chaseSpeed = 4f;
    [SerializeField] private float _pointReachedDistance = 0.5f;

    [Header("Настройки обнаружения")]
    [SerializeField] private float _detectionRadius = 5f;
    [SerializeField] private LayerMask _obstacleLayer = 1;

    [Header("Визуальные настройки")]
    [SerializeField] private bool _faceMovementDirection = true;
    [SerializeField] private string _runAnimation = "isRun";

    // Компоненты
    protected NavMeshAgent _agent;
    protected Animator _animator;
    protected SpriteRenderer _spriteRenderer;

    // Состояния
    protected Transform _player;
    protected Vector3 _currentTarget;
    protected bool _isWaiting = false;
    protected bool _isChasing = false;
    protected float _currentWaitTime;
    protected bool _isInitialized = false;

    // Бродяжничество
    protected float _wanderTimer = 0f;
    protected float _wanderTime = 3f;

    // События для коммуникации
    public System.Action<Transform> OnPlayerDetected;
    public System.Action OnPlayerLost;

    protected virtual void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        SetupNavMeshAgent();
    }

    public virtual void Initialization(Transform player)
    {
        if (player != null)
        {
            _player = player;
            _isInitialized = true;
        }
    }

    protected virtual void SetupNavMeshAgent()
    {
        _agent.speed = _patrolSpeed;
        _agent.stoppingDistance = _pointReachedDistance;
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    protected virtual void Update()
    {
        if (!_isInitialized || _player == null) return;

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

    protected virtual void HandleWanderState()
    {
        if (_isWaiting)
        {
            _currentWaitTime -= Time.deltaTime;
            if (_currentWaitTime <= 0)
            {
                _isWaiting = false;
                FindRandomPoint();
            }
            return;
        }

        _wanderTimer += Time.deltaTime;
        if (_wanderTimer >= _wanderTime)
        {
            _wanderTimer = 0f;
            _wanderTime = Random.Range(2f, 5f);
            FindRandomPoint();
        }

        if (_agent.remainingDistance <= _agent.stoppingDistance && !_agent.pathPending)
        {
            StartWaiting();
        }
    }


    public float GetStoppingDistance()
    {
        return _pointReachedDistance;
    }

    public float GetDistanceToTarget()
    {
        return Vector3.Distance(transform.position, _player.position);
    }


    protected virtual void HandleChaseState()
    {
        if (_player == null)
        {
            ReturnToWander();
            return;
        }

        if (!CanSeePlayer())
        {
            if (!SearchForPlayer())
            {
                ReturnToWander();
                return;
            }
        }

        _agent.SetDestination(_player.position);
    }

    protected virtual bool SearchForPlayer()
    {
        return _agent.remainingDistance > _agent.stoppingDistance;
    }

    protected virtual void CheckForPlayer()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        if (distanceToPlayer <= _detectionRadius && CanSeePlayer())
        {
            StartChase();
        }
    }

    protected virtual bool CanSeePlayer()
    {
        if (_player == null) return false;

        Vector2 directionToPlayer = (_player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToPlayer,
            distanceToPlayer,
            _obstacleLayer
        );

        return hit.collider == null || hit.collider.CompareTag("Player");
    }

    protected virtual void FindRandomPoint()
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
            Invoke("FindRandomPoint", 1f);
        }
    }

    protected virtual void StartWaiting()
    {
        _isWaiting = true;
        _currentWaitTime = _waitTimeAtPoint;
        _agent.isStopped = true;
    }

    public virtual void StartChase()
    {
        _isChasing = true;
        _isWaiting = false;
        _agent.speed = _chaseSpeed;
        _agent.isStopped = false;

        OnPlayerDetected?.Invoke(_player);
    }

    public virtual void ReturnToWander()
    {
        _isChasing = false;
        _agent.speed = _patrolSpeed;
        _agent.isStopped = false;
        FindRandomPoint();

        OnPlayerLost?.Invoke();
    }

    protected virtual void UpdateAnimations()
    {
        if (_animator == null) return;

        _animator.SetBool(_runAnimation, _agent.velocity.magnitude > 0.1f && !_isWaiting);
    }

    protected virtual void UpdateFacingDirection()
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

    public virtual void StopMovement()
    {
        _agent.isStopped = true;
        _isWaiting = true;
    }

    public virtual void ResumeMovement()
    {
        _agent.isStopped = false;
        _isWaiting = false;
    }

    public virtual bool IsChasing()
    {
        return _isChasing;
    }

    public virtual void SetDestination(Vector3 destination)
    {
        _agent.SetDestination(destination);
    }

    public virtual float GetDistanceToPlayer()
    {
        return _player != null ? Vector3.Distance(transform.position, _player.position) : float.MaxValue;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _patrolRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        if (Application.isPlaying && _currentTarget != Vector3.zero)
        {
            Gizmos.color = _isChasing ? Color.magenta : Color.green;
            Gizmos.DrawLine(transform.position, _currentTarget);
            Gizmos.DrawWireSphere(_currentTarget, 0.3f);
        }
    }
}