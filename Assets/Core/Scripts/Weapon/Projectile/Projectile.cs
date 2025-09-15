
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

    private float _criticalMultiplier;
    private float _criticalChance;
    private bool _canCritical;
    private bool _critical;

    private string _nameAttack;
    private SpriteRenderer _spriteRenderer;
    private DamageType _damageType;
    public void Initialize(Vector2 direction, DamageType damageType, WeaponConfig weaponConfig)
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _direction = direction.normalized;
        _speed = weaponConfig.projectileSpeed;
        _damage = weaponConfig.damage;
        _destroyTime = weaponConfig.destroyTime;
        _nameAttack = weaponConfig.nameAttack;
        _spriteRenderer.color = weaponConfig.colorProjectile;
        _damageType = damageType;
        _criticalMultiplier = weaponConfig.criticalMultiplier;
        _criticalChance = weaponConfig.criticalChance;
        _canCritical = weaponConfig.canCritical;

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
            Health damageable = other.GetComponent<Health>();
            if (damageable != null)
            {
                // Рассчитываем урон и получаем информацию о крите
                (float finalDamage, bool isCritical) = CalculateDamageWithCrit(_damage);

                // Передаем оба параметра
                damageable.TakeDamage(finalDamage, _damageType, isCritical);
                Destroy(gameObject);
            }
        }

        if (other.CompareTag("Wall"))
        {
            // Уничтожаем при столкновении со стеной
            Destroy(gameObject);
        }
    }

    // Возвращаем кортеж с уроном и флагом крита
    public (float damage, bool isCritical) CalculateDamageWithCrit(float baseDamage)
    {
        float damage = baseDamage;
        bool isCritical = false;

        if (_canCritical && IsCriticalHit())
        {
            damage = Mathf.RoundToInt(damage * _criticalMultiplier);
            isCritical = true;
            Debug.Log("Критический удар!");
        }

        return (damage, isCritical);
    }

    private bool IsCriticalHit()
    {
        return Random.value <= _criticalChance;
    }
}