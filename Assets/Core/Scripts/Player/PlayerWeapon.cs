using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : Weapon
{
    private void Update()
    {
        HandleShootingInput();
    }

    private void HandleShootingInput()
    {
        if (_isReloading || !_canShoot) return;

        if (weaponConfig.isAutomatic)
        {
            if (Input.GetMouseButton(0))
            {
                TryShoot();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryShoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
        }
    }

    public override void PlayShootEffects()
    {
        _cameraShakeController.ShakeCamera(weaponConfig.screenShakeIntensity, 0.1f);
        base.PlayShootEffects();
    }

    public void ChangeWeaponConfig(WeaponConfig newConfig)
    {
        weaponConfig = newConfig;

        accessoryWeapon.DropAllAccessories();

        int slots = weaponConfig.accessorySlots;
        accessoryWeapon.accessoryConfig = new List<AccessoryConfig>(slots);

        for (int i = 0; i < slots; i++)
        {
            accessoryWeapon.accessoryConfig.Add(null);
        }

        InitializeWeapon();
    }
}