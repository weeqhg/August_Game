using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySetting : MonoBehaviour
{
    [Header("Настройки оружия и аксессуаров")]
    public WeaponConfig currentWeapon;
    public List<AccessoryConfig> accessoryConfig;
    //public AttackType CurrentAttackType;
}
