using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// AccessoryBarUI отвечает за отображение и управление аксессуарами игрока в UI.
/// Новые слоты создаются динамически на основе конфигурации оружия, находящегося на объекте.
/// В WeaponConfig.cs можно настроить количество слотов.
/// <summary>
public class AccessoryBarUI : MonoBehaviour
{
    [Header("Prefab UI-элемента аксессуара")]
    [SerializeField] private GameObject _accessorySlotPrefab;
    [SerializeField] private Canvas _accessoryCanvas;

    [Header("Родительский объект для слотов")]
    [SerializeField] private Transform _slotsParent;

    private PlayerAccessoryWeapon _accessoryWeapon;
    private List<AccessorySlotUI> _slotUI = new List<AccessorySlotUI>();


    [SerializeField] private KeyCode dropKey = KeyCode.Q;


    public void Initialize(PlayerAccessoryWeapon newAccessoryWeapon)
    {
        _accessoryWeapon = newAccessoryWeapon;

        if (_accessoryWeapon == null)
        {
            Debug.LogError("AccessoryWeapon не назначен в AccessoryBarUI!");
            return;
        }

        _accessoryWeapon.OnAccessoryChanged.AddListener(RefreshUI);
    }

    /// <summary>
    /// Реализация выброса предмета из слотов
    /// </summary>

    void Update()
    {
        // Проверяем наведение и нажатие Q
        if (Input.GetKeyDown(dropKey))
        {
            foreach (var slot in _slotUI)
            {
                if (slot.IsHovered)
                {
                    if (int.TryParse(slot.name, out int slotIndex))
                    {
                        _accessoryWeapon.DropAccessory(slotIndex);
                        break;
                    }
                }
            }
        }
    }
    

    /// <summary>
    /// Обновляет визуальное отображение аксессуаров.
    /// </summary>
    public void RefreshUI()
    {
        // Удаляем старые слоты
        foreach (var obj in _slotUI)
        {
            if (obj != null) Destroy(obj.gameObject);
        }
        _slotUI.Clear();

        // Создаём новые слоты по количеству в accessoryConfig
        for (int i = 0; i < _accessoryWeapon.accessoryConfig.Count; i++)
        {
            var config = _accessoryWeapon.accessoryConfig[i];
            GameObject slotObj = Instantiate(_accessorySlotPrefab, _slotsParent);
            slotObj.name = i.ToString();

            // Добавляем компонент для обработки событий слота
            AccessorySlotUI slotUI = slotObj.GetComponent<AccessorySlotUI>();
            if (slotUI == null)
            {
                slotUI = slotObj.AddComponent<AccessorySlotUI>();
            }

            _slotUI.Add(slotUI);


            // Настраиваем отображение
            Image icon = slotObj.GetComponentInChildren<Image>();
            if (icon != null)
            {
                if (config != null && config.accessorySprite != null)
                {
                    icon.sprite = config.accessorySprite;
                    icon.color = Color.white;
                }
                else
                {
                    icon.sprite = null;
                    icon.color = new Color(1, 1, 1, 0.2f); // прозрачный слот
                }
            }
        }
    }
}