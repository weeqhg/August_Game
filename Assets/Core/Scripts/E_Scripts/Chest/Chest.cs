using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoreWeapon
{
    public WeaponConfig weaponConfig;
    public Sprite sprite;
}

public class Chest : MonoBehaviour
{
    [SerializeField] private StoreWeapon[] weapons;
}
