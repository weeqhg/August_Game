using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRoundWeapon : MonoBehaviour
{
    [Header("Основные настройки")]
    public float rotationRadius = 2f; // Радиус вращения вокруг врага
    public float positionSmoothness = 10f; // Плавность перемещения
    public float rotationSmoothness = 10f; // Плавность поворота

    [Header("Режим патрулирования")]
    public float minRandomChangeTime = 1f; // Минимальное время до смены позиции
    public float maxRandomChangeTime = 3f; // Максимальное время до смены позиции
    public float randomAngleRange = 45f; // Диапазон случайного угла

    [Header("Режим преследования")]
    public float attackRotationSpeed = 15f; // Скорость поворота при атаке

    [Header("Дополнительные настройки")]
    public bool flipSprite = true; // Отражать спрайт при повороте

    private Transform _enemy;
    private Transform _player;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _targetLocalPosition;
    private Quaternion _targetRotation;

    // Режимы работы
    private bool _isChasing = false;
    private float _randomChangeTimer = 0f;
    private float _nextChangeTime;
    private float _currentBaseAngle = 0f;

    private void Start()
    {
        _enemy = transform.parent;
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();


        _targetLocalPosition = Vector3.right * rotationRadius;
        _targetRotation = Quaternion.identity;

        // Устанавливаем первое время смены позиции
        _nextChangeTime = Random.Range(minRandomChangeTime, maxRandomChangeTime);
    }

    public void SetChasingState(bool isChasing, Transform player = null)
    {
        _isChasing = isChasing;

        if (player != null)
        {
            _player = player;
        }

        if (!_isChasing)
        {
            // При возврате к патрулированию выбираем случайную позицию
            SetRandomTargetPosition();
        }
    }

    private void Update()
    {
        if (_enemy == null) return;

        if (_isChasing && _player != null)
        {
            HandleChaseMode();
        }
        else
        {
            HandlePatrolMode();
        }

        // Применяем вычисленные позиции и повороты
        ApplyMovementAndRotation();
    }

    private void HandlePatrolMode()
    {
        // Таймер для смены случайной позиции
        _randomChangeTimer += Time.deltaTime;

        if (_randomChangeTimer >= _nextChangeTime)
        {
            SetRandomTargetPosition();
            _randomChangeTimer = 0f;
            _nextChangeTime = Random.Range(minRandomChangeTime, maxRandomChangeTime);
        }
    }

    private void SetRandomTargetPosition()
    {
        // Случайное изменение угла относительно текущего положения
        float randomAngleChange = Random.Range(-randomAngleRange, randomAngleRange);
        _currentBaseAngle += randomAngleChange;

        // Ограничиваем угол от 0 до 360 градусов
        _currentBaseAngle = Mathf.Repeat(_currentBaseAngle, 360f);

        // Вычисляем целевую позицию
        float angleRad = _currentBaseAngle * Mathf.Deg2Rad;
        _targetLocalPosition = new Vector3(
            Mathf.Cos(angleRad) * rotationRadius,
            Mathf.Sin(angleRad) * rotationRadius,
            0f
        );

        // Вычисляем целевой поворот
        _targetRotation = Quaternion.Euler(0f, 0f, _currentBaseAngle);
    }

    private void HandleChaseMode()
    {
        if (_player == null) return;


        // Вычисляем направление к игроку
        Vector2 directionToPlayer = (_player.position - _enemy.position).normalized;

        // Целевая позиция - направление на игрока
        _targetLocalPosition = directionToPlayer * rotationRadius;

        // Вычисляем угол поворота к игроку
        float targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        _targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        // Обновляем базовый угол для плавного перехода
        _currentBaseAngle = Mathf.LerpAngle(_currentBaseAngle, targetAngle,
            attackRotationSpeed * Time.deltaTime);
    }

    private void ApplyMovementAndRotation()
    {
        // Плавное перемещение к целевой позиции
        transform.position = Vector3.Lerp(
            transform.position,
            _enemy.position + _targetLocalPosition,
            positionSmoothness * Time.deltaTime
        );

        // Плавный поворот
        float smoothness = _isChasing ? attackRotationSpeed : rotationSmoothness;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            _targetRotation,
            smoothness * Time.deltaTime
        );

        // Обработка отражения спрайта
        HandleSpriteFlip();
    }

    private void HandleSpriteFlip()
    {
        if (_spriteRenderer != null && flipSprite)
        {
            // Определяем направление взгляда
            Vector3 direction = _targetLocalPosition.normalized;

            // Отражаем по Y если смотрим влево
            _spriteRenderer.flipY = direction.x < 0;

            // Дополнительно можно использовать flipX если нужно
            //_spriteRenderer.flipX = direction.x > 0;
        }
    }

    // Метод для получения направления оружия (можно использовать для выстрела)
    public Vector2 GetWeaponDirection()
    {
        return transform.right; // Направление, куда "смотрит" оружие
    }

    // Метод для получения точки выстрела
    public Vector2 GetFirePosition()
    {
        return transform.position;
    }

    // Метод для принудительной установки цели
    public void ForceAimAtTarget(Vector3 targetPosition)
    {
        Vector2 direction = (targetPosition - _enemy.position).normalized;
        _targetLocalPosition = direction * rotationRadius;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _targetRotation = Quaternion.Euler(0f, 0f, angle);
        _currentBaseAngle = angle;
    }

    private void OnDrawGizmosSelected()
    {
        if (_enemy != null)
        {
            // Радиус вращения
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_enemy.position, rotationRadius);

            // Текущее направление
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_enemy.position, transform.position);

            // Направление атаки
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.right * 1f);

            // Целевая позиция (в режиме редактора)
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(_enemy.position + _targetLocalPosition, 0.1f);
            }
        }
    }

    // Для отладки в инспекторе
    public void DebugSetChasing(bool chasing)
    {
        SetChasingState(chasing);
    }

    public void DebugSetRandomPosition()
    {
        SetRandomTargetPosition();
    }
}