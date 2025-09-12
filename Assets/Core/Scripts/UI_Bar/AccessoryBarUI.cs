using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccessoryBarUI : MonoBehaviour
{
    [Header("Prefab UI-элемента аксессуара")]
    [SerializeField] private GameObject _accessorySlotPrefab;

    [Header("Родительский объект для слотов")]
    [SerializeField] private Transform _slotsParent;

    private PlayerAccessoryWeapon _accessoryWeapon;
    private List<GameObject> slotObjects = new List<GameObject>();

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
    /// Обновляет визуальное отображение аксессуаров.
    /// </summary>
    public void RefreshUI()
    {
        // Удаляем старые слоты
        foreach (var obj in slotObjects)
        {
            Destroy(obj);
        }
        slotObjects.Clear();

        // Создаём новые слоты по количеству в accessoryConfig
        for (int i = 0; i < _accessoryWeapon.accessoryConfig.Count; i++)
        {
            var config = _accessoryWeapon.accessoryConfig[i];
            GameObject slot = Instantiate(_accessorySlotPrefab, _slotsParent);
            slotObjects.Add(slot);

            // Настраиваем отображение
            Image icon = slot.GetComponentInChildren<Image>();
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