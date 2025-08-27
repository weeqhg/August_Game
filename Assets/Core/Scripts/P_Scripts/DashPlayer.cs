using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DashPlayer : MonoBehaviour
{
    [Header("Настройка рывка")]
    [SerializeField] private float _dashCooldown = 1f;
    [SerializeField] private float _invincibilityDuration = 0.5f;
    [SerializeField] private LayerMask _wallLayerMask = 1;

    [Header("DOTween настройки")]
    [SerializeField] private Ease _dashEase = Ease.OutCubic;
    [SerializeField] private float _wallCheckDistance = 0.5f;

    // Переменные для рывка
    private float _dashSpeed = 2.3f;
    private float _dashDuration = 0.45f;
    private float _dashRotation = 360f;
    private bool _isDashing = false;
    private Rigidbody2D _rb;
    private bool _canDash = true;
    private Vector2 _dashDirection;
    private float _dashTimer;
    private float _cooldownTimer;
    private bool _isInvincible = false;

    // DOTween
    private Sequence _dashSequence;

    // Для определения направления
    private float _lastHorizontalInput = 1f;

    // Ссылка на MovePlayer для временного отключения
    private MovePlayer _movePlayer;

    // Сохраняем последнюю скорость перед рывком
    private Vector2 _preDashVelocity;

    public bool IsDashing => _isDashing;

    // Флаг для отслеживания завершения рывка
    private bool _isDashCompleting = false;
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _movePlayer = GetComponent<MovePlayer>();

        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Инициализация DOTween (упрощенная)

        DOTween.Init(recycleAllByDefault: false, useSafeMode: true, logBehaviour: LogBehaviour.ErrorsOnly);

    }

    private void Update()
    {
        HandleDashInput();
        UpdateTimers();
    }

    public void HandleDashInput()
    {
        // Если рывок завершается, не обрабатываем ввод
        if (_isDashCompleting) return;

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

        if (moveInput.x != 0)
        {
            _lastHorizontalInput = Mathf.Sign(moveInput.x);
        }

        if (Input.GetKeyDown(KeyCode.Space) && _canDash && !_isDashing)
        {
            StartDash(moveInput);
        }
    }

    private void StartDash(Vector2 moveInput)
    {
        // Определяем направление рывка на основе движения и курсора
        _dashDirection = CalculateDashDirection(moveInput);

        if (CheckWallInDirection(_dashDirection.normalized))
        {
            return;
        }

        _isDashing = true;
        _canDash = false;
        _isInvincible = true;

        _dashTimer = _dashDuration;
        _cooldownTimer = _dashCooldown;

        // Сохраняем текущую скорость перед рывком
        _preDashVelocity = _rb.velocity;

        // Отключаем физику для избежания конфликтов
        _rb.isKinematic = true;
        _rb.velocity = Vector2.zero;

        StartDashAnimation();
    }

    private Vector2 CalculateDashDirection(Vector2 moveInput)
    {
        // Получаем направление к курсору
        Vector2 cursorDirection = GetCursorDirection();

        // Проверяем, движется ли персонаж
        bool isMoving = moveInput.magnitude > 0.1f || _rb.velocity.magnitude > 0.5f;

        if (isMoving)
        {
            // Если персонаж движется, используем КОМБИНАЦИЮ направления движения и курсора
            // Вес направления движения (0.7 = 70% движения, 30% курсора)
            float movementWeight = 0.7f;

            // Нормализуем направление движения (предпочтение отдается input, но если input маленький, используем velocity)
            Vector2 movementDirection;
            if (moveInput.magnitude > 0.1f)
            {
                movementDirection = moveInput.normalized;
            }
            else
            {
                movementDirection = _rb.velocity.normalized;
            }

            // Смешиваем направление движения и направление к курсору
            Vector2 blendedDirection = (movementDirection * movementWeight + cursorDirection * (1 - movementWeight)).normalized;

            return blendedDirection;
        }
        else
        {
            // Если персонаж стоит на месте, используем только направление к курсору
            return cursorDirection;
        }
    }

    private void StartDashAnimation()
    {
        // Останавливаем предыдущие анимации
        StopDashAnimation();

        float actualDashDistance = CalculateSafeDashDistance();
        Vector3 targetPosition = transform.position + (Vector3)_dashDirection * actualDashDistance;

        float rotationDirection = GetRotationDirectionBasedOnCursor();

        // Создаем новую последовательность
        _dashSequence = DOTween.Sequence();

        // Анимация движения
        _dashSequence.Append(
            transform.DOMove(targetPosition, _dashDuration)
                .SetEase(_dashEase)
                .SetUpdate(UpdateType.Fixed) // Используем FixedUpdate для физики
        );

        // Анимация вращения
        _dashSequence.Join(
            transform.DOLocalRotate(new Vector3(0, 0, _dashRotation * rotationDirection),
                                  _dashDuration,
                                  RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine)
                .SetUpdate(UpdateType.Fixed)
        );

        // Анимация масштаба
        _dashSequence.Join(
            transform.DOScale(new Vector3(0.85f, 0.85f, 1f), _dashDuration * 0.3f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad)
                .SetUpdate(UpdateType.Fixed)
        );

        // Настраиваем завершение
        _dashSequence.OnComplete(() =>
        {
            if (_isDashing)
            {
                EndDash();
            }
        });

        _dashSequence.OnKill(() =>
        {
            // Защита от рекурсивных вызовов
            if (_isDashing && !_isDashCompleting)
            {
                EndDash();
            }
        });

        // Запускаем последовательность
        _dashSequence.Play();
    }

    private void EndDash()
    {
        // Защита от множественных вызовов
        if (_isDashCompleting) return;

        _isDashCompleting = true;

        // Останавливаем анимации безопасным способом
        SafeStopDashAnimation();

        // Включаем обратно физику
        _rb.isKinematic = false;

        // Плавно восстанавливаем предыдущую скорость
        if (_movePlayer != null)
        {
            StartCoroutine(SmoothVelocityRestore());
        }
        else
        {
            _rb.velocity = _preDashVelocity;
        }

        // Сбрасываем трансформации
        ResetDashTransform();

        // Сбрасываем флаги
        _isInvincible = false;
        _isDashing = false;
        _isDashCompleting = false;
    }

    // Безопасная остановка анимаций
    private void SafeStopDashAnimation()
    {
        if (_dashSequence != null)
        {
            // Отключаем колбэки перед убийством последовательности
            _dashSequence.OnComplete(null);
            _dashSequence.OnKill(null);

            if (_dashSequence.IsActive())
            {
                _dashSequence.Kill(false); // false - не вызывать колбэк OnComplete
            }
            _dashSequence = null;
        }
    }

    // Плавное восстановление скорости после рывка
    private IEnumerator SmoothVelocityRestore()
    {
        float restoreTime = 0.1f;
        float elapsed = 0f;
        Vector2 startVelocity = Vector2.zero;

        while (elapsed < restoreTime)
        {
            _rb.velocity = Vector2.Lerp(startVelocity, _preDashVelocity, elapsed / restoreTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _rb.velocity = _preDashVelocity;
    }

    private Vector2 GetCursorDirection()
    {
        Vector3 cursorWorldPos = GameUtils.GetMousePosition();
        cursorWorldPos.z = 0;

        // Направление от игрока к курсору
        Vector2 directionToCursor = (cursorWorldPos - transform.position).normalized;

        // Если курсор слишком близко к игроку, используем запасное направление
        if (directionToCursor.magnitude < 0.1f)
        {
            return GetFallbackDirection();
        }

        return directionToCursor;
    }

    private Vector2 GetFallbackDirection()
    {
        Vector2 fallbackDirection;
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (input.magnitude > 0.1f)
        {
            fallbackDirection = input.normalized;
        }
        else if (_rb.velocity.magnitude > 0.1f)
        {
            fallbackDirection = _rb.velocity.normalized;
        }
        else
        {
            fallbackDirection = new Vector2(_lastHorizontalInput, 0f);
        }

        return fallbackDirection;
    }

    private float GetRotationDirectionBasedOnCursor()
    {
        Vector3 cursorWorldPos = GameUtils.GetMousePosition();
        Vector3 relativeCursorPos = cursorWorldPos - transform.position;
        bool isCursorOnLeft = relativeCursorPos.x < 0;

        // Определяем направление вращения на основе положения курсора
        // и направления рывка
        float dotProduct = Vector2.Dot(_dashDirection, new Vector2(relativeCursorPos.x, relativeCursorPos.y).normalized);

        // Если рывок в направлении курсора - одно направление вращения,
        // если от курсора - противоположное
        if (dotProduct > 0)
        {
            return isCursorOnLeft ? 1f : -1f;
        }
        else
        {
            return isCursorOnLeft ? -1f : 1f;
        }
    }

    private float CalculateSafeDashDistance()
    {
        float maxDistance = _dashSpeed * _dashDuration;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _dashDirection, maxDistance, _wallLayerMask);

        if (hit.collider != null)
        {
            float safeDistance = hit.distance - _wallCheckDistance;
            return Mathf.Max(0.1f, safeDistance);
        }

        return maxDistance;
    }

    private bool CheckWallInDirection(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, _wallCheckDistance, _wallLayerMask);
        return hit.collider != null;
    }

    private void StopDashAnimation()
    {
        if (_dashSequence != null && _dashSequence.IsActive())
        {
            _dashSequence.Kill();
        }
        _dashSequence = null;
    }

    public void UpdateTimers()
    {
        if (_isDashing)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f && !_isDashCompleting)
            {
                EndDash();
            }
        }

        if (!_canDash)
        {
            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer <= 0f)
            {
                _canDash = true;
            }
        }
    }

    private void ResetDashTransform()
    {
        // Немедленный сброс вместо твинов (чтобы избежать конфликтов)
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    private void OnDestroy()
    {
        SafeStopDashAnimation();
    }

    private void OnDisable()
    {
        SafeStopDashAnimation();
    }
}