using UnityEngine;
using System.Collections;
using DG.Tweening.Core.Easing;

public class EnemyWeapon : Weapon
{

    public override void Start()
    {
        base.Start();
        InitializeWeapon();
        AccessoryWeapon.InitializeAccessory();
    }
    public void EnemyShoot()
    {
        if (isReloading || !canShoot) return;

        if (weaponConfig.isAutomatic)
        {
            StartCoroutine(LineShots());
        }
        else
        {
            TryShoot();
        }
    }

    private IEnumerator LineShots()
    {
        if (weaponConfig.maxAmmo <= 0) yield break;

        for (int i = 0; i < weaponConfig.maxAmmo; i++)
        {
            // Проверяем можно ли стрелять на каждом выстреле
            if (isReloading || !canShoot) yield break;

            TryShoot();

            // Ждем перед следующим выстрелом, кроме последнего
            if (i < weaponConfig.maxAmmo - 1)
            {
                yield return new WaitForSeconds(weaponConfig.fireRate);
            }
        }
    }
}
