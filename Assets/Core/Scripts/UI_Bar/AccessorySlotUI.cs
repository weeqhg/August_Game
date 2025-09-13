// Отдельный класс для обработки событий каждого слота
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Этот класс отвечает за обработку событий наведения и снятия наведения с аксессуарного слота.
/// Нужен чтобы выделять слот при наведении и убирать выделение при снятии наведения.
/// В AccessoryBarUI.cs происходит проверка свойства IsHovered для возможности взаимодействия с аксессуаром.
/// <summary>

public class AccessorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _icon;
    public bool IsHovered { get; private set; }
    [SerializeField] private Material _outLineMaterial;

    private void Start()
    {
        _icon = GetComponent<Image>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHovered = true;
        _icon.material = _outLineMaterial;
        Debug.Log($"Наведен на слот: {name}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHovered = false;
        _icon.material = null;
    }
}