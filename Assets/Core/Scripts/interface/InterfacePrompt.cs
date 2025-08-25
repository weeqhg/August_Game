using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InterfacePrompt: MonoBehaviour
{
    [Header("Настройка интерфейса взаимодействия")]
    [SerializeField] private Image _buttonE;
    [SerializeField] private TextMeshProUGUI _nameWeapon;


    private void Awake()
    {
        GameManager.Instance.Register(this);
    }
    private void Start()
    {
        _buttonE.enabled = false;
        _nameWeapon.enabled = false;
    }

    public void AnimationName(string nameWeapon, Transform pos)
    {
        _nameWeapon.rectTransform.position = pos.position;
        _nameWeapon.text = nameWeapon;
    }

    public void ButtonPressE(Transform pos, bool enable)
    {
        _buttonE.enabled = enable;
        _buttonE.rectTransform.position = pos.position;
    }
}
