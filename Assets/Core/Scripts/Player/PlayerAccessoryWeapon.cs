using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAccessoryWeapon : AccessoryWeapon
{
    public UnityEvent OnAccessoryChanged = new UnityEvent();
    List<GameObject> _interactAccessories = new List<GameObject>();
    public override void InitializeAccessory()
    {
        Debug.Log("Инициализация аксессуара (игрок)");
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
                OnAccessoryChanged.Invoke();
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

        OnAccessoryChanged.Invoke();
    }

    public void ChangeAccessoryConfig(AccessoryConfig newConfig, GameObject accessoryObject = null)
    {
        int emptySlot = accessoryConfig.FindIndex(cfg => cfg == null);
        if (emptySlot != -1)
        {
            accessoryConfig[emptySlot] = newConfig;
            InitializeAccessory();

            if (accessoryObject != null && accessoryObject.transform.parent != null)
            {
                while (_interactAccessories.Count <= emptySlot)
                    _interactAccessories.Add(null);

                _interactAccessories[emptySlot] = accessoryObject.transform.parent.gameObject;
                _interactAccessories[emptySlot].SetActive(false);
            }
        }
        else
        {
            Debug.Log("Нет свободных слотов для аксессуара!");
        }
    }
    public void DropAllAccessories()
    {
        if (accessoryConfig == null) return;

        // Копируем индексы, чтобы избежать проблем при изменении accessoryConfig
        for (int i = 0; i < accessoryConfig.Count; i++)
        {
            if (accessoryConfig[i] != null)
            {
                DropAccessory(i);
            }
        }
    }
    public void DropAccessory(int slotIndex)
    {
        if (accessoryConfig == null || slotIndex < 0 || slotIndex >= accessoryConfig.Count)
        {
            Debug.LogWarning("Некорректный индекс слота или accessoryConfig не назначен.");
            return;
        }

        var config = accessoryConfig[slotIndex];
        if (config == null)
        {
            Debug.LogWarning("В этом слоте нет аксессуара для выбрасывания.");
            return;
        }

        GameObject interactAccessory = null;
        if (_interactAccessories.Count > slotIndex)
        {
            interactAccessory = _interactAccessories[slotIndex];
        }

        if (interactAccessory != null)
        {
            interactAccessory.SetActive(true);
            interactAccessory.transform.position = transform.position;
            var interactComponent = interactAccessory.GetComponentInChildren<InteractAccessory>();
            SpriteRenderer spriteRenderer = interactAccessory.GetComponentInChildren<SpriteRenderer>();
            interactComponent.Initialize(config, config.accessorySprite, transform, spriteRenderer);
            _interactAccessories[slotIndex] = null;
        }
        else
        {
            GameObject interactObjectAccessory = Instantiate(config.dropPrefab, transform.position, Quaternion.identity);
            var interactComponent = interactObjectAccessory.GetComponentInChildren<InteractAccessory>();
            SpriteRenderer spriteRenderer = interactObjectAccessory.GetComponentInChildren<SpriteRenderer>();
            interactComponent.Initialize(config, config.accessorySprite, transform, spriteRenderer);
        }
            

        accessoryConfig[slotIndex] = null;
        InitializeAccessory();
    }
}