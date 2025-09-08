using UnityEngine;

public enum AttackType
{
    Melee,
    Ranged
}
[CreateAssetMenu(fileName = "NewWeaponConfig", menuName = "Weapons/Weapon Configuration")]
public class WeaponConfig : ScriptableObject
{
    [Header("Основные настройки")]
    public string weaponName = "Weapon";
    public Sprite weaponSprite;
    public Sprite weaponSpriteFire;
    public Sprite weaponSpriteIce;
    public GameObject projectilePrefab;
    public AttackType attackType = AttackType.Melee;

    [Header("Параметры стрельбы")]
    public float fireRate = 0.5f;
    public int projectilesPerShot = 1;
    public float spreadAngle = 0f;
    public float projectileSpeed = 10f;
    public float damage = 10f;
    public float destroyTime = 5f;

    [Header("Визуальные эффекты")]
    public GameObject muzzleFlashEffect;
    public AudioClip shootSound;
    public float screenShakeIntensity = 0.1f;

    [Header("Анимации")]
    public string shootAnimationName = "Shoot";

    [Header("Боезапас")]
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;

    [Header("Разное")]
    public bool isAutomatic = false;

}