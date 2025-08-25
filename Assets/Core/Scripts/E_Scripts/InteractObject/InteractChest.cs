using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;



public class InteractChest : PlayerInteract
{
    [SerializeField] private WeaponConfig[] weapons;
    [SerializeField] private GameObject _prefabWeaponPick;

    [SerializeField] private Material _outLineMaterial;
    [SerializeField] private int id = 0;

    private bool _isOpen;
    private Material _originalMaterial;
    private SpriteRenderer _spriteRenderer;
    private InterfacePrompt _prompt;


    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalMaterial = _spriteRenderer.material;
        _prompt = GameManager.Instance.Get<InterfacePrompt>();
    }
    public override void Interact()
    {
        if (!_isOpen) return;

        GameObject gameObject = Instantiate(_prefabWeaponPick, transform.position, Quaternion.identity);
        InteractWeapon interactWeapon = gameObject.GetComponentInChildren<InteractWeapon>();
        interactWeapon.Initialize(weapons[id], weapons[id].weaponSprite, transform);

        Disable();

        Destroy(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isOpen = true;
            _spriteRenderer.material = _outLineMaterial;
            _prompt.ButtonPressE(transform, _isOpen);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Disable();
        }
    }

    private void Disable()
    {
        _isOpen = false;
        _spriteRenderer.material = _originalMaterial;
        _prompt.ButtonPressE(transform, _isOpen);
    }


}
