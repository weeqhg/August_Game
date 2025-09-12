using UnityEngine;

public class EnemyAccessoryWeapon : AccessoryWeapon
{
    public override void InitializeAccessory()
    {
        Debug.Log("Инициализация аксессуара (враг)");
        if (accessoryConfig == null || accessoryConfig.Count == 0)
        {
            Debug.LogError("AccessoryConfig не назначен или список пуст!");
            return;
        }

        AccessoryConfig activeConfig = accessoryConfig.Find(cfg => cfg != null);

        if (activeConfig == null)
        {
            if (_weapon != null && _weapon.weaponConfig != null)
            {
                _weaponSprite.sprite = _weapon.weaponConfig.weaponSpriteDefault;
            }
            return;
        }

        switch (activeConfig.accessoryName)
        {
            case "Fire":
                _weaponSprite.sprite = _weapon.weaponConfig.weaponSpriteFire;
                break;
            case "Ice":
                _weaponSprite.sprite = _weapon.weaponConfig.weaponSpriteIce;
                break;
            default:
                _weaponSprite.sprite = _weapon.weaponConfig.weaponSpriteDefault;
                break;
        }
    }
    
}