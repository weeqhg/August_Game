using System.Collections;
using UnityEngine;


/// <summary>
/// InteractChest для взаимодействия с сундуком
/// 
/// 
/// 
/// </summary>
public enum ItemType { Weapon, Accessory }
public class InteractChest : PlayerInteract
{
    [Header("Chest Settings")]
    [SerializeField] private ItemType itemType;

    [SerializeField] private WeaponConfig[] weapons;
    [SerializeField] private GameObject weaponPickPrefab;

    [SerializeField] private AccessoryConfig[] accessories;
    [SerializeField] private GameObject accessoryPickPrefab;


    private Animator animator;
    private bool isOpened = false;
    private int selectedId;

    protected override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
        if (itemType == ItemType.Weapon)
            selectedId = Random.Range(0, weapons.Length);
        else if (itemType == ItemType.Accessory)
            selectedId = Random.Range(0, accessories.Length);
    }

    public override void Interact()
    {
        if (isOpened) return;
        isOpened = true;
        animator.SetTrigger("Open");


        StartCoroutine(SpawnItemWithDelay());
    }


    private IEnumerator SpawnItemWithDelay()
    {
        yield return new WaitForSeconds(0.2f);

        if (this == null) yield break;

        GameObject itemPrefab = itemType == ItemType.Weapon ? weaponPickPrefab : accessoryPickPrefab;


        GameObject itemObject = Instantiate(itemPrefab, transform.position, Quaternion.identity, transform);


        var interactWeapon = itemObject.GetComponentInChildren<InteractWeapon>();
        var interactAccessory = itemObject.GetComponentInChildren<InteractAccessory>();

        SpriteRenderer spriteRenderer = itemObject.GetComponentInChildren<SpriteRenderer>();

        if (interactWeapon != null)
        {
            interactWeapon.Initialize(weapons[selectedId],
                weapons[selectedId].weaponSpriteDefault, transform, spriteRenderer);
        }

        if (interactAccessory != null)
        {
            interactAccessory.Initialize(accessories[selectedId],
                accessories[selectedId].accessorySprite, transform, spriteRenderer);
        }

        //Сначала выключаем
        DisableInteraction();
        useInteract = true;

    }
}