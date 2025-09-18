using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// InteractChest для взаимодействия с сундуком
/// </summary>
public enum ItemType { Weapon, Accessory }
public class InteractChest : PlayerInteract
{
    [Header("Chest Settings")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private SpriteRenderer keySprite;

    [SerializeField] private WeaponConfig[] weapons;
    [SerializeField] private GameObject weaponPickPrefab;

    [SerializeField] private AccessoryConfig[] accessories;
    [SerializeField] private GameObject accessoryPickPrefab;

    [Header("Animation Settings")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 0.05f;
    [SerializeField] private int shakeVibrato = 10;
    [SerializeField] private float noKeyFlashDuration = 0.3f;
    [SerializeField] private Color noKeyFlashColor = Color.red;

    private int coastKey = 1;
    private PlayerKey playerKey;
    private Animator animator;
    private bool isOpened = false;
    private int selectedId;
    private SpriteRenderer chestSpriteRenderer;
    private Color originalChestColor;

    protected override void Start()
    {
        base.Start();
        keySprite.enabled = false;
        animator = GetComponent<Animator>();
        chestSpriteRenderer = GetComponent<SpriteRenderer>();
        originalChestColor = chestSpriteRenderer.color;

        if (itemType == ItemType.Weapon)
            selectedId = Random.Range(0, weapons.Length);
        else if (itemType == ItemType.Accessory)
            selectedId = Random.Range(0, accessories.Length);
    }

    public override void Interact()
    {
        if (isOpened) return;

        if (playerKey.CurrentCountKey() <= 0)
        {
            // Анимация недостатка ключей
            PlayNoKeyAnimation();
            return;
        }

        isOpened = true;

        animator.SetTrigger("Open");
        playerKey.MinusKey(coastKey);

        StartCoroutine(SpawnItemWithDelay());
    }

    private void PlayNoKeyAnimation()
    {
        Debug.Log("Анимация");
        // Анимация встряски сундука
        transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato)
            .SetEase(Ease.OutQuad);

        // Мигание красным цветом
        chestSpriteRenderer.DOColor(noKeyFlashColor, noKeyFlashDuration / 2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                chestSpriteRenderer.DOColor(originalChestColor, noKeyFlashDuration / 2f)
                    .SetEase(Ease.InQuad);
            });

        // Анимация ключа (если он виден)
        if (keySprite.enabled)
        {
            keySprite.transform.DOShakeRotation(shakeDuration, new Vector3(0, 0, 30f), shakeVibrato)
                .SetEase(Ease.OutQuad);

            keySprite.DOColor(Color.red, noKeyFlashDuration / 2f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    keySprite.DOColor(Color.white, noKeyFlashDuration / 2f)
                        .SetEase(Ease.InQuad);
                });
        }

        // Можно добавить звуковой эффект
        // AudioManager.Instance.PlaySFX("chest_locked");
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerKey = collision.GetComponent<PlayerKey>();
            canInteract = true;

            if (playerKey.CurrentCountKey() > 0)
            {
                base.OnTriggerEnter2D(collision);
            }
            else if (!isOpened)
            {
                keySprite.enabled = true;
                // Плавное появление ключа
                keySprite.color = new Color(1, 1, 1, 0);
                keySprite.DOFade(1f, 0.3f);
            }
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

        if (collision.CompareTag("Player"))
        {
            canInteract = false;
            // Плавное исчезновение ключа
            if (keySprite.enabled)
            {
                keySprite.DOFade(0f, 0.3f)
                    .OnComplete(() => keySprite.enabled = false);
            }
        }
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

        // Сначала выключаем
        DisableInteraction();
        useInteract = true;
    }

    // Метод для принудительной остановки анимаций
    protected override void OnDestroy()
    {
        transform.DOKill();
        chestSpriteRenderer.DOKill();
        keySprite.DOKill();
    }
}