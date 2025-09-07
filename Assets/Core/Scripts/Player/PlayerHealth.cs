using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Здоровье")]
    [SerializeField] private float _health = 40;
    [SerializeField] private Sprite _die;

    private SpriteRenderer _sprite;
    private Animator _animator;
    private GameObject _children;
    private MovePlayer _movePlayer;
    private DashPlayer _dashPlayer;
    private PlayerView _playerView;
    private Rigidbody2D _rb;
    public bool isDie { get; private set; } = false;

    [Header("Настройки смерти")]
    [SerializeField] private AudioClip _deathSound;

    private Tween _damageTween;
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _movePlayer = GetComponent<MovePlayer>();
        _dashPlayer = GetComponent<DashPlayer>();
        _rb = GetComponent<Rigidbody2D>();
        _playerView = GetComponent<PlayerView>();

        _children = transform.GetChild(0).gameObject;
    }

    public void TakeDamage(float damage)
    {
        _health -= damage;
        Debug.Log("Здоровье игрока: " + _health);

        DamageFlash();

        if (_health <= 0)
        {
            Die();
        }
    }
    private void DamageFlash()
    {
        if (_dashPlayer.IsDashing) return;

        // Останавливаем предыдущую анимацию урона
        if (_damageTween != null && _damageTween.IsActive())
        {
            _damageTween.Kill();
            _sprite.color = Color.white;
        }

        // Мигание при получении урона
        _damageTween = _sprite.DOColor(Color.red, 0.1f)
               .SetLoops(2, LoopType.Yoyo)
               .SetEase(Ease.Flash);
    }
    private void Die()
    {
        _animator.enabled = false;
        _movePlayer.enabled = false;
        _dashPlayer.enabled = false;
        _playerView.enabled = false;
        _rb.isKinematic = true;
        _rb.velocity = Vector3.zero;
        _children.SetActive(false);

        PlayDeathAnimation();

        _sprite.sprite = _die;
    }
    private void PlayDeathAnimation()
    {
        // Воспроизводим звук
        if (_deathSound != null)
        {
            AudioSource.PlayClipAtPoint(_deathSound, transform.position);
        }
    }

}
