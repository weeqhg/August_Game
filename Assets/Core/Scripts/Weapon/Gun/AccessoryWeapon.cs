using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class AccessoryWeapon : MonoBehaviour
{
    [Header("Настройка для аксессуаров")]
    public List<AccessoryConfig> accessoryConfig;
    protected SpriteRenderer _weaponSprite;
    protected Weapon _weapon;

    protected virtual void Start()
    {
        _weapon = GetComponent<Weapon>();
        _weaponSprite = GetComponent<SpriteRenderer>();
        InitializeSlots();
    }

    public void InitializeSlots()
    {
        if (accessoryConfig.Count <= 0)
        {
            int slots = _weapon.weaponConfig.accessorySlots;
            accessoryConfig = new List<AccessoryConfig>(slots);

            for (int i = 0; i < slots; i++)
            {
                accessoryConfig.Add(null);
            }
        }
    }
    public abstract void InitializeAccessory();
}