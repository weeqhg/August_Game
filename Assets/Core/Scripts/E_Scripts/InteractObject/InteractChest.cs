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

    private Animator _animator;


    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalMaterial = _spriteRenderer.material;
        _prompt = GameManager.Instance.Get<InterfacePrompt>();
        _animator = GetComponent<Animator>();
    }
    public override void Interact()
    {
        if (!_isOpen) return;
        _animator.SetTrigger("Open");

        // Запускаем таймер с задержкой перед появление оружия
        StartCoroutine(SpawnWeaponWithDelay());

        Disable();
    }

    private IEnumerator SpawnWeaponWithDelay()
    {
        // Задержка перед появление оружия (например, 0.5 секунды)
        yield return new WaitForSeconds(0.2f);

        // Проверяем, не уничтожен ли уже объект
        if (this == null) yield break;

        GameObject gameObject = Instantiate(_prefabWeaponPick, transform.position, Quaternion.identity);
        InteractWeapon interactWeapon = gameObject.GetComponentInChildren<InteractWeapon>();

        if (interactWeapon != null && weapons.Length > id)
        {
            interactWeapon.Initialize(weapons[id], weapons[id].weaponSprite, transform);
        }

        // Уничтожаем этот компонент после появление оружия
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
