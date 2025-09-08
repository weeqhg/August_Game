using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Настройки оружия")]
    public WeaponConfig weaponConfig;

    [Header("Ссылки")]
    [SerializeField] private Transform firePoint;
    public SpriteRenderer weaponSpriteRenderer { get; private set; }

    public bool _canShoot { get; private set; } = true;
    public int _currentAmmo { get; private set; }
    public bool _isReloading { get; private set; } = false;

    public CameraShakeController _cameraShakeController { get; private set; }

    public Animator _weaponAnimator { get; private set; }

    public AttackType attackType { get; private set; }


    private void Start()
    {
        _cameraShakeController = GameManager.Instance.Get<CameraShakeController>();
        _weaponAnimator = GetComponent<Animator>();
        weaponSpriteRenderer = GetComponent<SpriteRenderer>();

        InitializeWeapon();
    }

    private void InitializeWeapon()
    {
        if (weaponConfig == null)
        {
            Debug.LogError("WeaponConfig не назначен!");
            return;
        }

        // Устанавливаем спрайт оружия
        if (weaponSpriteRenderer != null && weaponConfig.weaponSprite != null)
        {
            weaponSpriteRenderer.sprite = weaponConfig.weaponSprite;
        }

        attackType = weaponConfig.attackType;
        // Инициализируем боезапас
        _currentAmmo = weaponConfig.maxAmmo;

        Debug.Log($"Оружие инициализировано: {weaponConfig.weaponName}");
    }

    public void TryShoot()
    {
        if (_currentAmmo <= 0)
        {
            // Автоматическая перезарядка при пустом магазине
            if (!_isReloading)
            {
                StartReload();
            }
            return;
        }

        if (_canShoot)
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        if (weaponConfig == null || firePoint == null) return;

        // Расходуем патроны
        _currentAmmo--;

        // Визуальные эффекты
        PlayShootEffects();

        // Создаем снаряды
        for (int i = 0; i < weaponConfig.projectilesPerShot; i++)
        {
            CreateProjectile();
        }

        // Задержка перед следующим выстрелом
        StartCoroutine(ShootCooldown());
    }

    public virtual void PlayShootEffects()
    {
        // Звук выстрела
        if (weaponConfig.shootSound != null)
        {
            AudioSource.PlayClipAtPoint(weaponConfig.shootSound, firePoint.position);
        }

        // Эффект дульного вспышки
        if (weaponConfig.muzzleFlashEffect != null)
        {
            GameObject flash = Instantiate(weaponConfig.muzzleFlashEffect, firePoint.position, firePoint.rotation);
            flash.transform.SetParent(firePoint);
            flash.transform.localScale = Vector3.one;
        }

        // Анимация стрельбы
        if (_weaponAnimator != null && !string.IsNullOrEmpty(weaponConfig.shootAnimationName))
        {
            _weaponAnimator.Play(weaponConfig.shootAnimationName);
        }
    }

    private void CreateProjectile()
    {
        if (weaponConfig.projectilePrefab == null) return;

        // Рассчитываем направление с разбросом
        Vector2 shootDirection = CalculateShotDirection();

        // Создаем снаряд
        GameObject projectile = Instantiate(
            weaponConfig.projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        // Настраиваем снаряд
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(
                shootDirection,
                weaponConfig.projectileSpeed,
                weaponConfig.damage, weaponConfig.destroyTime
            );
        }
        else
        {
            // Fallback для Rigidbody2D
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = shootDirection * weaponConfig.projectileSpeed;
            }
        }
    }

    private Vector2 CalculateShotDirection()
    {
        Vector2 baseDirection = -(transform.position - firePoint.position).normalized;

        // Добавляем разброс
        if (weaponConfig.spreadAngle > 0)
        {
            float spread = Random.Range(-weaponConfig.spreadAngle, weaponConfig.spreadAngle);
            Quaternion spreadRotation = Quaternion.Euler(0, 0, spread);
            return spreadRotation * baseDirection;
        }

        return baseDirection;
    }

    private IEnumerator ShootCooldown()
    {
        _canShoot = false;
        yield return new WaitForSeconds(weaponConfig.fireRate);
        _canShoot = true;
    }

    public void StartReload()
    {
        if (_isReloading || _currentAmmo >= weaponConfig.maxAmmo) return;

        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        _isReloading = true;
        _canShoot = false;

        Debug.Log("Перезарядка...");

        yield return new WaitForSeconds(weaponConfig.reloadTime);

        _currentAmmo = weaponConfig.maxAmmo;
        _isReloading = false;
        _canShoot = true;

        Debug.Log("Перезарядка завершена!");
    }

    // Метод для смены конфигурации оружия
    public void ChangeWeaponConfig(WeaponConfig newConfig)
    {
        weaponConfig = newConfig;
        InitializeWeapon();
    }
}
