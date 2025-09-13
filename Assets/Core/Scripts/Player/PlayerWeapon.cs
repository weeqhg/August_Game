using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : Weapon
{
    private SaveSystem saveSystem;

    public override void Start()
    {
        base.Start();
        saveSystem = GameManager.Instance.Get<SaveSystem>();
        //LoadPlayerData();
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

        playerAccessoryWeapon.DropAllAccessories();

        int slots = weaponConfig.accessorySlots;
        playerAccessoryWeapon.accessoryConfig = new List<AccessoryConfig>(slots);

        for (int i = 0; i < slots; i++)
        {
            playerAccessoryWeapon.accessoryConfig.Add(null);
        }

        InitializeWeapon();
    }

    public void LoadPlayerData()
    {

        GameData data = saveSystem.GetCurrentGameData();

        Debug.Log(saveSystem);
        if (data != null)
        {
            weaponConfig = data.weaponConfig;
        }
        InitializeWeapon();

    }

    public void SaveGameData()
    {
        var gameData = saveSystem.GetCurrentGameData();

        if (gameData != null)
        {
            gameData.weaponConfig = this.weaponConfig;
        }

        saveSystem.SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
    }
}