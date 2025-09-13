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
    [Header("Настройка для аксессуаров")]
    public List<AccessoryConfig> accessoryConfig;
    protected SpriteRenderer _weaponSprite;
    protected Weapon _weapon;

    public virtual void Start()
    {
        _weapon = GetComponent<Weapon>();
        _weaponSprite = GetComponent<SpriteRenderer>();
    }

    
    public abstract void InitializeAccessory();
}