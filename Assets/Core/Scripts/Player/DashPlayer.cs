using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DashPlayer : MonoBehaviour
{
    [Header("Настройка рывка")]
    [SerializeField] private LayerMask _wallLayerMask = 1;

    [Header("DOTween настройки")]
    [SerializeField] private Ease _dashEase = Ease.OutCubic;
    [SerializeField] private float _wallCheckDistance = 0.5f;

    // Переменные для рывка
    private readonly float _dashSpeed = 2.3f;
    private readonly float _dashDuration = 0.45f;
    private readonly float _dashRotation = 360f;
    private bool _isDashing = false;
    private Rigidbody2D _rb;
    private Vector2 _dashDirection;
    private float _dashTimer;
    private DashSystem _dashSystem;

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

    [Header("Настройки частиц")]
    [SerializeField] private ParticleSystem _dashParticlePrefab;
    [SerializeField] private float _particleSpawnInterval = 0.05f;
    [SerializeField] private int _poolSize = 24;

    private Queue<ParticleSystem> _particlePool = new Queue<ParticleSystem>();
    private BoxCollider2D _boxCollider2D;

    // Переменные для системы частиц
    private float _particleTimer = 0f;

    private void Start()
    {
        _dashSystem = GetComponent<DashSystem>();
        _movePlayer = GetComponent<MovePlayer>();
        _boxCollider2D = GetComponent<BoxCollider2D>();

        _rb = GetComponent<Rigidbody2D>();
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;


        DOTween.Init(recycleAllByDefault: false, useSafeMode: true, logBehaviour: LogBehaviour.ErrorsOnly);

        CreateParticlePool();

        if (_dashSystem == null)
        {
            Debug.LogError("DashSystem not found on player!");
            return;
        }
    }

    private void CreateParticlePool()
    {
        GameObject parent = new GameObject("DashParticlesPool");
        for (int i = 0; i < _poolSize; i++)
        {
            ParticleSystem particle = Instantiate(_dashParticlePrefab, parent.transform);
            particle.gameObject.SetActive(false);
            _particlePool.Enqueue(particle);
        }
    }

    private void Update()
    {
        HandleDashInput();
        UpdateTimers();
        UpdateTrailParticles();
    }

    private void UpdateTrailParticles()
    {
        if (_isDashing)
        {
            _particleTimer += Time.deltaTime;
            if (_particleTimer >= _particleSpawnInterval)
            {
                SpawnTrailParticle();
                _particleTimer = 0f;
            }
        }
    }

    private void SpawnTrailParticle()
    {
        if (_particlePool.Count > 0)
        {
            ParticleSystem particle = _particlePool.Dequeue();
            particle.transform.position = transform.position;
            particle.gameObject.SetActive(true);
            particle.Play();

            StartCoroutine(ReturnToPoolAfterLifetime(particle));
        }
    }

    private IEnumerator ReturnToPoolAfterLifetime(ParticleSystem particle)
    {
        yield return new WaitForSeconds(particle.main.duration);
        particle.gameObject.SetActive(false);
        _particlePool.Enqueue(particle);
    }

    public void HandleDashInput()
    {
        if (_isDashCompleting || _dashSystem == null) return;

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        if (Input.GetKeyDown(KeyCode.Space) && _dashSystem.CanDash && !_isDashing)
        {
            UseDash(moveInput);
        }
    }

    private void UseDash(Vector2 moveInput)
    {
        // Проверяем наличие DashSystem
        if (_dashSystem == null)
        {
            Debug.LogError("DashSystem is null!");
            return;
        }

        // Определяем направление ДО использования заряда
        _dashDirection = CalculateDashDirection(moveInput);

        if (CheckWallInDirection(_dashDirection.normalized))
        {
            return;
        }

        // Используем заряд через DashSystem
        if (_dashSystem.TryUseDash())
        {
            _isDashing = true;
            _dashTimer = _dashDuration;
            _preDashVelocity = _rb.velocity;

            if (_rb != null) _rb.isKinematic = true;
            if (_boxCollider2D != null) _boxCollider2D.enabled = false;
            if (_rb != null) _rb.velocity = Vector2.zero;

            StartDashAnimation();
        }
    }

    private Vector2 CalculateDashDirection(Vector2 moveInput)
    {
        Vector2 cursorDirection = GetCursorDirection();
        bool isMoving = moveInput.magnitude > 0.1f || (_rb != null && _rb.velocity.magnitude > 0.5f);

        if (isMoving)
        {
            float movementWeight = 0.7f;
            Vector2 movementDirection;

            if (moveInput.magnitude > 0.1f)
            {
                movementDirection = moveInput.normalized;
            }
            else
            {
                movementDirection = _rb.velocity.normalized;
            }

            Vector2 blendedDirection = (movementDirection * movementWeight + cursorDirection * (1 - movementWeight)).normalized;
            return blendedDirection;
        }
        else
        {
            return cursorDirection;
        }
    }

    private void StartDashAnimation()
    {
        StopDashAnimation();

        float actualDashDistance = CalculateSafeDashDistance();
        Vector3 targetPosition = transform.position + (Vector3)_dashDirection * actualDashDistance;
        float rotationDirection = GetRotationDirectionBasedOnCursor();

        _dashSequence = DOTween.Sequence();

        _dashSequence.Append(
            transform.DOMove(targetPosition, _dashDuration)
                .SetEase(_dashEase)
                .SetUpdate(UpdateType.Fixed)
        );

        _dashSequence.Join(
            transform.DOLocalRotate(new Vector3(0, 0, _dashRotation * rotationDirection),
                                  _dashDuration,
                                  RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine)
                .SetUpdate(UpdateType.Fixed)
        );

        _dashSequence.Join(
            transform.DOScale(new Vector3(0.85f, 0.85f, 1f), _dashDuration * 0.3f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad)
                .SetUpdate(UpdateType.Fixed)
        );

        _dashSequence.OnComplete(() =>
        {
            if (_isDashing)
            {
                EndDash();
            }
        });

        _dashSequence.OnKill(() =>
        {
            if (_isDashing && !_isDashCompleting)
            {
                EndDash();
            }
        });

        _dashSequence.Play();
    }

    private void EndDash()
    {
        if (_isDashCompleting) return;
        _isDashCompleting = true;

        SafeStopDashAnimation();

        // Включаем обратно физику с проверкой на null
        if (_rb != null)
        {
            _rb.isKinematic = false;
            if (_movePlayer != null)
            {
                StartCoroutine(SmoothVelocityRestore());
            }
            else
            {
                _rb.velocity = _preDashVelocity;
            }
        }

        if (_boxCollider2D != null)
        {
            _boxCollider2D.enabled = true;
        }

        ResetDashTransform();

        _isDashing = false;
        _isDashCompleting = false;
    }

    private IEnumerator SmoothVelocityRestore()
    {
        float restoreTime = 0.1f;
        float elapsed = 0f;
        Vector2 startVelocity = Vector2.zero;

        while (elapsed < restoreTime && _rb != null)
        {
            _rb.velocity = Vector2.Lerp(startVelocity, _preDashVelocity, elapsed / restoreTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_rb != null)
        {
            _rb.velocity = _preDashVelocity;
        }
    }

    private Vector2 GetCursorDirection()
    {
        Vector3 cursorWorldPos = GameUtils.GetMousePosition();
        cursorWorldPos.z = 0;

        Vector2 directionToCursor = (cursorWorldPos - transform.position).normalized;

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
        else if (_rb != null && _rb.velocity.magnitude > 0.1f)
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

        float dotProduct = Vector2.Dot(_dashDirection, new Vector2(relativeCursorPos.x, relativeCursorPos.y).normalized);

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

    private void UpdateTimers()
    {
        if (_isDashing)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f && !_isDashCompleting)
            {
                EndDash();
            }
        }
    }

    private void ResetDashTransform()
    {
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    private void SafeStopDashAnimation()
    {
        if (_dashSequence != null)
        {
            _dashSequence.OnComplete(null);
            _dashSequence.OnKill(null);

            if (_dashSequence.IsActive())
            {
                _dashSequence.Kill(false);
            }
            _dashSequence = null;
        }
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