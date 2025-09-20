using DG.Tweening;
using System.Collections;
using UnityEngine;

public abstract class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected float maxHealth = 40f;
    [SerializeField] protected Sprite dieSprite;
    [SerializeField] protected AudioClip deathSound;
    [Range(0f, 1f)][SerializeField] protected float freezeChance = 0.3f;
    [SerializeField] protected float freezeDuration = 2f;

    [Header("Damage Effects")]
    [SerializeField] protected ParticleSystem burnEffect;
    [SerializeField] protected ParticleSystem freezeEffect;
    [SerializeField] protected Color freezeColor = Color.blue;

    protected float currentHealth = 100;
    protected SpriteRenderer spriteRenderer;
    protected Animator animator;
    protected Color originalColor;
    protected bool isDead = false;
    protected bool isBurning = false;
    protected bool isFrozen = false;

    protected Tween damageTween;
    protected Coroutine burnCoroutine;
    protected Coroutine freezeCoroutine;

    // Абстрактные свойства для компонентов
    protected abstract MonoBehaviour MovementComponent { get; }
    protected abstract MonoBehaviour WeaponComponent { get; }
    protected abstract MonoBehaviour SpecialComponent { get; } // Dash для игрока
    protected abstract GameObject ChildrenObject { get; }

    protected virtual void Start()
    {
        GetNeedComponent();
        currentHealth = maxHealth;
        originalColor = spriteRenderer.color;
    }

    private void GetNeedComponent()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    #region Реализация получения урона
    /// <summary>
    /// Метод реализующий получение урона от пули в скрипте Projectile
    /// Protected хранит в себе тип атаки (огонь, лед, яд и т.д.)
    /// и меняется в зависимости от типа аксессуара который экипирован на оружие
    /// Все остальные методы отвечают за урон от различных типов атак
    /// Действует как на игрока так и на врагов
    /// <summary>
    public virtual void TakeDamage(float damage, DamageType damageType, bool isCritical)
    {
        if (isDead) return;

        switch (damageType)
        {
            case DamageType.Normal:
                TakeDamageNormal(damage);
                break;
            case DamageType.Fire:
                TakeDamageFire(damage);
                break;
            case DamageType.Ice:
                TakeDamageIce(damage);
                break;
        }
    }


    protected virtual void TakeDamageNormal(float damage)
    {
        currentHealth -= damage;
        DamageFlash();
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void TakeDamageFire(float damage)
    {
        TakeDamageNormal(damage);

        if (isBurning && burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
        }

        burnCoroutine = StartCoroutine(BurnCoroutine());
    }

    protected virtual void TakeDamageIce(float damage)
    {
        TakeDamageNormal(damage);

        if (Random.value <= freezeChance && !isFrozen)
        {
            if (freezeCoroutine != null)
            {
                StopCoroutine(freezeCoroutine);
            }

            freezeCoroutine = StartCoroutine(FreezeCoroutine());
        }
    }
    #endregion

    #region Визуальные эффекты
    protected abstract IEnumerator BurnCoroutine();


    protected virtual void StopBurning()
    {
        isBurning = false;

        if (burnEffect != null)
        {
            burnEffect.Stop();
        }

        burnCoroutine = null;
    }

    protected virtual IEnumerator FreezeCoroutine()
    {
        isFrozen = true;

        // Сохраняем оригинальную скорость через абстрактный метод
        float originalSpeed = SaveOriginalSpeed();
        SetFrozenState(true);

        ApplyFreezeVisuals(true);

        yield return new WaitForSeconds(freezeDuration);

        Unfreeze(originalSpeed);
    }

    protected abstract float SaveOriginalSpeed();
    protected abstract void SetFrozenState(bool frozen);

    protected virtual void ApplyFreezeVisuals(bool freeze)
    {
        if (damageTween != null && damageTween.IsActive())
        {
            damageTween.Kill();
        }

        spriteRenderer.color = freeze ? freezeColor : originalColor;

        if (freezeEffect != null)
        {
            if (freeze)
                freezeEffect.Play();
            else
                freezeEffect.Stop();
        }

        if (animator != null)
        {
            animator.speed = freeze ? 0f : 1f;
        }
    }

    protected virtual void Unfreeze(float originalSpeed)
    {
        isFrozen = false;

        if (!isDead)
        {
            RestoreFromFreeze(originalSpeed);
        }

        ApplyFreezeVisuals(false);
    }

    protected abstract void RestoreFromFreeze(float originalSpeed);

    protected virtual void DamageFlash()
    {
        if (damageTween != null && damageTween.IsActive())
        {
            damageTween.Kill();
            spriteRenderer.color = isFrozen ? freezeColor : originalColor;
        }

        Color targetColor = isFrozen ? freezeColor : originalColor;

        damageTween = spriteRenderer.DOColor(Color.red, 0.1f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.Flash)
            .OnComplete(() =>
            {
                spriteRenderer.color = targetColor;
            });
    }
    #endregion

    #region Смерть
    public virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        DisableComponents();
        PlayDeathAnimation();

        if (spriteRenderer != null && dieSprite != null)
        {
            spriteRenderer.sprite = dieSprite;
        }
    }

    protected abstract void DisableComponents();
    protected abstract void PlayDeathAnimation();

    protected virtual void OnDestroy()
    {
        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
        }
        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }

        if (damageTween != null && damageTween.IsActive())
        {
            damageTween.Kill();
        }
    }
    #endregion
}