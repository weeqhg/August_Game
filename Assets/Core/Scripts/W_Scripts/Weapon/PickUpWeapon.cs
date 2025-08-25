using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpWeapon : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private WeaponConfig _weaponConfig;
    [SerializeField] private Sprite _weaponSprite;

    [Header("Анимации")]
    [SerializeField] private float _hoverHeight = 0.2f;
    [SerializeField] private float _hoverDuration = 1f;
    [SerializeField] private float _pickupScaleDuration = 0.3f;
    [SerializeField] private float _respawnScaleDuration = 0.5f;
    [SerializeField] private float _kickForce = 2f;
    [SerializeField] private Material _outlineMaterial;

    private bool _isPickUp = false;
    private SpriteRenderer _spriteRenderer;
    private Weapon _weapon;
    private Tween _hoverTween;
    private Vector3 _originalPosition;
    private Material _originalMaterial;
    private bool _isRespawning = false;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Initialize(_weaponConfig, _weaponSprite, transform);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && _isPickUp)
        {
            Interact();
        }
    }

    public void Initialize(WeaponConfig wConfig, Sprite sprite, Transform tChest)
    {
        _weaponConfig = wConfig;
        _spriteRenderer.sprite = sprite;
        _originalPosition = tChest.position;
        _originalMaterial = _spriteRenderer.material;

        StartHoverAnimation();
    }

    private void StartHoverAnimation()
    {
        if (_isRespawning) return;

        StopHoverAnimation();

        // Сохраняем текущую позицию как оригинальную для анимации
        Vector3 currentPosition = transform.position;

        _hoverTween = transform.DOMoveY(currentPosition.y + _hoverHeight, _hoverDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .OnKill(() => {
                // Плавно возвращаемся к исходной позиции при убийстве твина
                transform.DOMoveY(currentPosition.y, 0.2f).SetEase(Ease.OutSine);
            });
    }

    private void StopHoverAnimation()
    {
        if (_hoverTween != null && _hoverTween.IsActive())
        {
            _hoverTween.Kill();
            _hoverTween = null;
        }
    }

    private void PlayPickupAnimation()
    {
        StopHoverAnimation();
        _isRespawning = true;

        // Анимация исчезновения через scale
        Sequence pickupSequence = DOTween.Sequence();

        pickupSequence.Append(transform.DOScale(Vector3.zero, _pickupScaleDuration)
            .SetEase(Ease.InBack));

        pickupSequence.OnComplete(() =>
        {
            // После исчезновения - появляемся снова
            PlayRespawnAnimation();
        });
    }

    private void PlayRespawnAnimation()
    {
        // Рандомное направление для отталкивания
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 kickPosition = _originalPosition + (Vector3)randomDirection * _kickForce;

        // Устанавливаем новую позицию сразу
        transform.position = kickPosition;

        // Сбрасываем scale
        transform.localScale = Vector3.zero;

        Sequence respawnSequence = DOTween.Sequence();

        // Анимация scale - появляемся
        respawnSequence.Append(transform.DOScale(Vector3.one, _respawnScaleDuration)
            .SetEase(Ease.OutBack));

        // Небольшая дополнительная анимация для плавности
        respawnSequence.Join(transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f, 2, 0.5f)
            .SetDelay(_respawnScaleDuration * 0.5f));

        respawnSequence.OnComplete(() =>
        {
            _isRespawning = false;
            // Обновляем оригинальную позицию после завершения всей анимации
            _originalPosition = transform.position;

            // Запускаем анимацию парения снова от новой позиции
            StartHoverAnimation();
        });
    }

    private void EnableOutline()
    {
        _spriteRenderer.material = _outlineMaterial;
    }

    private void DisableOutline()
    {
        _spriteRenderer.material = _originalMaterial;
    }

    private void Interact()
    {
        _spriteRenderer.sprite = _weapon.weaponSpriteRenderer.sprite;
        WeaponConfig previous = _weapon.weaponConfig;
        _weapon.ChangeWeaponConfig(_weaponConfig);
        _weaponConfig = previous;

        PlayPickupAnimation();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPickUp = true;
            _weapon = collision.GetComponentInChildren<Weapon>();
            Debug.Log(_weapon);
            EnableOutline();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPickUp = false;
            DisableOutline();
        }
    }

    private void OnDestroy()
    {
        StopHoverAnimation();
    }
}