using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    [Header("Настройка перемещения игрока")]
    [SerializeField] private float _moveSpeed;
    private float _acceleration = 20f;

    private Animator _animator;
    private Rigidbody2D _rb;
    private Vector2 _smoothVelocity;
    private Vector2 _moveInput;
    private DashPlayer _dashPlayer;

    // Флаги для отслеживания состояния ввода
    private bool _isMoving = false;
    private Vector2 _lastMoveInput;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _dashPlayer = GetComponent<DashPlayer>();
        _rb.gravityScale = 0;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        if (_dashPlayer != null && _dashPlayer.IsDashing)
            return;

        // Получаем сырой ввод (без нормализации)
        Vector2 rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Проверяем, изменился ли ввод
        bool wasMoving = _isMoving;
        _isMoving = rawInput.magnitude > 0.1f;

        // Если только что отпустили кнопки - резко останавливаемся
        if (wasMoving && !_isMoving)
        {
            _rb.velocity = Vector2.zero;
            _smoothVelocity = Vector2.zero;
        }

        // Нормализуем только если есть движение
        _moveInput = _isMoving ? rawInput.normalized : Vector2.zero;

        AnimationRun();
    }

    private void AnimationRun()
    {
        _animator.SetFloat("isRun", _moveInput.magnitude);
    }

    private void FixedUpdate()
    {
        if (_dashPlayer != null && _dashPlayer.IsDashing)
            return;

        // Если нет движения - не применяем SmoothDamp
        if (!_isMoving)
        {
            // Дополнительная гарантия остановки
            if (_rb.velocity.magnitude > 0.1f)
            {
                _rb.velocity = Vector2.zero;
            }
            return;
        }

        Vector2 targetVelocity = _moveInput * _moveSpeed;
        float smoothTime = 1f / _acceleration;

        _rb.velocity = Vector2.SmoothDamp(_rb.velocity, targetVelocity, ref _smoothVelocity, smoothTime);
    }
}