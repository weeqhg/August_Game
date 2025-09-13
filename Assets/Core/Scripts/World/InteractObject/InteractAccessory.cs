using DG.Tweening;
using UnityEngine;

/// <summary>
/// InteractAccessory отвечает за взаимодействие игрока с аксессуарами в мире.
/// Аксессуары имеют анимацию парения, анимацию подбора
/// Сохраняется при использовании, просто становится неактивным.
/// <summary>

public class InteractAccessory : PlayerInteract
{
    [Header("Accessory Settings")]
    [SerializeField] private Transform accessoryShadow;
    [SerializeField] private float hoverHeight = 0.2f;
    [SerializeField] private float hoverDuration = 1f;
    [SerializeField] private float pickupScaleDuration = 0.3f;
    [SerializeField] private float respawnScaleDuration = 0.5f;
    [SerializeField] private float kickForce = 0.1f;

    [SerializeField] private AccessoryConfig accessoryConfig;
    private PlayerAccessoryWeapon accessoryWeapon;
    private Tween hoverTween;
    private Vector3 originalPosition;
    private bool isRespawning = false;
    private bool isBeingDestroyed = false;

    public void Initialize(AccessoryConfig config, Sprite sprite, Transform spawnTransform, SpriteRenderer spriteR)
    {
        accessoryConfig = config;
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
        if (isRespawning || accessoryWeapon == null || isBeingDestroyed) return;
        accessoryWeapon.ChangeAccessoryConfig(accessoryConfig, gameObject);
        PlayPickupAnimation();
    }

    private void StartHoverAnimation()
    {
        if (isRespawning || isBeingDestroyed || !isActiveAndEnabled) return;

        StopHoverAnimation();
        UpdateShadowPosition();

        hoverTween = transform.DOMoveY(transform.position.y + hoverHeight, hoverDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .OnKill(() => hoverTween = null);
    }

    private void StopHoverAnimation()
    {
        if (hoverTween != null && hoverTween.IsActive())
        {
            hoverTween.Kill();
            hoverTween = null;
        }
    }

    private void UpdateShadowPosition()
    {
        if (accessoryShadow != null && accessoryShadow.gameObject.activeInHierarchy)
        {
            accessoryShadow.position = new Vector3(transform.position.x, transform.position.y, 0);
        }
    }

    private void PlayPickupAnimation()
    {
        if (isBeingDestroyed) return;

        StopHoverAnimation();
        isRespawning = true;

        Sequence pickupSequence = DOTween.Sequence();
        pickupSequence.Append(transform.DOScale(Vector3.zero, pickupScaleDuration));

        if (accessoryShadow != null && accessoryShadow.gameObject.activeInHierarchy)
        {
            pickupSequence.Join(accessoryShadow.DOScale(Vector3.zero, pickupScaleDuration));
        }

        pickupSequence.OnComplete(() =>
        {
            if (!isBeingDestroyed)
            {
                PlayRespawnAnimation();
            }
        });
    }

    private void PlayRespawnAnimation()
    {
        if (isBeingDestroyed) return;

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 kickPosition = originalPosition + (Vector3)randomDirection * kickForce;

        transform.position = kickPosition;
        transform.localScale = Vector3.zero;
        UpdateShadowPosition();

        Sequence respawnSequence = DOTween.Sequence();
        respawnSequence.Append(transform.DOScale(Vector3.one, respawnScaleDuration));

        if (accessoryShadow != null && accessoryShadow.gameObject.activeInHierarchy)
        {
            respawnSequence.Join(accessoryShadow.DOScale(Vector3.one, respawnScaleDuration));
        }

        respawnSequence.OnComplete(() =>
        {
            if (!isBeingDestroyed)
            {
                isRespawning = false;
                originalPosition = transform.position;
                StartHoverAnimation();
            }
        });
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBeingDestroyed) return;
        base.OnTriggerEnter2D(collision);

        if (collision.CompareTag("Player"))
        {
            accessoryWeapon = collision.GetComponentInChildren<PlayerAccessoryWeapon>();
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (isBeingDestroyed) return;
        base.OnTriggerExit2D(collision);

        if (collision.CompareTag("Player"))
        {
            accessoryWeapon = null;
        }
    }

    protected override void OnDestroy()
    {
        isBeingDestroyed = true;
        StopAllCoroutines();
        StopHoverAnimation();

        // Безопасное завершение всех твинов
        if (this != null)
        {
            DOTween.Kill(transform);
            if (accessoryShadow != null)
            {
                DOTween.Kill(accessoryShadow);
            }

            // Завершаем все последовательности
            DOTween.Kill(this);
        }

        base.OnDestroy();
    }

    // Добавьте также обработку отключения
    private void OnDisable()
    {
        if (!isBeingDestroyed)
        {
            StopHoverAnimation();
            DOTween.Kill(transform);
            if (accessoryShadow != null)
            {
                DOTween.Kill(accessoryShadow);
            }
        }
    }


    // Добавляем обработчик включения объекта
    private void OnEnable()
    {
        if (!isBeingDestroyed && !isRespawning)
        {
            StartHoverAnimation();
        }
    }
}