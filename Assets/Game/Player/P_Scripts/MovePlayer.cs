using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    [Header("Настройка перемещения игрока")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _deceleration;

    private Animator _animator;
    private Rigidbody2D _rb;
    private Vector2 _smoothVelocity;
    private Vector2 _moveInput;

   
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }
    private void Update()
    {
        _moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

        AnimationRun();
    }


    private void AnimationRun()
    {
        _animator.SetFloat("isRun", _moveInput.magnitude);
    }
    private void FixedUpdate()
    {
        Vector2 velocity = _moveInput * _moveSpeed;

        float smoothTime = _moveInput.magnitude > 0.1f ?
            1f / _acceleration :
            1f / _deceleration;

        _rb.velocity = Vector2.SmoothDamp(
            _rb.velocity, velocity, ref _smoothVelocity, smoothTime);
    }
}


