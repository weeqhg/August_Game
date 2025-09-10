using UnityEngine;
public enum DamageType
{
    Normal,
    Fire,       // Наносит доп урон горением
    Ice,        // Замедляет врагов
    Poison,     // Наносит урон со временем
    Lightning,  // Цепная молния
    Explosive   // Взрывной урон
}
public class Projectile : MonoBehaviour
{
    private Vector2 _direction;
    private float _speed;
    private float _damage;
    private float _destroyTime;
    private string _nameAttack;
    private SpriteRenderer _spriteRenderer;
    private DamageType _damageType;
    public void Initialize(Vector2 direction, float speed, float damage, float destroyTime, string nameAttack, Color color, DamageType damageType)
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _direction = direction.normalized;
        _speed = speed;
        _damage = damage;
        _destroyTime = destroyTime;
        _nameAttack = nameAttack;
        _spriteRenderer.color = color;
        _damageType = damageType;

        // Поворачиваем снаряд в направлении движения
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Уничтожаем через время
        Destroy(gameObject, _destroyTime);
    }

    private void Update()
    {
        // Движение снаряда
        transform.Translate(_direction * _speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(_nameAttack))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(_damage, _damageType);
                Destroy(gameObject);
            }
        }

        if (other.CompareTag("Wall"))
        {
            // Уничтожаем при столкновении со стеной
            Destroy(gameObject);
        }
    }

    private void Attack()
    {

    }
}