using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    [Header("Настройка перемещения игрока")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _deceleration;


    private DogePlayer _dogeP;
    private Animator _animator;
    private Rigidbody2D _rb;
    private Vector2 _smoothVelocity;
    private Vector2 _moveInput;


    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _dogeP = GetComponent<DogePlayer>();
        _rb.gravityScale = 0;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }
    private void Update()
    {
        if (!_dogeP.isDodging)
            _moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

        _dogeP.HandleDodgeInput(_moveInput);
        _dogeP.UpdateTimers();
        AnimationRun();
    }
    
 

    private void AnimationRun()
    {
        _animator.SetFloat("isRun", _moveInput.magnitude);
    }
    private void FixedUpdate()
    {
        if (_dogeP.isDodging)
        {
            _dogeP.DogeMove();
        }
        else
        {
            Vector2 velocity = _moveInput * _moveSpeed;

            float smoothTime = _moveInput.magnitude > 0.1f ?
                1f / _acceleration :
                1f / _deceleration;

            _rb.velocity = Vector2.SmoothDamp(
                _rb.velocity, velocity, ref _smoothVelocity, smoothTime);
        }
    }
}


