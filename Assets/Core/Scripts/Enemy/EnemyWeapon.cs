using UnityEngine;
using System.Collections;
using DG.Tweening.Core.Easing;

public class EnemyWeapon : Weapon
{
    public void EnemyShoot()
    {
        if (_isReloading || !_canShoot) return;

        if (weaponConfig.isAutomatic)
        {
            TryShoot();
        }
        else
        {
            TryShoot();
        }
    }
}
