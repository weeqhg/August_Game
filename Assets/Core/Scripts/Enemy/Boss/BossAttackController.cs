
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackController : MonoBehaviour
{
    [System.Serializable]
    public class AttackPattern
    {
        public string attackName;
        public float minCooldown = 2f;
        public float maxCooldown = 5f;
        public int weight = 1; // Вес для случайного выбора
        public bool isAvailable = true;
    }

    [Header("Attack Patterns")]
    [SerializeField] private List<AttackPattern> attackPatterns = new List<AttackPattern>();
    [SerializeField] private int phase = 1;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private EnemyHealth bossHealth;
    [SerializeField] private EnemyMove bossMovement;

    private bool isAttacking = false;
    private Coroutine currentAttackCoroutine;
    [SerializeField] private Spawn spawn;
    private EnemySetting enemySetting;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spawn = FindAnyObjectByType<Spawn>();
        StartCoroutine(AttackRoutine());
        enemySetting = GetComponent<EnemySetting>();
    }

    private void Update()
    {
        UpdatePhaseBasedOnHealth();
    }

    public void EndGame()
    {
        spawn.SpawnPortalEndGame();
        InterruptAttack();
        this.enabled = false;
    }


    private void UpdatePhaseBasedOnHealth()
    {
        int newPhase = bossHealth.GetCurrentHealth() switch
        {
            <= 30 => 3,
            <= 60 => 2,
            _ => 1
        };

        if (newPhase != phase)
        {
            phase = newPhase;
            OnPhaseChanged(phase);
        }
    }

    private void OnPhaseChanged(int newPhase)
    {
        Debug.Log($"Босс перешел в фазу {newPhase}!");
        // Можно добавить визуальные эффекты, анимации
    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (!isAttacking && player != null)
            {
                yield return new WaitForSeconds(GetRandomCooldown());

                if (!isAttacking) // Двойная проверка
                {
                    ChooseAndExecuteAttack();
                }
            }
            yield return null;
        }
    }

    private void ChooseAndExecuteAttack()
    {
        var availableAttacks = attackPatterns.FindAll(a => a.isAvailable);
        if (availableAttacks.Count == 0) return;

        // Взвешенный случайный выбор
        AttackPattern chosenAttack = WeightedRandomSelection(availableAttacks);

        isAttacking = true;
        currentAttackCoroutine = StartCoroutine(ExecuteAttack(chosenAttack));
    }

    private AttackPattern WeightedRandomSelection(List<AttackPattern> attacks)
    {
        int totalWeight = 0;
        foreach (var attack in attacks)
        {
            totalWeight += attack.weight;
        }

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var attack in attacks)
        {
            currentWeight += attack.weight;
            if (randomValue < currentWeight)
            {
                return attack;
            }
        }

        return attacks[0];
    }

    private float GetRandomCooldown()
    {
        return Random.Range(1f, 3f) / phase; // Уменьшаем кд с фазой
    }

    private IEnumerator ExecuteAttack(AttackPattern attack)
    {
        Debug.Log($"Босс использует: {attack.attackName}");

        switch (attack.attackName)
        {
            case "CircleProjectiles":
                yield return CircleProjectilesAttack();
                break;


            case "SummonMinions":
                yield return SummonMinionsAttack();
                break;

            case "ChargeAttack":
                yield return ChargeAttack();
                break;

        }

        isAttacking = false;
    }

    // Отменяет текущую атаку
    public void InterruptAttack()
    {
        if (isAttacking && currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
            isAttacking = false;
            Debug.Log("Атака босса прервана!");
        }
    }
    // Добавить в класс BossAttackController

    #region Attack Implementations

    [Header("Circle Projectiles Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectilesCount = 16;

    private IEnumerator CircleProjectilesAttack()
    {
        bossMovement.PauseMovement(false);

        // Анимация подготовки
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < projectilesCount * phase; i++)
        {
            float angle = i * (360f / (projectilesCount * phase));
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            SpriteRenderer spriteRenderer = projectile.GetComponent<SpriteRenderer>();
            Projectile projectile1 = projectile.GetComponent<Projectile>();
            projectile1.Initialize(direction, DamageType.Normal, enemySetting.currentWeapon);

            // Поворачиваем projectile в направлении движения
            //float rotationAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            //projectile.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);

            yield return new WaitForSeconds(0.1f);
        }

        bossMovement.PauseMovement(false);
    }


    [Header("Minion Summoning")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int minionsPerSummon = 2;
    [SerializeField] private float summonRadius = 3f;

    private IEnumerator SummonMinionsAttack()
    {
        bossMovement.PauseMovement(true);

        // Анимация призыва
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < minionsPerSummon * phase; i++)
        {
            Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * summonRadius;
            GameObject minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
        }

        bossMovement.PauseMovement(false);
    }


    [Header("Charge Attack")]
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeDistance = 8f;
    [SerializeField] private float chargeDamage = 15f;
    [SerializeField] private ParticleSystem chargeParticles;

    private IEnumerator ChargeAttack()
    {
        Vector2 chargeDirection = (player.position - transform.position).normalized;
        Vector2 chargeTarget = (Vector2)transform.position + chargeDirection * chargeDistance;

        // Подготовка к заряду
        chargeParticles.Play();
        yield return new WaitForSeconds(0.5f);

        // Заряд
        float chargeTimer = 0f;
        float maxChargeTime = chargeDistance / chargeSpeed;

        while (chargeTimer < maxChargeTime)
        {
            chargeTimer += Time.deltaTime;
            transform.position = Vector2.Lerp(transform.position, chargeTarget, chargeSpeed * Time.deltaTime);

            // Проверка столкновения с игроком
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    hit.GetComponent<PlayerHealth>().TakeDamage(chargeDamage, DamageType.Normal, false);
                    break;
                }
            }

            yield return null;
        }

        chargeParticles.Stop();
    }

    #endregion
}