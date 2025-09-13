using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAccessoryWeapon : AccessoryWeapon
{
    public UnityEvent OnAccessoryChanged { get; } = new UnityEvent();
    private List<GameObject> _interactAccessories = new List<GameObject>();
    private SaveSystem _saveSystem;
    public override void Start()
    {
        base.Start();   
        _saveSystem = GameManager.Instance.Get<SaveSystem>();
        LoadPlayerData();
        InitializeAccessory();
    }

    public void InitializeSlots()
    {
        if (accessoryConfig.Count <= 0)
        {
            int slots = _weapon.weaponConfig.accessorySlots;
            accessoryConfig = new List<AccessoryConfig>(slots);

            for (int i = 0; i < slots; i++)
            {
                accessoryConfig.Add(null);
            }
        }
    }

    public override void InitializeAccessory()
    {
        Debug.Log("Инициализация аксессуара (игрок)");

        // Гарантируем что список инициализирован
        if (accessoryConfig == null)
        {
            InitializeSlots();
        }

        if (accessoryConfig.Count == 0)
        {
            Debug.Log("Список аксессуаров пуст");
            if (_weapon != null && _weapon.weaponConfig != null)
            {
                _weaponSprite.sprite = _weapon.weaponConfig.weaponSpriteDefault;
                OnAccessoryChanged.Invoke();
            }
            return;
        }

        //Находит первый слот в массиве
        var activeConfig = accessoryConfig.Find(cfg => cfg != null);

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
            while (_interactAccessories.Count <= emptySlotIndex)
                _interactAccessories.Add(null);

            _interactAccessories[emptySlotIndex] = accessoryObject.transform.parent.gameObject;
            _interactAccessories[emptySlotIndex].SetActive(false);
        }

        SaveGameData();
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
        if (_interactAccessories.Count > slotIndex && _interactAccessories[slotIndex] != null)
        {
            interactAccessory = _interactAccessories[slotIndex];
        }

        if (interactAccessory != null)
        {
            interactAccessory.SetActive(true);
            interactAccessory.transform.position = transform.position;
            CreateInteractAccessory(interactAccessory, config);
            _interactAccessories[slotIndex] = null;
        }
        else
        {
            GameObject interactObjectAccessory = Instantiate(config.dropPrefab, transform.position, Quaternion.identity);
            CreateInteractAccessory(interactAccessory, config);
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

    public void LoadPlayerData()
    {
        if (_saveSystem == null)
        {
            Debug.LogError("SaveSystem is null!");
            return;
        }

        GameData data = _saveSystem.GetCurrentGameData();

        if (data != null)
        {
            // Безопасное копирование данных аксессуаров
            if (data.accessoryConfig != null)
            {
                // Создаем новый список чтобы избежать ссылочных проблем
                accessoryConfig = new List<AccessoryConfig>(data.accessoryConfig);
                Debug.Log($"Загружено {accessoryConfig.Count} аксессуаров");
            }
            else
            {
                // Если в сохранении нет аксессуаров, инициализируем пустые слоты
                Debug.Log("В сохранении нет данных аксессуаров");
                InitializeSlots();
            }
        }
        else
        {
            Debug.Log("Нет данных сохранения");
            InitializeSlots();
        }
    }



    public void SaveGameData()
    {
        if (_saveSystem == null)
        {
            Debug.LogError("SaveSystem is null!");
            return;
        }

        // Получаем или создаем данные игры
        GameData gameData = _saveSystem.GetCurrentGameData();
        if (gameData == null)
        {
            Debug.Log("Создаем новые данные игры");
            _saveSystem.CreateNewGame();
            gameData = _saveSystem.GetCurrentGameData();
        }

        if (gameData != null)
        {
            // Безопасное обновление данных аксессуаров
            if (accessoryConfig != null)
            {
                // Создаем копию чтобы избежать ссылочных проблем
                gameData.accessoryConfig = new List<AccessoryConfig>(accessoryConfig);
            }
            else
            {
                gameData.accessoryConfig = new List<AccessoryConfig>();
            }

            _saveSystem.SaveGame();
            Debug.Log("Данные аксессуаров сохранены");
        }
        else
        {
            Debug.LogError("Не удалось получить или создать данные игры");
        }
    }

    

    private void OnApplicationQuit()
    {
        SaveGameData();
    }
}