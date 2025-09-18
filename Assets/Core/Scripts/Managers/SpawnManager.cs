using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.Tilemaps;


public class Spawn : MonoBehaviour
{
    [Header("Необходимые ссылки")]
    [SerializeField] private LevelManager _levelManager;
    [SerializeField] private AccessoryBarUI _barUI;
    [SerializeField] private HealthBar_UI _healthBarUI;
    [SerializeField] private DashBar_UI _dashBarUI;
    [SerializeField] private KeyBar_UI _keyBarUI;

    [Header("Настройка появление игрока")]
    [SerializeField] private CinemachineVirtualCamera _cm;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _spawnAnimationDuration = 1f;

    [Header("Безопасная зона для игрока")]
    [SerializeField] private int _maxSpawnAttemptsPlayer = 30;
    [SerializeField] private Vector2 _spawnAreaSizePlayer = new Vector2(5f, 5f);

    [Header("Настройка появления мобов")]
    [SerializeField, HideInInspector] private GameObject[] _defaultEnemy;
    [SerializeField, HideInInspector] private GameObject[] _eliteEnemy;
    private int _enemiesToSpawn;
    [SerializeField] private float _minDistanceFromPlayer = 5f;

    [Header("Безопасная зона для мобов")]
    [SerializeField] private int _maxSpawnAttemptsEnemy = 30;
    [SerializeField] private Vector2 _spawnAreaSizeEnemy = new Vector2(5f, 5f);

    [Header("Настройка для интерактивных объектов")]
    [SerializeField] private GameObject[] _objectInteract;
    [SerializeField, HideInInspector] private int _countItemInteract;
    [SerializeField] private GameObject _portalNextLevel;

    [Header("Безопасная зона для объектов")]
    [SerializeField] private int _maxSpawnAttemptsItem = 30;
    [SerializeField] private Vector2 _spawnAreaSizeItem = new Vector2(5f, 5f);



    [SerializeField] private Tilemap _tileMapFloor;
    private GameObject _playerInstance;
    private List<GameObject> _enemiesSpawn = new List<GameObject>();
    private List<GameObject> _itemsSpawn = new List<GameObject>();
    private List<GameObject> _othersItemSpawn = new List<GameObject>();
    private List<GameObject> _keyItems = new List<GameObject>();

    private void Awake()
    {
        GameManager.Instance.Register(this);
    }

    public void SettingSpawnEnemy()
    {
        var (enemy, countEnemy, countItem) = _levelManager.RandomSetting();
        _defaultEnemy = enemy;
        _enemiesToSpawn = countEnemy;
        _countItemInteract = countItem;
    }


    public void StartSpawn()
    {
        StartCoroutine(ClearLevelCoroutine());
    }
    public void SpawnOnWorld()
    {
        SettingSpawnEnemy();
        SpawnPlayer();
        SpawnItemsInteract();
        SpawnEnemies();
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
        _levelManager.GetCountEnemy(_enemiesToSpawn);
    }
    public void SpawnItemsInteract()
    {
        for (int i = 0; i < _countItemInteract; i++)
        {
            SpawnItems();
        }
    }
    public void SpawnPortalNextLevel(int level)
    {
        if (_portalNextLevel == null) return;

        Vector2 spawnPosition = FindSafePortalPositionNearPlayer();

        if (spawnPosition == Vector2.zero)
        {
            Debug.LogWarning("Не удалось найти безопасную позицию для появления портала");
            return;
        }

        GameObject portal = Instantiate(_portalNextLevel, spawnPosition, Quaternion.identity);
        PortalNextLevel portalNextLevel = portal.GetComponent<PortalNextLevel>();
        portalNextLevel.Initialize(level);
    }

    private void GetNeedComponent()
    {
        _cm.Follow = _playerTransform;
        _barUI.Initialize(_playerInstance.GetComponentInChildren<PlayerAccessoryWeapon>());
        _healthBarUI.Initialize(_playerInstance.GetComponent<PlayerHealth>());
        _dashBarUI.Initialize(_playerInstance.GetComponent<DashSystem>());
        _keyBarUI.Initialize(_playerInstance.GetComponent<PlayerKey>());
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
        GameObject item = Instantiate(itemInteract, spawnPosition, Quaternion.identity);
        _itemsSpawn.Add(item);
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
        EnemyHealth enemyHealth = enemyPrefab.GetComponent<EnemyHealth>();
        EnemySetting enemySetting = enemyPrefab.GetComponent<EnemySetting>();
        enemyHealth.GetLevelManager(_levelManager);
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemyHealth.Initialize(this);
        _enemiesSpawn.Add(enemy);
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
        _playerTransform = _playerInstance.transform;
        SetPlayerComponentsEnabled(false);
    }

    public async void AnimationPlayerSpawn()
    {

        Sequence spawnSequence = DOTween.Sequence();


        spawnSequence.Join(_playerTransform.DORotate(new Vector3(0f, 0f, 360f), _spawnAnimationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic));

        _playerTransform.localScale = Vector3.zero;
        spawnSequence.Join(_playerTransform.DOScale(Vector3.one, _spawnAnimationDuration * 0.7f)
            .SetEase(Ease.OutBack));

        spawnSequence.Append(_playerTransform.DOShakeScale(0.3f, 0.2f, 10, 90f));

        // Ожидаем завершение анимации
        await spawnSequence.AsyncWaitForCompletion();

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

    private Vector2 FindSafePortalPositionNearPlayer()
    {
        if (_playerInstance == null) return Vector2.zero;

        Vector2 playerPosition = _playerInstance.transform.position;
        float[] distances = { 1f, 2f, 3f, 4f, 5f }; // Приоритетные расстояния от игрока
        float[] angles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f }; // Направления

        // Сначала ищем на оптимальном расстоянии
        foreach (float distance in distances)
        {
            foreach (float angle in angles)
            {
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
                Vector2 candidatePosition = playerPosition + direction * distance;

                if (IsSafePortalPosition(candidatePosition, playerPosition))
                {
                    return candidatePosition;
                }
            }
        }

        // Если не нашли на оптимальном расстоянии, ищем ближе
        for (float distance = 0.1f; distance <= 8f; distance += 0.5f)
        {
            for (float angle = 0f; angle < 360f; angle += 45f)
            {
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
                Vector2 candidatePosition = playerPosition + direction * distance;

                if (IsSafePortalPosition(candidatePosition, playerPosition))
                {
                    return candidatePosition;
                }
            }
        }

        return Vector2.zero;
    }

    private bool IsSafePortalPosition(Vector2 position, Vector2 playerPosition)
    {
        // Проверяем что позиция на тайле
        if (!CanSpawnOnTile(position)) return false;

        // Проверяем минимальное расстояние от игрока (не слишком близко)
        float distanceToPlayer = Vector2.Distance(position, playerPosition);
        if (distanceToPlayer < 0.2f) return false;

        // Проверяем коллизии с другими объектами
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 1f);
        foreach (var collider in colliders)
        {
            // Игнорируем триггеры и самого игрока
            if (collider.isTrigger) continue;
            if (collider.gameObject == _playerInstance) continue;

            // Если есть твердый коллайдер - позиция не безопасна
            if (!collider.isTrigger)
            {
                return false;
            }
        }

        return true;
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



    private IEnumerator ClearLevelCoroutine()
    {
        // 1. Уничтожаем врагов
        for (int i = _enemiesSpawn.Count - 1; i >= 0; i--)
        {
            if (_enemiesSpawn[i] != null)
            {
                Destroy(_enemiesSpawn[i]);
            }
        }
        _enemiesSpawn.Clear();

        // Ждем завершения кадра
        yield return null;

        // 2. Уничтожаем предметы
        for (int i = _itemsSpawn.Count - 1; i >= 0; i--)
        {
            if (_itemsSpawn[i] != null)
            {
                Destroy(_itemsSpawn[i]);
            }
        }
        _itemsSpawn.Clear();

        yield return null;

        if (_playerInstance != null)
        {
            PlayerAccessoryWeapon playerAccessoryWeapon = _playerInstance.GetComponentInChildren<PlayerAccessoryWeapon>();
            _othersItemSpawn = playerAccessoryWeapon.listInteractItems;
        }

        // 3. Уничтожаем предметы
        for (int i = _othersItemSpawn.Count - 1; i >= 0; i--)
        {
            if (_othersItemSpawn[i] != null)
            {
                Destroy(_othersItemSpawn[i]);
            }
        }
        _othersItemSpawn.Clear();

        yield return null;

        // 4. Уничтожаем ключи
        for (int i = _keyItems.Count - 1; i >= 0; i--)
        {
            if (_keyItems[i] != null)
            {
                Destroy(_keyItems[i]);
            }
        }
        _keyItems.Clear();

        yield return null;

        // 4. Уничтожаем игрока
        if (_playerInstance != null)
        {
            SavePlayerData();
            Destroy(_playerInstance);
            _playerInstance = null;
        }

        yield return null;

        SpawnOnWorld();
    }

    public void AddDropKey(GameObject gameObject)
    {
        if (gameObject != null)
            _keyItems.Add(gameObject);
    }

    private void SavePlayerData()
    {
        PlayerHealth playerHealth = _playerInstance.GetComponent<PlayerHealth>();
        PlayerWeapon playerWeapon = _playerInstance.GetComponentInChildren<PlayerWeapon>();
        PlayerKey playerKey = _playerInstance.GetComponent<PlayerKey>();

        playerKey.SaveGameData();
        playerHealth.SaveGameData();
        playerWeapon.SaveGameData();
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
