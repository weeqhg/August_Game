using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AccessoryWeapon основной класс отвечающий за аксессуары оружия.
/// Делится на PlayerAccessoryWeapon и EnemyAccessoryWeapon.
/// Отвечает за обработку аксессуаров, их инициализацию и добавление в слоты, Ui
/// <summary>

public abstract class AccessoryWeapon : MonoBehaviour
{
    //[Header("Настройка для аксессуаров")]
    [HideInInspector] public List<AccessoryConfig> accessoryConfig = new List<AccessoryConfig>();
    protected SpriteRenderer weaponSprite;
    protected Weapon weapon;

    public virtual void Start()
    {
        weapon = GetComponent<Weapon>();
        weaponSprite = GetComponent<SpriteRenderer>();
    }

    public abstract void InitializeAccessory();
}