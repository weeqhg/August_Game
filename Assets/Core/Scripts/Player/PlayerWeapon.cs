using UnityEngine;
using System.Collections;
using DG.Tweening.Core.Easing;

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
}