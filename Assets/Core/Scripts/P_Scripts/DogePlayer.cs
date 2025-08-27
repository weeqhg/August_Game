using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DogePlayer : MonoBehaviour
{
    [Header("Настройка кувырка")]
    [SerializeField] private float _dodgeCooldown = 1f;
    [SerializeField] private float _invincibilityDuration = 0.5f;
    [SerializeField] private LayerMask _wallLayerMask = 1;
    [SerializeField] private bool _enableDoubleDodge = true; // Включение двойного кувырка

    [Header("Настройки двойного кувырка")]
    [SerializeField] private float _doubleDodgeWindow = 0.5f; // Время для второго кувырка
    [SerializeField] private float _doubleDodgeCooldown = 2f; // Перезарядка после двойного кувырка

    [Header("DOTween настройки")]
    [SerializeField] private Ease _dodgeEase = Ease.OutCubic;
    [SerializeField] private float _wallCheckDistance = 0.5f;

    // Переменные для кувырка
    private float _dodgeSpeed = 2.3f;
    private float _dodgeDuration = 0.45f;
    private float _dodgeRotation = 360f;
    public bool isDodging = false;
    private Rigidbody2D _rb;
    private bool _canDodge = true;
    private Vector2 _dodgeDirection;
    private float _dodgeTimer;
    private float _cooldownTimer;
    private bool _isInvincible = false;

    // Переменные для двойного кувырка
    private int _dodgeCount = 0;
    private float _doubleDodgeTimer = 0f;
    private bool _isDoubleDodgeAvailable = false;

    // DOTween
    private Sequence _dodgeSequence;
    private Tween _dodgeTween;

    // Для определения направления
    private float _lastHorizontalInput = 1f; // По умолчанию вправо

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        DOTween.Init();
    }

    public void HandleDodgeInput(Vector2 moveInput)
    {
        // Сохраняем последнее горизонтальное направление для анимации
        if (moveInput.x != 0)
        {
            _lastHorizontalInput = Mathf.Sign(moveInput.x);
        }

        // Обновляем таймер двойного кувырка
        UpdateDoubleDodgeTimer();

        if (Input.GetKeyDown(KeyCode.Space) && _canDodge && !isDodging)
        {
            if (_enableDoubleDodge && _isDoubleDodgeAvailable && _dodgeCount == 1)
            {
                // Второй кувырок в серии
                StartDodge(moveInput);
            }
            else if (_dodgeCount == 0)
            {
                // Первый кувырок
                StartDodge(moveInput);
            }
        }
    }

    private void UpdateDoubleDodgeTimer()
    {
        if (_enableDoubleDodge && _dodgeCount > 0 && !isDodging)
        {
            _doubleDodgeTimer -= Time.deltaTime;
            if (_doubleDodgeTimer <= 0f)
            {
                ResetDodgeCount();
            }
        }
    }

    private void StartDodge(Vector2 moveInput)
    {
        // Получаем направление от игрока к курсору
        Vector2 cursorDirection = GetCursorDirection();

        // Если курсор слишком близко к игроку, используем последнее движение или направление взгляда
        if (cursorDirection.magnitude < 0.1f)
        {
            cursorDirection = GetFallbackDirection();
        }

        if (CheckWallInDirection(cursorDirection.normalized))
        {
            return;
        }

        isDodging = true;
        _isInvincible = true;

        _dodgeDirection = moveInput.normalized;
        _dodgeTimer = _dodgeDuration;

        // Увеличиваем счетчик кувырков
        _dodgeCount++;

        // Если это второй кувырок, устанавливаем специальную перезарядку
        if (_dodgeCount == 2)
        {
            _cooldownTimer = _doubleDodgeCooldown;
            _isDoubleDodgeAvailable = false;
        }
        else
        {
            _cooldownTimer = _dodgeCooldown;
            // Запускаем таймер для возможности второго кувырка
            if (_enableDoubleDodge)
            {
                _doubleDodgeTimer = _doubleDodgeWindow;
                _isDoubleDodgeAvailable = true;
            }
        }

        _canDodge = _dodgeCount < 2; // Разрешаем кувырок только если меньше 2

        StartDodgeAnimation();
    }

    private Vector2 GetCursorDirection()
    {
        // Получаем позицию курсора в мировых координатах
        Vector3 cursorWorldPos = GameUtils.GetMousePosition();
        cursorWorldPos.z = 0; // Обнуляем Z для 2D

        // Направление от игрока к курсору
        return (cursorWorldPos - transform.position).normalized;
    }

    private Vector2 GetFallbackDirection()
    {
        // Если курсор слишком близко или не доступен, используем альтернативные направления
        Vector2 fallbackDirection;

        // Сначала пытаемся использовать последнее движение
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.magnitude > 0.1f)
        {
            fallbackDirection = input.normalized;
        }
        // Если нет движения, используем направление взгляда игрока (последнее горизонтаное)
        else
        {
            fallbackDirection = new Vector2(_lastHorizontalInput, 0f);
        }

        return fallbackDirection;
    }

    private void StartDodgeAnimation()
    {
        StopDodgeAnimation();

        float actualDodgeDistance = CalculateSafeDodgeDistance();
        Vector2 targetPosition = _rb.position + _dodgeDirection * actualDodgeDistance;

        // Определяем направление вращения на основе положения курсора относительно игрока
        float rotationDirection = GetRotationDirectionBasedOnCursor();

        _dodgeSequence = DOTween.Sequence();

        // Анимация движения
        _dodgeSequence.Append(
            DOTween.To(() => _rb.position, x => _rb.position = x, targetPosition, _dodgeDuration)
                .SetEase(Ease.OutQuint)
        );

        // ПЛАВНАЯ анимация вращения
        _dodgeSequence.Join(
            transform.DORotate(new Vector3(0, 0, _dodgeRotation * rotationDirection), _dodgeDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutSine)
        );

        // ПЛАВНАЯ анимация масштаба
        _dodgeSequence.Join(
            transform.DOScale(new Vector3(0.85f, 0.85f, 1f), _dodgeDuration * 0.3f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad)
        );

        _dodgeSequence.OnComplete(EndDodge);
        _dodgeSequence.OnKill(() =>
        {
            if (isDodging) EndDodge();
        });
    }

    private float GetRotationDirectionBasedOnCursor()
    {
        // Получаем позицию курсора относительно игрока
        Vector3 cursorWorldPos = GameUtils.GetMousePosition();
        Vector3 relativeCursorPos = cursorWorldPos - transform.position;

        // Получаем текущее направление движения
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        bool isMovingAwayFromCursor = IsMovingAwayFromCursor(moveInput, relativeCursorPos);

        // Определяем, находится ли курсор слева или справа от игрока
        bool isCursorOnLeft = relativeCursorPos.x > 0;

        // Логика вращения:
        // Если двигаемся ОТ курсора - вращение стандартное
        // Если двигаемся К курсору - вращение противоположное
        if (isMovingAwayFromCursor)
        {
            // Движение ОТ курсора: стандартное вращение
            return isCursorOnLeft ? 1f : -1f;
        }
        else
        {
            // Движение К курсору: противоположное вращение
            return isCursorOnLeft ? -1f : 1f;
        }
    }

    private bool IsMovingAwayFromCursor(Vector2 moveInput, Vector3 relativeCursorPos)
    {
        if (moveInput.magnitude < 0.1f)
            return true; // Если не двигаемся, считаем что движемся от курсора (стандартное поведение)

        // Нормализуем векторы для сравнения
        Vector2 moveDirection = moveInput.normalized;
        Vector2 cursorDirection = new Vector2(relativeCursorPos.x, relativeCursorPos.y).normalized;

        // Вычисляем угол между направлением движения и направлением к курсору
        float dotProduct = Vector2.Dot(moveDirection, cursorDirection);

        // Если скалярное произведение отрицательное - двигаемся ОТ курсора
        // Если положительное - двигаемся К курсору
        return dotProduct < 0f;
    }

    private float CalculateSafeDodgeDistance()
    {
        float maxDistance = _dodgeSpeed * _dodgeDuration;

        RaycastHit2D hit = Physics2D.Raycast(
            _rb.position,
            _dodgeDirection,
            maxDistance,
            _wallLayerMask
        );

        if (hit.collider != null)
        {
            float safeDistance = hit.distance - _wallCheckDistance;
            return Mathf.Max(0, safeDistance);
        }

        return maxDistance;
    }

    private bool CheckWallInDirection(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            _rb.position,
            direction,
            _wallCheckDistance,
            _wallLayerMask
        );

        return hit.collider != null;
    }

    private void StopDodgeAnimation()
    {
        if (_dodgeSequence != null && _dodgeSequence.IsActive())
        {
            _dodgeSequence.Kill();
        }
        if (_dodgeTween != null && _dodgeTween.IsActive())
        {
            _dodgeTween.Kill();
        }
    }

    public void UpdateTimers()
    {
        if (isDodging)
        {
            _dodgeTimer -= Time.deltaTime;
            if (_dodgeTimer <= 0f && (_dodgeSequence == null || !_dodgeSequence.IsActive()))
            {
                EndDodge();
            }
        }

        if (!_canDodge)
        {
            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer <= 0f)
            {
                _canDodge = true;
                ResetDodgeCount(); // Сбрасываем счетчик когда перезарядка закончилась
            }
        }

        if (_isInvincible && !isDodging)
        {
            _invincibilityDuration -= Time.deltaTime;
            if (_invincibilityDuration <= 0f)
            {
                _isInvincible = false;
            }
        }
    }

    private void ResetDodgeCount()
    {
        _dodgeCount = 0;
        _isDoubleDodgeAvailable = false;
        _doubleDodgeTimer = 0f;
    }

    public void DogeMove()
    {
        // Движение управляется DOTween
    }

    private void EndDodge()
    {
        isDodging = false;
        _isInvincible = false;
        ResetDodgeTransform();
        _rb.velocity = Vector2.zero;
        StopDodgeAnimation();

        // После завершения кувырка проверяем, нужно ли сбрасывать счетчик
        if (!_enableDoubleDodge || _dodgeCount >= 2)
        {
            ResetDodgeCount();
        }
    }

    private void ResetDodgeTransform()
    {
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public void ForceStopDodge()
    {
        if (isDodging)
        {
            EndDodge();
            ResetDodgeCount(); // Принудительно сбрасываем счетчик
        }
    }

    // Метод для включения/выключения двойного кувырка
    public void SetDoubleDodgeEnabled(bool enabled)
    {
        _enableDoubleDodge = enabled;
        if (!enabled)
        {
            ResetDodgeCount();
        }
    }

    // Метод для проверки доступности двойного кувырка
    public bool IsDoubleDodgeAvailable()
    {
        return _enableDoubleDodge && _isDoubleDodgeAvailable;
    }

    // Метод для получения текущего счетчика кувырков
    public int GetDodgeCount()
    {
        return _dodgeCount;
    }

    // Визуализация для отладки
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && isDodging)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)_dodgeDirection * 2f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)_dodgeDirection * _wallCheckDistance);

            // Показываем направление вращения
            Gizmos.color = Color.green;
            float rotationDir = GetRotationDirectionBasedOnCursor();
            Vector3 rotationIndicator = rotationDir > 0 ? Vector3.right : Vector3.left;
            Gizmos.DrawLine(transform.position, transform.position + rotationIndicator * 1.5f);
        }

        // Визуализация направления к курсору
        if (Application.isPlaying)
        {
            Vector3 cursorWorldPos = GameUtils.GetMousePosition();
            cursorWorldPos.z = 0;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, cursorWorldPos);
            Gizmos.DrawWireSphere(cursorWorldPos, 0.2f);

            // Показываем сторону курсора относительно игрока
            Gizmos.color = cursorWorldPos.x < transform.position.x ? Color.red : Color.blue;
            Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, cursorWorldPos.y, 0));
        }
    }

    private void OnDestroy()
    {
        StopDodgeAnimation();
    }
}