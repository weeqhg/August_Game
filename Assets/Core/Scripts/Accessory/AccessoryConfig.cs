using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum AccessoryType { WeaponMod, PlayerBuff }


[CreateAssetMenu(fileName = "NewAccessoryConfig", menuName = "Accessories/Accessory Configuration")]
public class AccessoryConfig : ScriptableObject
{
    [Header("Основные настройки")]
    public string accessoryName = "Accessory";
    public Sprite accessorySprite;
    public AccessoryType accessoryType = AccessoryType.WeaponMod;
    public GameObject dropPrefab;

    [Header("Модификаторы оружия (если WeaponMod)")]
    public DamageType damageType = DamageType.Normal;

    [Header("Модификаторы игрока (если PlayerBuff)")]
    public float healthBonus = 0f;

}
