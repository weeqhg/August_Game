using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [HideInInspector] public WeaponConfig weaponConfig;

    protected int currentAmmo;
    protected float savedVolumeS;
    
    protected bool canShoot = true;
    protected bool isReloading = false;
    protected bool freeze = false;

    protected CameraShakeController cameraShakeController;
    protected Animator weaponAnimator;
    protected SpriteRenderer WeaponSpriteRenderer; 
    protected AccessoryWeapon AccessoryWeapon;
    protected Transform firePoint;


    public virtual void Start()
    {
        GetNeedComponent();
        UpdateVolume();
    }
    public void UpdateVolume()
    {
        savedVolumeS = PlayerPrefs.GetFloat("Sound", 1f);
    }
    private void GetNeedComponent()
    {
        cameraShakeController = GameManager.Instance.Get<CameraShakeController>();
        weaponAnimator = GetComponent<Animator>();
        WeaponSpriteRenderer = GetComponent<SpriteRenderer>();
        AccessoryWeapon = GetComponent<AccessoryWeapon>();
        firePoint = transform.GetChild(0);
    }
    public virtual void InitializeWeapon()
    {
        if (weaponConfig == null)
        {
            Debug.LogError("WeaponConfig не назначен!");
            return;
        }
        currentAmmo = weaponConfig.maxAmmo;
    }



    public void TryShoot()
    {
        if (currentAmmo <= 0)
        {
            // Автоматическая перезарядка при пустом магазине
            if (!isReloading)
            {
                StartReload();
            }
            return;
        }

        if (canShoot && !freeze)
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        if (weaponConfig == null || firePoint == null) return;

        // Расходуем патроны
        currentAmmo--;

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
            AudioSource.PlayClipAtPoint(weaponConfig.shootSound, firePoint.position, savedVolumeS);
        }

        // Эффект дульного вспышки
        if (weaponConfig.muzzleFlashEffect != null)
        {
            GameObject flash = Instantiate(weaponConfig.muzzleFlashEffect, firePoint.position, firePoint.rotation);
            ParticleSystem particleSystem = flash.GetComponent<ParticleSystem>();

            var mainModule = particleSystem.main;
            mainModule.startColor = weaponConfig.colorProjectile;

            flash.transform.SetParent(firePoint);
            flash.transform.localScale = Vector3.one;
        }

        // Анимация стрельбы
        if (weaponAnimator != null && !string.IsNullOrEmpty(weaponConfig.shootAnimationName))
        {
            weaponAnimator.Play(weaponConfig.shootAnimationName);
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
            DamageType damageType = DamageType.Normal;
            if (AccessoryWeapon != null &&
                AccessoryWeapon.accessoryConfig != null &&
                AccessoryWeapon.accessoryConfig.Count > 0)
            {
                // Ищем первый не-null аксессуар
                var activeAccessory = AccessoryWeapon.accessoryConfig.Find(cfg => cfg != null);
                if (activeAccessory != null)
                {
                    damageType = activeAccessory.damageType;
                }
            }

            projectileScript.Initialize(
                shootDirection,
                damageType, weaponConfig
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
        canShoot = false;
        yield return new WaitForSeconds(weaponConfig.fireRate);
        canShoot = true;
    }

    public void StartReload()
    {
        if (isReloading || currentAmmo >= weaponConfig.maxAmmo) return;

        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        canShoot = false;

        //Debug.Log("Перезарядка...");

        yield return new WaitForSeconds(weaponConfig.reloadTime);

        currentAmmo = weaponConfig.maxAmmo;
        isReloading = false;
        canShoot = true;

        //Debug.Log("Перезарядка завершена!");
    }

    public void ChangeFreeze(bool newFreeze)
    {
        freeze = newFreeze;
    }
}
