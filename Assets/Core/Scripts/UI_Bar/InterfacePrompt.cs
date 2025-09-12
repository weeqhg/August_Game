using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InterfacePrompt : MonoBehaviour
{
    [Header("Настройка интерфейса взаимодействия")]
    [SerializeField] private Canvas _worldCanvas;
    [SerializeField] private Image _buttonE;
    [SerializeField] private TextMeshProUGUI _nameWeapon;
    [SerializeField] private Vector3 _worldOffset = new Vector3(0, 1f, 0);

    private void Awake()
    {
        GameManager.Instance.Register(this);
    }

    private void Start()
    {
        _buttonE.enabled = false;
        _nameWeapon.enabled = false;
    }

    public void AnimationName(string nameWeapon, Transform targetTransform)
    {
        _nameWeapon.enabled = true;
        _nameWeapon.text = nameWeapon;
        
    }

    public void ButtonPressE(Transform targetTransform, bool enable)
    {
        _buttonE.enabled = enable;
        if (enable && targetTransform != null)
        {
            // Позиционируем UI в мировом пространстве
            _buttonE.transform.position = targetTransform.position + _worldOffset;

            // Поворачиваем к камере
            _buttonE.transform.LookAt(Camera.main.transform);
            _buttonE.transform.Rotate(0, 180, 0); // Разворачиваем лицевой стороной
        }
    }

    
}