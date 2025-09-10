using DG.Tweening;
using UnityEngine;

public class InteractWeapon : PlayerInteract
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponShadow;
    [SerializeField] private float hoverHeight = 0.2f;
    [SerializeField] private float hoverDuration = 1f;
    [SerializeField] private float pickupScaleDuration = 0.3f;
    [SerializeField] private float respawnScaleDuration = 0.5f;
    [SerializeField] private float kickForce = 0.1f;

    private WeaponConfig weaponConfig;
    private Weapon playerWeapon;
    private Sprite weaponSprite;
    private Tween hoverTween;
    private Vector3 originalPosition;
    private bool isRespawning = false;

    public void Initialize(WeaponConfig config, Sprite sprite, Transform spawnTransform, SpriteRenderer spriteR)
    {
        weaponConfig = config;
        weaponSprite = sprite;
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
        if (isRespawning || playerWeapon == null) return;

        // Сохраняем предыдущую конфигурацию для респавна
        WeaponConfig previousConfig = playerWeapon.weaponConfig;
        //поменять здесь
        Sprite previousSprite = playerWeapon.weaponConfig.weaponSpriteDefault;
        // Меняем оружие у игрока
        playerWeapon.ChangeWeaponConfig(weaponConfig);

        // Устанавливаем старую конфигурацию для респавна
        spriteRenderer.sprite = previousSprite;
        weaponConfig = previousConfig;
        weaponSprite = previousSprite;

        PlayPickupAnimation();
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
        if (weaponShadow != null)
        {
            weaponShadow.position = new Vector3(transform.position.x, transform.position.y, 0);
        }
    }

    private void PlayPickupAnimation()
    {
        StopHoverAnimation();
        isRespawning = true;

        Sequence pickupSequence = DOTween.Sequence();
        pickupSequence.Append(transform.DOScale(Vector3.zero, pickupScaleDuration));

        if (weaponShadow != null)
        {
            pickupSequence.Join(weaponShadow.DOScale(Vector3.zero, pickupScaleDuration));
        }

        pickupSequence.OnComplete(PlayRespawnAnimation);
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

        if (weaponShadow != null)
        {
            respawnSequence.Join(weaponShadow.DOScale(Vector3.one, respawnScaleDuration));
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
            playerWeapon = collision.GetComponentInChildren<Weapon>();
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

        if (collision.CompareTag("Player"))
        {
            playerWeapon = null;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        StopHoverAnimation();
    }
}