using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundWeapon : MonoBehaviour
{
    [Header("Основные настройки")]
    public float rotationRadius = 2f; // Радиус вращения вокруг игрока
    public float positionSmoothness = 10f; // Плавность перемещения
    public float rotationSmoothness = 10f; // Плавность поворота

    [Header("Дополнительные настройки")]
    public bool flipSprite = true; // Отражать спрайт при повороте

    private Transform _player;
    private Vector2 _mousePosition;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _targetLocalPosition;
    private Quaternion _targetRotation;

    private void Start()
    {
        _player = transform.parent;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _targetLocalPosition = Vector3.right * rotationRadius;
        _targetRotation = Quaternion.identity;
    }

    private void Update()
    {
        if (_player == null) return;

        _mousePosition = GameUtils.GetMousePosition();

        // Вычисляем направление от игрока к курсору
        Vector2 direction = (_mousePosition - (Vector2)_player.position).normalized;

        // Вычисляем целевую позицию относительно игрока
        _targetLocalPosition = direction * rotationRadius;

        // Плавное перемещение
        transform.position = Vector3.Lerp(
            transform.position,
            _player.position + (Vector3)_targetLocalPosition,
            positionSmoothness * Time.deltaTime);

        // Вычисляем целевой поворот
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _targetRotation = Quaternion.Euler(0f, 0f, angle);

        // Плавный поворот
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            _targetRotation,
            rotationSmoothness * Time.deltaTime);

        // Отражаем спрайт если нужно
        HandleSpriteFlipAlternative(direction);
    }

    // Альтернативный метод без отражения scale (рекомендуется)
    private void HandleSpriteFlipAlternative(Vector2 direction)
    {
        if (_spriteRenderer != null && flipSprite)
        {
            // Используем только flipY для вертикального отражения если нужно
            // или меняем порядок сортировки вместо масштаба
            _spriteRenderer.flipY = direction.x < 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_player.position, rotationRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(_player.position, transform.position);

            // Отображаем точку выстрела
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
    }
}