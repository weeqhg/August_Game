using UnityEngine;

public enum AttackType
{
    Melee,
    Ranged
}
public enum TargetType
{ 
    Enemy, 
    Player 
}
[CreateAssetMenu(fileName = "NewWeaponConfig", menuName = "Weapons/Weapon Configuration")]
public class WeaponConfig : ScriptableObject
{
    [Header("Основные настройки")]
    public string weaponId; // Уникальный ID для сохранения
    public string weaponName = "Weapon";
    public string nameAttack = "Enemy";
    public TargetType TargetType = TargetType.Enemy;

    public Sprite weaponSpriteDefault;
    public Sprite weaponSpriteFire;
    public Sprite weaponSpriteIce;


    public GameObject projectilePrefab;
    public Color colorProjectile;
    public AttackType attackType = AttackType.Melee;
    public int accessorySlots;

    [Header("Параметры стрельбы")]
    [Range(0f, 1f)] public float criticalChance = 0.1f;
    [Min(1f)] public float criticalMultiplier = 2f;     // Множитель урона (2x)
    public bool canCritical = true;
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