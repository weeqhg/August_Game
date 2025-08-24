using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundWeapon : MonoBehaviour
{
    [Header("Основные настройки")]
    public float rotationRadius = 2f; // Радиус вращения вокруг игрока
    public float positionSmoothness = 10f; // Плавность перемещения

    [Header("Дополнительные настройки")]
    public bool flipSprite = true; // Отражать спрайт при повороте

    private Transform _player;
    private Vector2 _mousePosition;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _targetLocalPosition;



    private void Start()
    {
        _player = transform.parent;
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _targetLocalPosition = Vector3.right * rotationRadius;
    }

    private void Update()
    {
        if (_player == null) return;

        _mousePosition = GameUtils.GetMousePosition();

        // Вычисляем направление от игрока к курсору
        Vector2 direction = (_mousePosition - (Vector2)_player.position).normalized;

        // Вычисляем целевую позицию относительно игрока
        _targetLocalPosition = direction * rotationRadius;




        transform.position = Vector3.Lerp(
            transform.position,
            _player.position + (Vector3)_targetLocalPosition,
            positionSmoothness * Time.deltaTime);



        // Поворачиваем объект в сторону курсора
        RotateTowardsMouse(direction);

        // Отражаем спрайт если нужно
        HandleSpriteFlip(direction);
    }

    private void RotateTowardsMouse(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (flipSprite && direction.x < 0)
        {
            angle += 180f;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void HandleSpriteFlip(Vector2 direction)
    {
        if (_spriteRenderer != null && flipSprite)
        {
            if (direction.x > 0.1f)
                transform.localScale = new Vector3(1, 1, 1);
            else
                transform.localScale = new Vector3(-1, 1, 1);
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