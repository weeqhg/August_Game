using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyMove : MonoBehaviour
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




    [Header("Настройки атаки")]
    [SerializeField] private AttackType _attackType = AttackType.Melee;
    [SerializeField] private float _attackRange = 0.5f;
    [SerializeField] private float _rangedAttackRange = 1f;
    [SerializeField] private float _attackCooldown = 2f;
    [SerializeField] private float _retreatDistance = 2f;
    [SerializeField] private int _attackDamage = 10;

    private float _attackTimer = 0f;
    private bool _isRetreating = false;
    private Vector3 _retreatTarget;
    private float _currentAttackRange;
    private EnemyWeapon _weapon;
    private EnemyRoundWeapon _weaponRound;
    public enum AttackType
    {
        Melee,
        Ranged
    }


    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _weapon = GetComponentInChildren<EnemyWeapon>();
        _weaponRound = GetComponentInChildren<EnemyRoundWeapon>();

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

        // Определяем дистанцию атаки в зависимости от типа
        _currentAttackRange = _attackType == AttackType.Melee ? _attackRange : _rangedAttackRange;
        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        // Проверяем, видим ли еще игрока
        if (!CanSeePlayer())
        {
            if (!SearchForPlayer())
            {
                ReturnToWander();
                return;
            }
            return;
        }

        // Обновляем таймер атаки
        _attackTimer -= Time.deltaTime;

        // Если отступаем - обрабатываем отступ
        if (_isRetreating)
        {
            HandleRetreat();
            return;
        }

        //Debug.Log(distanceToPlayer);
        // Если в радиусе атаки - атакуем
        // Для дальнего боя - поддерживаем дистанцию
        if (_attackType == AttackType.Ranged)
        {
            HandleRangedCombat(distanceToPlayer, _currentAttackRange);
        }
        // Для ближнего боя
        else
        {
            HandleMeleeCombat(distanceToPlayer, _currentAttackRange);
        }
    }
    private void HandleRangedCombat(float distanceToPlayer, float attackRange)
    {
        // Если слишком далеко - приближаемся
        if (distanceToPlayer > attackRange * 1.2f)
        {
            _agent.SetDestination(_player.position);
            _agent.isStopped = false;
        }
        // Если слишком близко - отходим
        else if (distanceToPlayer < attackRange * 0.8f)
        {
            Vector3 retreatDirection = (transform.position - _player.position).normalized;
            Vector3 optimalPosition = _player.position + retreatDirection * attackRange;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(optimalPosition, out hit, attackRange, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
                _agent.isStopped = false;
            }
        }
        // Если на оптимальной дистанции - атакуем или меняем позицию
        else if (_attackTimer <= 0)
        {
            if (Random.value < 0.7f) // 70% chance to attack
            {
                AttackPlayer();
            }
            else
            {
                ChangeRangedPosition();
            }
        }
        // Двигаемся для уклонения
        else if (ShouldReposition())
        {
            ChangeRangedPosition();
        }
    }

    private void HandleMeleeCombat(float distanceToPlayer, float attackRange)
    {
        // Если в радиусе атаки - атакуем
        if (distanceToPlayer <= attackRange && _attackTimer <= 0)
        {
            AttackPlayer();
        }
        // Если слишком далеко - продолжаем преследование
        else if (distanceToPlayer > attackRange)
        {
            _agent.SetDestination(_player.position);
            _agent.isStopped = false;
        }
        // Если близко но кулдаун не прошел - держим дистанцию
        else
        {
            MaintainDistance(attackRange);
        }
    }

    private bool ShouldReposition()
    {
        // Меняем позицию каждые 3-5 секунд
        return Random.value < 0.1f && _attackTimer > _attackCooldown * 0.5f;
    }

    private void HandleRetreat()
    {
        // Проверяем, достигли ли точки отступления
        if (_agent.remainingDistance <= _agent.stoppingDistance && !_agent.pathPending)
        {
            _isRetreating = false;
            _attackTimer = _attackCooldown * 0.5f; // Короткий перерыв после отступления
        }
    }

    private void AttackPlayer()
    {
        // Останавливаемся для атаки
        _agent.isStopped = true;

        // Выполняем атаку
        PerformAttack();

        // Запускаем кулдаун
        _attackTimer = _attackCooldown;

        // Для ближнего боя - отступаем после атаки
        if (_attackType == AttackType.Melee)
        {
            StartRetreat();
        }
        // Для дальнего боя - меняем позицию
        else
        {
            ChangeRangedPosition();
        }
    }

    private void PerformAttack()
    {
        Debug.Log($"Атакую игрока! Урон: {_attackDamage}");



        _weapon.EnemyShoot();


        // Здесь реализация нанесения урона
        //PlayerHealth playerHealth = _player.GetComponent<PlayerHealth>();
        //if (playerHealth != null)
        {
            //playerHealth.TakeDamage(_attackDamage);
        }

        // Воспроизведение анимации атаки
        if (_animator != null)
        {
            //_animator.SetTrigger("Attack");
        }
    }

    private void StartRetreat()
    {
        _isRetreating = true;

        // Выбираем случайное направление для отступления
        Vector3 retreatDirection = (transform.position - _player.position).normalized;
        retreatDirection = Quaternion.Euler(0, 0, Random.Range(-45f, 45f)) * retreatDirection;

        _retreatTarget = transform.position + retreatDirection * _retreatDistance;

        // Ищем валидную позицию для отступления
        NavMeshHit hit;
        if (NavMesh.SamplePosition(_retreatTarget, out hit, _retreatDistance, NavMesh.AllAreas))
        {
            _retreatTarget = hit.position;
            _agent.SetDestination(_retreatTarget);
            _agent.isStopped = false;
        }
        else
        {
            _isRetreating = false;
        }
    }

    private void ChangeRangedPosition()
    {
        Debug.Log("Меняю позицию для дальней атаки");

        // Выбираем случайный угол вокруг игрока (не слишком близко к текущей позиции)
        float minAngle = 90f;
        float maxAngle = 270f;
        float angle = Random.Range(minAngle, maxAngle);

        // Вычисляем новую позицию
        Vector3 directionToPlayer = (_player.position - transform.position).normalized;
        float currentAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        angle = currentAngle + angle;

        Vector3 newPosition = _player.position + new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * _rangedAttackRange * 0.8f,
            Mathf.Sin(angle * Mathf.Deg2Rad) * _rangedAttackRange * 0.8f,
            0
        );

        // Ищем валидную позицию на NavMesh
        NavMeshHit hit;
        int maxAttempts = 5;

        for (int i = 0; i < maxAttempts; i++)
        {
            if (NavMesh.SamplePosition(newPosition, out hit, _rangedAttackRange, NavMesh.AllAreas))
            {
                // Проверяем, что новая позиция не слишком близко к текущей
                if (Vector3.Distance(transform.position, hit.position) > _rangedAttackRange * 0.3f)
                {
                    _agent.SetDestination(hit.position);
                    _agent.isStopped = false;
                    Debug.Log($"Новая позиция найдена: {hit.position}");
                    return;
                }
            }

            // Пробуем другую позицию
            angle = Random.Range(0f, 360f);
            newPosition = _player.position + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * _rangedAttackRange,
                Mathf.Sin(angle * Mathf.Deg2Rad) * _rangedAttackRange,
                0
            );
        }

        Debug.LogWarning("Не удалось найти подходящую позицию для атаки");
    }

    private void MaintainDistance(float optimalDistance)
    {
        Vector3 directionToPlayer = (_player.position - transform.position).normalized;
        Vector3 optimalPosition = _player.position - directionToPlayer * optimalDistance;

        _agent.SetDestination(optimalPosition);
    }

    private void ReturnToWander()
    {
        _weaponRound.SetChasingState(false);
        _isChasing = false;
        _isRetreating = false;
        _agent.speed = _patrolSpeed;
        _agent.isStopped = false;
        FindRandomPoint();
        Debug.Log("Игрок потерян. Возвращаюсь к бродяжничеству.");
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
            _weaponRound.SetChasingState(true, _player);
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

    private void UpdateAnimations()
    {
        if (_animator == null) return;

        _animator.SetBool(_runAnimation, _agent.velocity.magnitude > 0.1f && !_isWaiting);
        //_animator.SetBool("IsChasing", _isChasing);
        //_animator.SetBool("IsRetreating", _isRetreating);
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