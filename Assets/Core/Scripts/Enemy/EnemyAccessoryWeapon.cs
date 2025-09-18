using UnityEngine;

public class EnemyAccessoryWeapon : AccessoryWeapon
{
    public override void Start()
    {
        base.Start();
        EnemySetting enemySetting = GetComponentInParent<EnemySetting>();
        accessoryConfig = enemySetting.accessoryConfig;
    }
    public override void InitializeAccessory()
    {
        //Debug.Log("Инициализация аксессуара (враг)");
        if (accessoryConfig == null || accessoryConfig.Count == 0)
        {
            //Debug.Log("AccessoryConfig не назначен или список пуст!");
            weaponSprite.sprite = weapon.weaponConfig.weaponSpriteDefault;
            return;
        }

        AccessoryConfig activeConfig = accessoryConfig.Find(cfg => cfg != null);

        if (activeConfig == null)
        {
            if (weapon != null && weapon.weaponConfig != null)
            {
                weaponSprite.sprite = weapon.weaponConfig.weaponSpriteDefault;
            }
            return;
        }

        switch (activeConfig.accessoryName)
        {
            case "Fire":
                weaponSprite.sprite = weapon.weaponConfig.weaponSpriteFire;
                break;
            case "Ice":
                weaponSprite.sprite = weapon.weaponConfig.weaponSpriteIce;
                break;
            default:
                weaponSprite.sprite = weapon.weaponConfig.weaponSpriteDefault;
                break;
        }
    }
    
}