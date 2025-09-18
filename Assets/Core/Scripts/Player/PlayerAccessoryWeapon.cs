using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAccessoryWeapon : AccessoryWeapon
{
    public UnityEvent OnAccessoryChanged { get; } = new UnityEvent();
    private List<GameObject> interactAccessories = new List<GameObject>();
    public List<GameObject> listInteractItems = new List<GameObject>();
    
    public void InitializeSlots()
    {
        if (accessoryConfig.Count <= 0)
        {
            int slots = weapon.weaponConfig.accessorySlots;
            accessoryConfig = new List<AccessoryConfig>(slots);

            for (int i = 0; i < slots; i++)
            {
                accessoryConfig.Add(null);
            }
        }
    }

    public override void InitializeAccessory()
    {
        //Debug.Log("Инициализация аксессуара (игрок)");

        // Гарантируем что список инициализирован
        if (accessoryConfig.Count == 0 || accessoryConfig == null)
        {
            Debug.Log("Список аксессуаров пуст");
            if (weapon != null && weapon.weaponConfig != null)
            {
                weaponSprite.sprite = weapon.weaponConfig.weaponSpriteDefault;
                InitializeSlots();
                OnAccessoryChanged.Invoke();
            }
            return;
        }

        //Находит первый слот в массиве
        var activeConfig = accessoryConfig.Find(cfg => cfg != null);

        if (activeConfig == null)
        {
            if (weapon != null && weapon.weaponConfig != null)
            {
                weaponSprite.sprite = weapon.weaponConfig.weaponSpriteDefault;
                OnAccessoryChanged.Invoke();
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


        OnAccessoryChanged.Invoke();
    }

    /// <summary>
    /// Добавляет аксессуар в первый свободный слот.
    /// </summary>
    public void ChangeAccessoryConfig(AccessoryConfig newConfig, GameObject accessoryObject = null)
    {
        // Гарантируем инициализацию
        if (accessoryConfig == null)
        {
            InitializeSlots();
        }

        int emptySlotIndex = accessoryConfig.FindIndex(cfg => cfg == null);
        if (emptySlotIndex == -1)
        {
            Debug.Log("Нет свободных слотов для аксессуара!");
            return;
        }

        accessoryConfig[emptySlotIndex] = newConfig;
        InitializeAccessory();

        if (accessoryObject != null && accessoryObject.transform.parent != null)
        {
            while (interactAccessories.Count <= emptySlotIndex)
                interactAccessories.Add(null);

            interactAccessories[emptySlotIndex] = accessoryObject.transform.parent.gameObject;
            interactAccessories[emptySlotIndex].SetActive(false);
        }

        //SaveGameData();
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
        if (interactAccessories.Count > slotIndex && interactAccessories[slotIndex] != null)
        {
            interactAccessory = interactAccessories[slotIndex];
        }

        if (interactAccessory != null)
        {
            interactAccessory.SetActive(true);
            interactAccessory.transform.position = transform.position;
            CreateInteractAccessory(interactAccessory, config);
            interactAccessories[slotIndex] = null;
        }
        else
        {
            GameObject interactObjectAccessory = Instantiate(config.dropPrefab, transform.position, Quaternion.identity);
            listInteractItems.Add(interactObjectAccessory);
            CreateInteractAccessory(interactObjectAccessory, config);
        }


        accessoryConfig[slotIndex] = null;
        InitializeAccessory();
    }

    private void CreateInteractAccessory(GameObject interactAccessory, AccessoryConfig config)
    {
        Debug.Log(config.name);
        var interactComponent = interactAccessory.GetComponentInChildren<InteractAccessory>();
        SpriteRenderer spriteRenderer = interactAccessory.GetComponentInChildren<SpriteRenderer>();
        interactComponent.Initialize(config, config.accessorySprite, transform, spriteRenderer);
    }
}