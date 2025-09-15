using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : Weapon
{
    private SaveSystem saveSystem;
    public PlayerAccessoryWeapon PlayerAccessoryWeapon { get; private set; }

    public override void Start()
    {
        base.Start();
        saveSystem = GameManager.Instance.Get<SaveSystem>();
        PlayerAccessoryWeapon = GetComponent<PlayerAccessoryWeapon>();
        LoadPlayerData();
    }   
    

    private void Update()
    {
        HandleShootingInput();
    }

    private void HandleShootingInput()
    {
        if (isReloading || !canShoot) return;

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
        cameraShakeController.ShakeCamera(weaponConfig.screenShakeIntensity, 0.1f);
        base.PlayShootEffects();
    }

    public void ChangeWeaponConfig(WeaponConfig newConfig)
    {
        weaponConfig = newConfig;

        PlayerAccessoryWeapon.DropAllAccessories();

        int slots = weaponConfig.accessorySlots;
        PlayerAccessoryWeapon.accessoryConfig = new List<AccessoryConfig>(slots);

        for (int i = 0; i < slots; i++)
        {
            PlayerAccessoryWeapon.accessoryConfig.Add(null);
        }

        InitializeWeapon();
        PlayerAccessoryWeapon.InitializeAccessory();
    }

    public void LoadPlayerData()
    {
        if (saveSystem == null)
        {
            Debug.LogError("SaveSystem is null!");
            return;
        }

        GameData data = saveSystem.GetCurrentGameData();

        if (data != null)
        {
            // Загрузка WeaponConfig по уникальному id
            if (!string.IsNullOrEmpty(data.weaponConfigId))
            {
                weaponConfig = FindWeaponConfigById(data.weaponConfigId);
                PlayerAccessoryWeapon.InitializeSlots();
                Debug.Log($"Загружено оружие: {weaponConfig.weaponName}");
            }

            // Загрузка AccessoryConfig по id
            if (PlayerAccessoryWeapon != null)
            {
                PlayerAccessoryWeapon.accessoryConfig = new List<AccessoryConfig>();
                if (data.accessoryConfigIds != null)
                {
                    foreach (var id in data.accessoryConfigIds)
                    {
                        PlayerAccessoryWeapon.accessoryConfig.Add(!string.IsNullOrEmpty(id) ? FindAccessoryConfigById(id) : null);
                    }
                }
                PlayerAccessoryWeapon.InitializeAccessory();
                Debug.Log($"Загружено {PlayerAccessoryWeapon.accessoryConfig.Count} аксессуаров");
            }
        }
        else
        {
            Debug.Log("Нет данных сохранения");
            if (PlayerAccessoryWeapon != null)
            {
                PlayerAccessoryWeapon.InitializeAccessory();
            }
        }
    }

    public void SaveGameData()
    {
        if (saveSystem == null)
        {
            Debug.LogError("SaveSystem is null!");
            return;
        }

        GameData gameData = saveSystem.GetCurrentGameData();
        if (gameData == null)
        {
            Debug.Log("Создаем новые данные игры");
            saveSystem.CreateNewGame();
            gameData = saveSystem.GetCurrentGameData();
        }

        if (gameData != null)
        {
            // Сохраняем id оружия
            if (weaponConfig != null)
                gameData.weaponConfigId = weaponConfig.weaponId;

            // Сохраняем id аксессуаров
            gameData.accessoryConfigIds = new List<string>();
            if (PlayerAccessoryWeapon != null && PlayerAccessoryWeapon.accessoryConfig != null)
            {
                foreach (var config in PlayerAccessoryWeapon.accessoryConfig)
                    gameData.accessoryConfigIds.Add(config != null ? config.accessoryId : null);
            }

            saveSystem.SaveGame();
            Debug.Log("Данные игрока сохранены");
        }
        else
        {
            Debug.LogError("Не удалось получить или создать данные игры");
        }
    }

    private WeaponConfig FindWeaponConfigById(string id)
    {
        var allConfigs = Resources.LoadAll<WeaponConfig>("Weapons");
        foreach (var config in allConfigs)
        {
            if (config.weaponId == id) // или сравнивайте с уникальным полем
                return config;
        }
        Debug.LogWarning($"WeaponConfig с id {id} не найден");
        return null;
    }

    private AccessoryConfig FindAccessoryConfigById(string id)
    {
        var allConfigs = Resources.LoadAll<AccessoryConfig>("Accessory");
        foreach (var config in allConfigs)
        {
            if (config.accessoryId == id)
                return config;
        }
        Debug.LogWarning($"AccessoryConfig с id {id} не найден");
        return null;
    }

    private void OnApplicationQuit()
    {
        //SaveGameData();
    }
}