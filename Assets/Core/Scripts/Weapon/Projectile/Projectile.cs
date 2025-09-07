using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 _direction;
    private float _speed;
    private float _damage;
    private float _destroyTime;
    [SerializeField] private ProjectileAttack _projectile;
    public enum ProjectileAttack
    {
        Enemy,
        Player
    }
    public void Initialize(Vector2 direction, float speed, float damage, float destroyTime)
    {
        _direction = direction.normalized;
        _speed = speed;
        _damage = damage;
        _destroyTime = destroyTime;

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
        if (_projectile == ProjectileAttack.Enemy)
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyHealth health = other.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.TakeDamage(_damage);
                    Destroy(gameObject);
                }
            }
        }
        else if (_projectile == ProjectileAttack.Player)
        {
            if (other.CompareTag("Player"))
            {
                PlayerHealth health = other.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(_damage);
                }
                Destroy(gameObject);
            }
        }
        if (other.CompareTag("Wall"))
        {
            // Уничтожаем при столкновении со стеной
            Destroy(gameObject);
        }
    }
}