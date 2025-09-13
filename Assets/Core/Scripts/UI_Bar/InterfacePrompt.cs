using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InterfacePrompt : MonoBehaviour
{
    [Header("Настройка интерфейса взаимодействия")]
    [SerializeField] private Canvas _worldCanvas;
    [SerializeField] private TextMeshProUGUI _nameItem;
    [SerializeField] private Vector3 _worldOffset = new Vector3(0, 1f, 0);

    private void Awake()
    {
        GameManager.Instance.Register(this);
    }

    private void Start()
    {
        _nameItem.enabled = false;
    }

    public void AnimationName(string nameWeapon, Transform targetTransform)
    {
        _nameItem.enabled = true;
        _nameItem.text = nameWeapon;

        _nameItem.transform.position = targetTransform.position + _worldOffset;
    }

    

    
}