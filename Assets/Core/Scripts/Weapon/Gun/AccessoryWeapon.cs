using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccessoryWeapon : MonoBehaviour
{
    [Header("Настройка для аксессуаров")]
    public AccessoryConfig accessoryConfig;

    private SpriteRenderer _weaponSpriteRenderer;
    private Weapon _weapon;

    private void Start()
    {
        _weaponSpriteRenderer = GetComponent<SpriteRenderer>();
        _weapon = GetComponent<Weapon>();
    }
    private void InitializeAccessory()
    {
        if (accessoryConfig == null)
        {
            Debug.LogError("AccessoryConfig не назначен!");
            return;
        }

        switch(accessoryConfig.name)
        {
            case "Default":
                _weapon.weaponSpriteRenderer.sprite = _weapon.weaponConfig.weaponSpriteDefault;
                break;
            case "Fire":
                _weapon.weaponSpriteRenderer.sprite = _weapon.weaponConfig.weaponSpriteFire;
                break;
            case "Ice":
                _weapon.weaponSpriteRenderer.sprite = _weapon.weaponConfig.weaponSpriteIce;
                break;

        }

    }


    public void ChangeAccessoryConfig(AccessoryConfig newConfig)
    {
        accessoryConfig = newConfig;
        InitializeAccessory();
    }
}
