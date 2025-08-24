using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 _direction;
    private float _speed;
    private float _damage;

    public void Initialize(Vector2 direction, float speed, float damage)
    {
        _direction = direction.normalized;
        _speed = speed;
        _damage = damage;

        // Поворачиваем снаряд в направлении движения
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Уничтожаем через время
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        // Движение снаряда
        transform.Translate(_direction * _speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Наносим урон врагу
            //Health health = other.GetComponent<Health>();
            //if (health != null)
            //{
            //    health.TakeDamage(_damage);
            //}

            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            // Уничтожаем при столкновении со стеной
            Destroy(gameObject);
        }
    }
}