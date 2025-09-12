using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.Tilemaps;


public class Spawn : MonoBehaviour
{
    [Header("Настройка появление игрока")]
    [SerializeField] private CinemachineVirtualCamera _cm;
    [SerializeField] private AccessoryBarUI _barUI;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _spawnAnimationDuration = 1f;

    [Header("Безопасная зона для игрока")]
    [SerializeField] private int _maxSpawnAttemptsPlayer = 30;
    [SerializeField] private Vector2 _spawnAreaSizePlayer = new Vector2(5f, 5f);

    [Header("Настройка появления мобов")]
    [SerializeField] private GameObject[] _defaultEnemy;
    [SerializeField] private GameObject[] _eliteEnemy;
    [SerializeField] private int _enemiesToSpawn;
    [SerializeField] private float _minDistanceFromPlayer = 5f;

    [Header("Безопасная зона для мобов")]
    [SerializeField] private int _maxSpawnAttemptsEnemy = 30;
    [SerializeField] private Vector2 _spawnAreaSizeEnemy = new Vector2(5f, 5f);

    [Header("Настройка для интерактивных объектов")]
    [SerializeField] private GameObject[] _objectInteract;
    [SerializeField] private int _countItemInteract;

    [Header("Безопасная зона для объектов")]
    [SerializeField] private int _maxSpawnAttemptsItem = 30;
    [SerializeField] private Vector2 _spawnAreaSizeItem = new Vector2(5f, 5f);



    [SerializeField] private Tilemap _tileMapFloor;
    private GameObject _playerInstance;


    private void Awake()
    {
        GameManager.Instance.Register(this);
    }

    public void SpawnOnWorld()
    {
        SpawnPlayer();
        SpawnEnemies();
        SpawnItemsInteract();
        GetNeedComponent();
    }
    public void SpawnPlayer()
    {
        StartCoroutine(SpawnPlayerCoroutine());
    }
    public void SpawnEnemies()
    {
        for (int i = 0; i < _enemiesToSpawn; i++)
        {
            SpawnSingleEnemy();
        }
    }
    public void SpawnItemsInteract()
    {
        for (int i = 0; i < _countItemInteract; i++)
        {
            SpawnItems();
        }
    }

    private void GetNeedComponent()
    {
        _cm.Follow = _playerTransform;
        _barUI.Initialize(_playerInstance.GetComponentInChildren<PlayerAccessoryWeapon>());
    }

    private void SpawnItems()
    {
        if (_objectInteract.Length == 0) return;

        Vector2 spawnPosition = FindSafeSpawnPosition2DItems();

        if (spawnPosition == Vector2.zero)
        {
            Debug.LogWarning("Не удалось найти безопасную позицию для появления врага");
            return;
        }

        GameObject itemInteract = _objectInteract[Random.Range(0, _objectInteract.Length)];
        Instantiate(itemInteract, spawnPosition, Quaternion.identity);
    }
    private void SpawnSingleEnemy()
    {
        if (_playerInstance == null) return;

        Vector2 spawnPosition = FindSafeEnemySpawnPosition();

        if (spawnPosition == Vector2.zero)
        {
            Debug.LogWarning("Не удалось найти безопасную позицию для появления врага");
            return;
        }

        // Выбираем случайного врага из массива
        GameObject enemyPrefab = _defaultEnemy[Random.Range(0, _defaultEnemy.Length)];
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }

    private IEnumerator SpawnPlayerCoroutine()
    {
        Vector2 spawnPosition = FindSafeSpawnPosition2D();

        if (spawnPosition == Vector2.zero)
        {
            Debug.LogError("No safe spawn position found in 2D!");
            yield break;
        }

        Vector2 startPosition = spawnPosition;
        _playerInstance = Instantiate(_playerPrefab, startPosition, Quaternion.identity);

        SetPlayerComponentsEnabled(false);

        _playerTransform = _playerInstance.transform;

        Sequence spawnSequence = DOTween.Sequence();


        spawnSequence.Join(_playerTransform.DORotate(new Vector3(0f, 0f, 360f), _spawnAnimationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic));

        _playerTransform.localScale = Vector3.zero;
        spawnSequence.Join(_playerTransform.DOScale(Vector3.one, _spawnAnimationDuration * 0.7f)
            .SetEase(Ease.OutBack));

        spawnSequence.Append(_playerTransform.DOShakeScale(0.3f, 0.2f, 10, 90f));

        yield return spawnSequence.WaitForCompletion();

        SetPlayerComponentsEnabled(true);
    }
    private Vector2 FindSafeSpawnPosition2DItems()
    {
        Vector2 center = transform.position;

        for (int i = 0; i < _maxSpawnAttemptsItem; i++)
        {
            Vector2 randomPoint = center + new Vector2(
                Random.Range(-_spawnAreaSizeItem.x / 2f, _spawnAreaSizeItem.x / 2f),
                Random.Range(-_spawnAreaSizeItem.y / 2f, _spawnAreaSizeItem.y / 2f)
            );

            if (CanSpawnOnTile(randomPoint))
            {
                return randomPoint;
            }
        }

        return center;
    }

    private Vector2 FindSafeSpawnPosition2D()
    {
        Vector2 center = transform.position;

        for (int i = 0; i < _maxSpawnAttemptsPlayer; i++)
        {
            Vector2 randomPoint = center + new Vector2(
                Random.Range(-_spawnAreaSizePlayer.x / 2f, _spawnAreaSizePlayer.x / 2f),
                Random.Range(-_spawnAreaSizePlayer.y / 2f, _spawnAreaSizePlayer.y / 2f)
            );

            if (CanSpawnOnTile(randomPoint))
            {
                return randomPoint;
            }
        }

        return center;
    }

    private Vector2 FindSafeEnemySpawnPosition()
    {
        Vector2 center = transform.position;
        Vector2 playerPosition = _playerInstance.transform.position;

        for (int i = 0; i < _maxSpawnAttemptsEnemy; i++)
        {
            Vector2 randomPoint = center + new Vector2(
                Random.Range(-_spawnAreaSizeEnemy.x / 2f, _spawnAreaSizeEnemy.x / 2f),
                Random.Range(-_spawnAreaSizeEnemy.y / 2f, _spawnAreaSizeEnemy.y / 2f)
            );

            // Проверяем, что точка достаточно далеко от игрока
            float distanceToPlayer = Vector2.Distance(randomPoint, playerPosition);

            if (distanceToPlayer >= _minDistanceFromPlayer && CanSpawnOnTile(randomPoint))
            {
                return randomPoint;
            }
        }

        // Если не нашли идеальную позицию, попробуем найти любую доступную
        for (int i = 0; i < _maxSpawnAttemptsEnemy; i++)
        {
            Vector2 randomPoint = center + new Vector2(
                Random.Range(-_spawnAreaSizeEnemy.x / 2f, _spawnAreaSizeEnemy.x / 2f),
                Random.Range(-_spawnAreaSizeEnemy.y / 2f, _spawnAreaSizeEnemy.y / 2f)
            );

            if (CanSpawnOnTile(randomPoint))
            {
                return randomPoint;
            }
        }

        return Vector2.zero;
    }

    private bool CanSpawnOnTile(Vector2 position)
    {
        if (_tileMapFloor == null) return false;

        Vector3Int cellPosition = _tileMapFloor.WorldToCell(position);
        return _tileMapFloor.HasTile(cellPosition);
    }


    private void SetPlayerComponentsEnabled(bool enabled)
    {
        if (_playerInstance == null) return;
        var movement = _playerInstance.GetComponent<MovePlayer>();

        if (movement != null) movement.enabled = enabled;
        var rb2D = _playerInstance.GetComponent<Rigidbody2D>();


        if (rb2D != null)
        {
            rb2D.simulated = enabled;
            if (enabled) rb2D.velocity = Vector2.zero;
        }

        var collider2D = _playerInstance.GetComponent<Collider2D>();
        if (collider2D != null) collider2D.enabled = enabled;

        var playerWeapon = _playerInstance.GetComponentInChildren<Weapon>();
        if (playerWeapon != null) playerWeapon.enabled = enabled;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector2(_spawnAreaSizePlayer.x, _spawnAreaSizePlayer.y));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector2(_spawnAreaSizeEnemy.x, _spawnAreaSizeEnemy.y));

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector2(_spawnAreaSizeItem.x, _spawnAreaSizeItem.y));

        // Зона безопасности вокруг игрока (если игрок существует)
        if (_playerInstance != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_playerInstance.transform.position, _minDistanceFromPlayer);
        }
    }
}
