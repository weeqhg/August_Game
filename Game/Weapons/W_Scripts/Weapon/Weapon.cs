using UnityEngine;
using System.Collections;
using DG.Tweening.Core.Easing;

public class Weapon : MonoBehaviour
{
    [Header("Настройки оружия")]
    public WeaponConfig weaponConfig;

    [Header("Ссылки")]
    public Transform firePoint;
    public SpriteRenderer weaponSpriteRenderer;

    private bool _canShoot = true;
    private int _currentAmmo;
    private bool _isReloading = false;

    private CameraShakeController _cameraShakeController;


    private void Awake()
    {
        GameManager.Instance.Register(this);
    }
    private void Start()
    {
        _cameraShakeController = GameManager.Instance.Get<CameraShakeController>();
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

        // Инициализируем боезапас
        _currentAmmo = weaponConfig.maxAmmo;

        Debug.Log($"Оружие инициализировано: {weaponConfig.weaponName}");
    }

    private void Update()
    {
        HandleShootingInput();
    }

    private void HandleShootingInput()
    {
        if (_isReloading || !_canShoot) return;

        if (weaponConfig.isAutomatic)
        {
            // Автоматическая стрельба
            if (Input.GetMouseButton(0))
            {
                TryShoot();
            }
        }
        else
        {
            // Одиночная стрельба
            if (Input.GetMouseButtonDown(0))
            {
                TryShoot();
            }
        }

        // Перезарядка
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
        }
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

    private void PlayShootEffects()
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

        // Тряска камеры
        _cameraShakeController.ShakeCamera(weaponConfig.screenShakeIntensity, 0.1f);

        // Анимация стрельбы
        //if (weaponAnimator != null && !string.IsNullOrEmpty(weaponConfig.shootAnimationName))
        //{
            //weaponAnimator.Play(weaponConfig.shootAnimationName);
        //}
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
                weaponConfig.damage
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
        Vector2 baseDirection = - (transform.position - firePoint.position).normalized;
        //Vector2 baseDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - firePoint.position).normalized;

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

    // Методы для получения информации об оружии
    public int GetCurrentAmmo() => _currentAmmo;
    public int GetMaxAmmo() => weaponConfig.maxAmmo;
    public bool IsReloading() => _isReloading;
    public string GetWeaponName() => weaponConfig?.weaponName ?? "No Weapon";

    // Метод для добавления патронов
    public void AddAmmo(int amount)
    {
        _currentAmmo = Mathf.Min(_currentAmmo + amount, weaponConfig.maxAmmo);
    }
}