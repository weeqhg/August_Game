using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;



public class InteractChest : PlayerInteract
{
    [SerializeField] private WeaponConfig[] weapons;
    [SerializeField] private GameObject _prefabWeaponPick;
    [SerializeField] private int id = 0;
    
    private bool _isOpen;
    public override void Interact()
    {
        if (!_isOpen) return;

        GameObject gameObject = Instantiate(_prefabWeaponPick, transform.position, Quaternion.identity);
        InteractWeapon interactWeapon = gameObject.GetComponent<InteractWeapon>();
        interactWeapon.Initialize(weapons[id], weapons[id].weaponSprite, transform);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isOpen = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isOpen = false;
        }
    }


}
