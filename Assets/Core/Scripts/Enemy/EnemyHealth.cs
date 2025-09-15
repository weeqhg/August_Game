using DamageNumbersPro;
using UnityEngine;

public class EnemyHealth : Health
{
    private EnemyMove enemyMove;
    private EnemyWeapon enemyWeapon;
    private CircleCollider2D circleCollider;
    private GameObject children;
    [SerializeField, HideInInspector] private LevelManager levelManager;
    [SerializeField] private DamageNumber defaultDamageNumber;
    [SerializeField] private DamageNumber criticalDamageNumber;

    private DamageNumber currentNumber;
    protected override MonoBehaviour MovementComponent => enemyMove;
    protected override MonoBehaviour WeaponComponent => enemyWeapon;
    protected override MonoBehaviour SpecialComponent => null; // У врага нет специального компонента
    protected override GameObject ChildrenObject => children;

    protected override void Start()
    {
        enemyMove = GetComponent<EnemyMove>();
        enemyWeapon = GetComponentInChildren<EnemyWeapon>();
        circleCollider = GetComponent<CircleCollider2D>();
        children = transform.GetChild(0).gameObject;

        base.Start();
    }

    public override void TakeDamage(float damage, DamageType damageType, bool isCritical)
    {
        base.TakeDamage(damage, damageType, isCritical);

        currentNumber = isCritical ? criticalDamageNumber : defaultDamageNumber;
        currentNumber.Spawn(transform.position, damage, transform);
    }

    public void GetLevelManager(LevelManager newLevelManager)
    {
        levelManager = newLevelManager;
        Debug.Log(levelManager);
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
        enemyMove.enabled = false;
        circleCollider.enabled = false;
        animator.enabled = false;
        children.SetActive(false);
        EnemyCounter();
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
        if (levelManager != null)
            levelManager.CounterDiedEnemy();
    }
}