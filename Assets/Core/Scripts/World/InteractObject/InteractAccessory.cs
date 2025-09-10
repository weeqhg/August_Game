using DG.Tweening;
using UnityEngine;

public class InteractAccessory : PlayerInteract
{
    [Header("Accessory Settings")]
    [SerializeField] private Transform accessoryShadow;
    [SerializeField] private float hoverHeight = 0.2f;
    [SerializeField] private float hoverDuration = 1f;
    [SerializeField] private float pickupScaleDuration = 0.3f;
    [SerializeField] private float respawnScaleDuration = 0.5f;
    [SerializeField] private float kickForce = 0.1f;

    private AccessoryConfig accessoryConfig;
    private AccessoryWeapon accessoryWeapon;
    private Tween hoverTween;
    private Vector3 originalPosition;
    private bool isRespawning = false;
    private Sprite accessorySprite;

    public void Initialize(AccessoryConfig config, Sprite sprite, Transform spawnTransform, SpriteRenderer spriteR)
    {
        accessoryConfig = config;
        accessorySprite = sprite;
        originalPosition = spawnTransform.position;
        spriteRenderer = spriteR;


        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }

        StartHoverAnimation();
    }

    public override void Interact()
    {
        if (isRespawning || accessoryWeapon == null) return;

        // Сохраняем предыдущую конфигурацию для респавна
        AccessoryConfig previousConfig = accessoryWeapon.accessoryConfig;
        //поменять здесь
        Sprite previousSprite = accessoryWeapon.accessoryConfig.accessorySprite;
        // Меняем оружие у игрока
        accessoryWeapon.ChangeAccessoryConfig(accessoryConfig);

        // Устанавливаем старую конфигурацию для респавна
        spriteRenderer.sprite = previousSprite;
        accessoryConfig = previousConfig;
        accessorySprite = previousSprite;
    }

    private void StartHoverAnimation()
    {
        if (isRespawning) return;

        StopHoverAnimation();
        UpdateShadowPosition();

        hoverTween = transform.DOMoveY(transform.position.y + hoverHeight, hoverDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopHoverAnimation()
    {
        hoverTween?.Kill();
    }

    private void UpdateShadowPosition()
    {
        if (accessoryShadow != null)
        {
            accessoryShadow.position = new Vector3(transform.position.x, transform.position.y, 0);
        }
    }

    private void PlayPickupAnimation()
    {
        StopHoverAnimation();
        isRespawning = true;

        Sequence pickupSequence = DOTween.Sequence();
        pickupSequence.Append(transform.DOScale(Vector3.zero, pickupScaleDuration));

        if (accessoryShadow != null)
        {
            pickupSequence.Join(accessoryShadow.DOScale(Vector3.zero, pickupScaleDuration));
        }

        pickupSequence.OnComplete(() =>
        {
            PlayRespawnAnimation();
        });
    }

    private void PlayRespawnAnimation()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 kickPosition = originalPosition + (Vector3)randomDirection * kickForce;

        transform.position = kickPosition;
        transform.localScale = Vector3.zero;
        UpdateShadowPosition();

        Sequence respawnSequence = DOTween.Sequence();
        respawnSequence.Append(transform.DOScale(Vector3.one, respawnScaleDuration));

        if (accessoryShadow != null)
        {
            respawnSequence.Join(accessoryShadow.DOScale(Vector3.one, respawnScaleDuration));
        }

        respawnSequence.OnComplete(() =>
        {
            isRespawning = false;
            originalPosition = transform.position;
            StartHoverAnimation();
        });
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if (collision.CompareTag("Player"))
        {
            accessoryWeapon = collision.GetComponentInChildren<AccessoryWeapon>();
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

        if (collision.CompareTag("Player"))
        {
            accessoryWeapon = null;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        StopHoverAnimation();
    }
}