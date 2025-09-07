using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Здоровье")]
    [SerializeField] private float _health = 20;
    [SerializeField] private Sprite _dieSprite;

    private SpriteRenderer _sprite;
    private Animator _animator;
    private GameObject _children;
    private EnemyMove _enemyMove;
    private CircleCollider2D _circleCollider;

    private bool _isDead = false;

    [Header("Настройки смерти")]
    [SerializeField] private AudioClip _deathSound;

    private Sequence _deathSequence;

    private Tween _damageTween;
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _enemyMove = GetComponent<EnemyMove>();
        _circleCollider = GetComponent<CircleCollider2D>();
        _children = transform.GetChild(0).gameObject;
    }

    public void TakeDamage(float damage)
    {
        _health -= damage;
        Debug.Log(_health);

        // Анимация получения урона
        DamageFlash();

        if (_health <= 0 )
        {
            Die();
        }
    }
    private void DamageFlash()
    {
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
        if (_isDead) return;
        _isDead = true;

        // Отключаем компоненты
        _enemyMove.enabled = false;
        _circleCollider.enabled = false;
        _animator.enabled = false;
        _children.SetActive(false);

        // Запускаем анимацию смерти
        PlayDeathAnimation();

        // Меняем спрайт
        _sprite.sprite = _dieSprite;

    }
    private void PlayDeathAnimation()
    {
        // Воспроизводим звук
        if (_deathSound != null)
        {
            AudioSource.PlayClipAtPoint(_deathSound, transform.position);
        }    
    }

    private void OnDestroy()
    {
        // Очищаем твины при уничтожении
        if (_deathSequence != null && _deathSequence.IsActive())
        {
            _deathSequence.Kill();
        }
    }
}
