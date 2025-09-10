using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    [Header("Настройка перемещения игрока")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _stopSharpness = 30f; // Резкость остановки

    private float _acceleration = 20f;
    private Animator _animator;
    private Rigidbody2D _rb;
    private Vector2 _smoothVelocity;
    private Vector2 _moveInput;
    private DashPlayer _dashPlayer;

    private bool _isMoving = false;
    private Vector2 _lastRawInput;

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

        // Получаем сырой ввод
        Vector2 rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Проверяем изменение ввода
        bool inputChanged = rawInput != _lastRawInput;
        _lastRawInput = rawInput;

        bool wasMoving = _isMoving;
        _isMoving = rawInput.magnitude > 0.1f;

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

        if (!_isMoving)
        {
            // Резкое гашение остаточной скорости
            if (_rb.velocity.magnitude > 0.01f)
            {
                _rb.velocity = Vector2.Lerp(_rb.velocity, Vector2.zero, _stopSharpness * Time.fixedDeltaTime);
            }
            else
            {
                _rb.velocity = Vector2.zero;
            }
            return;
        }

        // Плавное ускорение при движении
        Vector2 targetVelocity = _moveInput * _moveSpeed;
        float smoothTime = 1f / _acceleration;

        _rb.velocity = Vector2.SmoothDamp(_rb.velocity, targetVelocity, ref _smoothVelocity, smoothTime);
    }

    public float GetMoveSpeed()
    {
        return _moveSpeed;
    }
    public void SetMoveSpeed(float speed)
    {
        _moveSpeed = speed;
    }
}