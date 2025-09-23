using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractHeart : PlayerInteract
{
    [Header("Accessory Settings")]
    [SerializeField] private float valueRecovery;
    [SerializeField] private Transform heartShadow;
    [SerializeField] private float hoverHeight = 0.2f;
    [SerializeField] private float hoverDuration = 1f;
    [SerializeField] private float pickupScaleDuration = 0.3f;

    private Tween hoverTween;
    private bool isRespawning = false;
    private bool isBeingDestroyed = false;

    public override void Interact() { }


    public void Initialize()
    {
        StartHoverAnimation();
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
        if (heartShadow != null && heartShadow.gameObject.activeInHierarchy)
        {
            heartShadow.position = new Vector3(transform.position.x, transform.position.y, 0);
        }
    }

    private void PlayPickupAnimation()
    {
        StopHoverAnimation();

        Sequence pickupSequence = DOTween.Sequence();
        pickupSequence.Append(transform.DOScale(Vector3.zero, pickupScaleDuration));

    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBeingDestroyed) return;
        base.OnTriggerEnter2D(collision);

        if (collision.CompareTag("Player"))
        {
            if (isBeingDestroyed) return;
            isBeingDestroyed = true;

            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            playerHealth.RecoveryHealth(valueRecovery);
            Destroy(transform.parent.gameObject, pickupScaleDuration);
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (isBeingDestroyed) return;
        base.OnTriggerExit2D(collision);

        if (collision.CompareTag("Player"))
        { }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        StopAllCoroutines();
        StopHoverAnimation();

        // Безопасное завершение всех твинов
        if (this != null)
        {
            DOTween.Kill(transform);
            if (heartShadow != null)
            {
                DOTween.Kill(heartShadow);
            }

            // Завершаем все последовательности
            DOTween.Kill(this);
        }

    }
}