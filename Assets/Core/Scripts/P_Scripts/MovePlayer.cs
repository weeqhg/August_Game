using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    [Header("Настройка перемещения игрока")]
    [SerializeField] private float _moveSpeed;
    private float _acceleration = 20f;
    private float _deceleration = 300f;

    private Animator _animator;
    private Rigidbody2D _rb;
    private Vector2 _smoothVelocity;
    private Vector2 _moveInput;
    private DashPlayer _dashPlayer;

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
        // Не обновляем ввод во время рывка
        if (_dashPlayer != null && _dashPlayer.IsDashing)
            return;

        _moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        AnimationRun();
    }

    private void AnimationRun()
    {
        _animator.SetFloat("isRun", _moveInput.magnitude);
    }

    private void FixedUpdate()
    {
        // Не двигаем во время рывка
        if (_dashPlayer != null && _dashPlayer.IsDashing)
            return;

        Vector2 velocity = _moveInput * _moveSpeed;
        float smoothTime = _moveInput.magnitude > 0.1f ? 1f / _acceleration : 1f / _deceleration;

        _rb.velocity = Vector2.SmoothDamp(_rb.velocity, velocity, ref _smoothVelocity, smoothTime);
    }
}