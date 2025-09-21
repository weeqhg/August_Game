using DamageNumbersPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : Health
{
    [Header("Настройка выпадения дропа")]
    [SerializeField] private GameObject _keyPrefab;
    [SerializeField] private Slider healthSlider;
    [SerializeField][Range(0f, 1f)] private float _dropChance = 0.1f;
    [SerializeField] private int _countKey;

    private EnemyMove enemyMove;
    private EnemyWeapon enemyWeapon;
    private CircleCollider2D circleCollider;
    private GameObject children;
    [SerializeField] private bool isSpawn;
    [SerializeField] private Spawn spawn;

    [SerializeField, HideInInspector] private LevelManager levelManager;
    [SerializeField] private DamageNumber defaultDamageNumber;
    [SerializeField] private DamageNumber criticalDamageNumber;

    private DamageNumber currentNumber;

    protected override MonoBehaviour MovementComponent => enemyMove;
    protected override MonoBehaviour WeaponComponent => enemyWeapon;
    protected override MonoBehaviour SpecialComponent => null; // У врага нет специального компонента
    protected override GameObject ChildrenObject => children;

    private BossAttackController bossAttackController;
    public float GetCurrentHealth() => currentHealth;

    protected override void Start()
    {
        enemyMove = GetComponent<EnemyMove>();
        enemyWeapon = GetComponentInChildren<EnemyWeapon>();
        circleCollider = GetComponent<CircleCollider2D>();
        bossAttackController = GetComponent<BossAttackController>();
        children = transform.GetChild(0).gameObject;
        base.Start();
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void Initialize(Spawn newSpawn)
    {
        if (newSpawn != null)
            spawn = newSpawn;
    }

    public override void TakeDamage(float damage, DamageType damageType, bool newIsCritical)
    {
        base.TakeDamage(damage, damageType, newIsCritical);
        currentNumber = newIsCritical ? criticalDamageNumber : defaultDamageNumber;
        currentNumber.Spawn(transform.position, damage, transform);
    }
    protected override void DamageFlash()
    {
        base.DamageFlash();
        if (healthSlider != null)
            healthSlider.value = currentHealth;
    }

    protected override IEnumerator BurnCoroutine()
    {
        isBurning = true;

        if (burnEffect != null)
        {
            burnEffect.Play();
        }

        float elapsedTime = 0f;

        while (elapsedTime < 3f)
        {
            //Убрать магические цифры
            currentNumber = defaultDamageNumber;
            currentNumber.Spawn(transform.position, 1f, transform);
            TakeDamageNormal(1f);
            yield return new WaitForSeconds(0.5f);
            elapsedTime += 0.5f;
        }

        StopBurning();
    }
    public void GetLevelManager(LevelManager newLevelManager)
    {
        levelManager = newLevelManager;
        //Debug.Log(levelManager);
    }
    protected override float SaveOriginalSpeed()
    {
        return 0f; // У врагов может не быть метода GetMoveSpeed
    }

    protected override void SetFrozenState(bool frozen)
    {
        if (frozen)
        {
            enemyMove.enabled = false;
            enemyWeapon.ChangeFreeze(true);
        }
    }

    protected override void RestoreFromFreeze(float originalSpeed)
    {
        enemyMove.enabled = true;
        enemyWeapon.ChangeFreeze(false);
    }

    protected override void DisableComponents()
    {
        if (bossAttackController != null)
        {
            bossAttackController.enabled = false;
            bossAttackController.InterruptAttack();
            bossAttackController.EndGame();
        }
        enemyMove.enabled = false;
        circleCollider.enabled = false;
        animator.enabled = false;
        children.SetActive(false);
        EnemyCounter();
        DropKey();
    }

    protected override void PlayDeathAnimation()
    {
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
    }

    private void EnemyCounter()
    {
        if (levelManager != null && isSpawn)
            levelManager.CounterDiedEnemy();
    }

    private void DropKey()
    {
        if (Random.value <= _dropChance && spawn != null)
        {
            GameObject gameObject = Instantiate(_keyPrefab, transform.position, Quaternion.identity);
            spawn.AddDropKey(gameObject);
            InteractKey interactKey = gameObject.GetComponentInChildren<InteractKey>();
            interactKey.Initialize(_countKey);
        }
    }
}