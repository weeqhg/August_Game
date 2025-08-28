using UnityEngine;
using System.Collections;
using UnityEngine.AI;
public class EnemyCombat : MonoBehaviour
{
    public enum AttackType { Melee, Ranged }

    [Header("Настройки атаки")]
    [SerializeField] private AttackType _attackType = AttackType.Melee;
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private float _rangedAttackRange = 5f;
    [SerializeField] private float _attackCooldown = 2f;
    [SerializeField] private float _retreatDistance = 3f;
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private float _attackAccuracy = 0.8f;

    [Header("Ссылки")]
    [SerializeField] private EnemyMove _enemyMove;
    [SerializeField] private Animator _animator;

    // Состояния
    private Transform _player;
    private float _attackTimer = 0f;
    private bool _isAttacking = false;
    private bool _isRetreating = false;
    private Vector3 _retreatTarget;

    private void Start()
    {
        if (_enemyMove == null)
            _enemyMove = GetComponent<EnemyMove>();

        if (_animator == null)
            _animator = GetComponent<Animator>();

        // Подписываемся на события движения
        _enemyMove.OnPlayerDetected += OnPlayerDetected;
        _enemyMove.OnPlayerLost += OnPlayerLost;
    }

    private void Update()
    {
        if (_player == null || !_enemyMove.IsChasing()) return;

        _attackTimer -= Time.deltaTime;

        if (_isRetreating)
        {
            HandleRetreat();
            return;
        }

        float distanceToPlayer = GetDistanceToPlayer();
        float currentAttackRange = GetCurrentAttackRange();

        if (distanceToPlayer <= currentAttackRange && _attackTimer <= 0 && !_isAttacking)
        {
            StartAttack();
        }
    }

    private void OnPlayerDetected(Transform player)
    {
        _player = player;
    }

    private void OnPlayerLost()
    {
        _player = null;
        _isAttacking = false;
        _isRetreating = false;
    }

    private void StartAttack()
    {
        _isAttacking = true;
        _enemyMove.StopMovement();

        // Анимация атаки
        if (_animator != null)
        {
            _animator.SetTrigger("Attack");
        }

        // Запускаем самую атаку через время анимации
        Invoke("PerformAttack", 0.3f);
    }

    private void PerformAttack()
    {
        if (_player == null) return;

        // Проверка точности
        if (Random.value <= _attackAccuracy)
        {
            //PlayerHealth playerHealth = _player.GetComponent<PlayerHealth>();
            //if (playerHealth != null)
            //{
                //playerHealth.TakeDamage(_attackDamage);
               // Debug.Log($"Нанесен урон: {_attackDamage}");
            //}
        }
        else
        {
            Debug.Log("Атака промахнулась!");
        }

        _attackTimer = _attackCooldown;
        _isAttacking = false;

        // Последствия атаки в зависимости от типа
        if (_attackType == AttackType.Melee)
        {
            StartRetreat();
        }
        else
        {
            ChangeRangedPosition();
        }

        _enemyMove.ResumeMovement();
    }

    private void StartRetreat()
    {
        _isRetreating = true;

        Vector3 retreatDirection = (transform.position - _player.position).normalized;
        retreatDirection = Quaternion.Euler(0, 0, Random.Range(-45f, 45f)) * retreatDirection;

        _retreatTarget = transform.position + retreatDirection * _retreatDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(_retreatTarget, out hit, _retreatDistance, NavMesh.AllAreas))
        {
            _retreatTarget = hit.position;
            _enemyMove.SetDestination(_retreatTarget);
        }
        else
        {
            _isRetreating = false;
        }
    }

    private void HandleRetreat()
    {
        if (_enemyMove.GetDistanceToTarget() <= _enemyMove.GetStoppingDistance())
        {
            _isRetreating = false;
        }
    }

    private void ChangeRangedPosition()
    {
        float angle = Random.Range(0f, 360f);
        Vector3 newPosition = _player.position + new Vector3(
            Mathf.Cos(angle) * _rangedAttackRange,
            Mathf.Sin(angle) * _rangedAttackRange,
            0
        );

        NavMeshHit hit;
        if (NavMesh.SamplePosition(newPosition, out hit, _rangedAttackRange, NavMesh.AllAreas))
        {
            _enemyMove.SetDestination(hit.position);
        }
    }

    private float GetCurrentAttackRange()
    {
        return _attackType == AttackType.Melee ? _attackRange : _rangedAttackRange;
    }

    private float GetDistanceToPlayer()
    {
        return _player != null ? Vector3.Distance(transform.position, _player.position) : float.MaxValue;
    }

    public void SetAttackType(AttackType type)
    {
        _attackType = type;
    }

    public void SetAttackParameters(float range, float cooldown, int damage)
    {
        _attackRange = range;
        _attackCooldown = cooldown;
        _attackDamage = damage;
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        if (_enemyMove != null)
        {
            _enemyMove.OnPlayerDetected -= OnPlayerDetected;
            _enemyMove.OnPlayerLost -= OnPlayerLost;
        }
    }
}