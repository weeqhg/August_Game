using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CaveGenerator : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private LevelManager levelManager;

    [Header("Настройка пещеры")]
    [SerializeField] private int _width = 100;
    [SerializeField] private int _height = 100;
    [SerializeField] private int _seed = 0;
    [SerializeField][Range(0, 100)] private int _randomFillPercent = 45;

    [Header("Настройки сглаживания")]
    [SerializeField] private int _smoothingIterations = 5;
    [SerializeField] private int _smoothingThreshold = 4;
    [SerializeField] private int _borderWidth = 5;

    [Header("Настройки границ")]
    [SerializeField] private int _borderIrregularity = 3; 
    [SerializeField] private int _borderSmoothing = 2;

    [Header("Центральная зона")]
    [SerializeField] private int _centerClearRadius = 10; // Радиус чистой зоны в центре
    [SerializeField] private int _minPathWidth = 3; // Минимальная ширина проходов

    [Header("Биомы пещеры")]
    [SerializeField] private List<Biome> _biomes = new List<Biome>();

    [Header("Tilemaps")]
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _wallTilemap;
    [SerializeField] private Tilemap _shadowTilemap;
    [SerializeField] private Tilemap _decorTilemap;

    private Biome currentBiome;

    [System.Serializable]
    public class Biome
    {
        public string name;
        public TileBase[] groundTiles;
        public TileBase wallTile;
        public TileBase wallShadowTile;

        [Header("Настройки генерации")]
        [Range(0f, 1f)] public float minDepth = 0f;
        [Range(0f, 1f)] public float maxDepth = 1f;
        [Range(0f, 1f)] public float spawnChance = 0.1f;

        [Header("Декорации")]
        public TileBase[] decorations;
        [Range(0f, 1f)] public float decorationDensity = 0.05f;
    }

    private int[,] _map;
    private System.Random _prng;

    private void Awake()
    {
        GameManager.Instance.Register(this);
    }

    public void StartGenerate()
    {
        //Debug.Log("генерация");
        GetBiome(2);
        GenerateCave();
        AddWallShadows();
        AddDecorations();
    }

    public void GenerateCave()
    {
        _map = new int[_width, _height];

        // Генерация с гарантией проходимости
        do
        {
            RandomFillMap();

            for (int i = 0; i < _smoothingIterations; i++)
            {
                SmoothMap();
            }

            // Очищаем центр
            ClearCenterArea();

            // Улучшаем проходимость
            ImproveAccessibility();

        } while (!IsMapAccessible()); // Повторяем пока карта не станет проходимой

        AddIrregularBorders();
        SmoothBorders();
        RenderCave();
    }

    // Очистка центральной зоны
    private void ClearCenterArea()
    {
        int centerX = _width / 2;
        int centerY = _height / 2;

        for (int y = centerY - _centerClearRadius; y <= centerY + _centerClearRadius; y++)
        {
            for (int x = centerX - _centerClearRadius; x <= centerX + _centerClearRadius; x++)
            {
                if (x >= 0 && x < _width && y >= 0 && y < _height)
                {
                    // Проверяем расстояние до центра
                    float distance = Mathf.Sqrt(Mathf.Pow(x - centerX, 2) + Mathf.Pow(y - centerY, 2));
                    if (distance <= _centerClearRadius)
                    {
                        _map[x, y] = 0; // Убираем стены в центре
                    }
                }
            }
        }
    }

    // Улучшение проходимости карты
    private void ImproveAccessibility()
    {
        // Убираем изолированные стены и полы
        RemoveIsolatedWalls();
        ConnectIsolatedAreas();
        WidenPaths();
    }

    // Удаление изолированных стен (одиночных стен посреди пола)
    private void RemoveIsolatedWalls()
    {
        for (int y = 1; y < _height - 1; y++)
        {
            for (int x = 1; x < _width - 1; x++)
            {
                if (_map[x, y] == 1) // Если это стена
                {
                    int floorNeighbors = 0;
                    for (int ny = y - 1; ny <= y + 1; ny++)
                    {
                        for (int nx = x - 1; nx <= x + 1; nx++)
                        {
                            if (nx >= 0 && nx < _width && ny >= 0 && ny < _height)
                            {
                                if (_map[nx, ny] == 0) floorNeighbors++;
                            }
                        }
                    }

                    // Если у стены слишком много соседей-полов, убираем её
                    if (floorNeighbors >= 6)
                    {
                        _map[x, y] = 0;
                    }
                }
            }
        }
    }

    // Соединение изолированных областей
    private void ConnectIsolatedAreas()
    {
        // Находим все отдельные области пола
        List<HashSet<Vector2Int>> areas = FindFloorAreas();

        if (areas.Count > 1)
        {
            // Соединяем области туннелями
            for (int i = 1; i < areas.Count; i++)
            {
                ConnectAreas(areas[0], areas[i]);
            }
        }
    }

    // Поиск отдельных областей пола
    private List<HashSet<Vector2Int>> FindFloorAreas()
    {
        List<HashSet<Vector2Int>> areas = new List<HashSet<Vector2Int>>();
        bool[,] visited = new bool[_width, _height];

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (_map[x, y] == 0 && !visited[x, y])
                {
                    HashSet<Vector2Int> area = new HashSet<Vector2Int>();
                    FloodFill(x, y, visited, area);
                    areas.Add(area);
                }
            }
        }

        return areas;
    }

    // Заливка для поиска связанных областей
    private void FloodFill(int startX, int startY, bool[,] visited, HashSet<Vector2Int> area)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            area.Add(current);

            // Проверяем соседей
            CheckNeighbor(current.x + 1, current.y, visited, queue);
            CheckNeighbor(current.x - 1, current.y, visited, queue);
            CheckNeighbor(current.x, current.y + 1, visited, queue);
            CheckNeighbor(current.x, current.y - 1, visited, queue);
        }
    }
    private void CheckNeighbor(int x, int y, bool[,] visited, Queue<Vector2Int> queue)
    {
        if (x >= 0 && x < _width && y >= 0 && y < _height &&
            _map[x, y] == 0 && !visited[x, y])
        {
            visited[x, y] = true;
            queue.Enqueue(new Vector2Int(x, y));
        }
    }

    // Соединение двух областей туннелем
    private void ConnectAreas(HashSet<Vector2Int> area1, HashSet<Vector2Int> area2)
    {
        // Находим ближайшие точки в двух областях
        Vector2Int point1 = FindClosestPoint(area1, area2);
        Vector2Int point2 = FindClosestPoint(area2, area1);

        // Прокладываем туннель между точками
        DigTunnel(point1, point2);
    }
    private Vector2Int FindClosestPoint(HashSet<Vector2Int> area, HashSet<Vector2Int> targetArea)
    {
        Vector2Int closest = new Vector2Int(0, 0);
        float minDistance = float.MaxValue;

        foreach (Vector2Int point in area)
        {
            foreach (Vector2Int targetPoint in targetArea)
            {
                float distance = Vector2Int.Distance(point, targetPoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = point;
                }
            }
        }

        return closest;
    }
    private void DigTunnel(Vector2Int start, Vector2Int end)
    {
        int x = start.x;
        int y = start.y;

        while (x != end.x || y != end.y)
        {
            _map[x, y] = 0; // Убираем стену

            if (x < end.x) x++;
            else if (x > end.x) x--;

            if (y < end.y) y++;
            else if (y > end.y) y--;

            // Делаем туннель шире
            for (int wx = x - 1; wx <= x + 1; wx++)
            {
                for (int wy = y - 1; wy <= y + 1; wy++)
                {
                    if (wx >= 0 && wx < _width && wy >= 0 && wy < _height)
                    {
                        if (Random.value < 0.3f) // Случайным образом расширяем туннель
                        {
                            _map[wx, wy] = 0;
                        }
                    }
                }
            }
        }
    }

    // Расширение узких проходов
    private void WidenPaths()
    {
        for (int y = 1; y < _height - 1; y++)
        {
            for (int x = 1; x < _width - 1; x++)
            {
                if (_map[x, y] == 0) // Если это пол
                {
                    // Проверяем ширину прохода
                    if (IsNarrowPassage(x, y))
                    {
                        // Расширяем проход
                        for (int wx = x - 1; wx <= x + 1; wx++)
                        {
                            for (int wy = y - 1; wy <= y + 1; wy++)
                            {
                                if (wx >= 0 && wx < _width && wy >= 0 && wy < _height)
                                {
                                    _map[wx, wy] = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private bool IsNarrowPassage(int x, int y)
    {
        int wallCount = 0;
        for (int ny = y - 1; ny <= y + 1; ny++)
        {
            for (int nx = x - 1; nx <= x + 1; nx++)
            {
                if (nx >= 0 && nx < _width && ny >= 0 && ny < _height)
                {
                    if (_map[nx, ny] == 1) wallCount++;
                }
            }
        }
        return wallCount >= 6; // Слишком много стен вокруг - узкий проход
    }

    // Проверка доступности карты
    private bool IsMapAccessible()
    {
        // Начинаем проверку из центра
        int centerX = _width / 2;
        int centerY = _height / 2;

        if (_map[centerX, centerY] != 0) return false; // Центр заблокирован

        // Проверяем доступность через flood fill
        bool[,] visited = new bool[_width, _height];
        int accessibleTiles = FloodFillCount(centerX, centerY, visited);

        // Карта считается доступной если доступно хотя бы 20% площади
        int totalFloorTiles = CountFloorTiles();
        return accessibleTiles > totalFloorTiles * 0.2f;
    }

    private int FloodFillCount(int startX, int startY, bool[,] visited)
    {
        int count = 0;
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            count++;

            CheckNeighborCount(current.x + 1, current.y, visited, queue);
            CheckNeighborCount(current.x - 1, current.y, visited, queue);
            CheckNeighborCount(current.x, current.y + 1, visited, queue);
            CheckNeighborCount(current.x, current.y - 1, visited, queue);
        }

        return count;
    }

    private void CheckNeighborCount(int x, int y, bool[,] visited, Queue<Vector2Int> queue)
    {
        if (x >= 0 && x < _width && y >= 0 && y < _height &&
            _map[x, y] == 0 && !visited[x, y])
        {
            visited[x, y] = true;
            queue.Enqueue(new Vector2Int(x, y));
        }
    }

    private int CountFloorTiles()
    {
        int count = 0;
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (_map[x, y] == 0) count++;
            }
        }
        return count;
    }

    private void AddIrregularBorders()
    {
        _seed = Random.Range(-100, 100);
        _prng = new System.Random(_seed);

        
        GenerateIrregularBorder(0, 0, _width, 0, true); 
        GenerateIrregularBorder(0, _height - 1, _width, _height - 1, true); 
        GenerateIrregularBorder(0, 0, 0, _height, false); 
        GenerateIrregularBorder(_width - 1, 0, _width - 1, _height, false); 
    }

    private void GenerateIrregularBorder(int startX, int startY, int endX, int endY, bool isHorizontal)
    {
        if (isHorizontal)
        {
            
            for (int x = 0; x < _width; x += _borderIrregularity)
            {
                int yOffset = _prng.Next(-_borderIrregularity, _borderIrregularity + 1);
                int currentY = startY;

                for (int borderThickness = 0; borderThickness < _borderWidth; borderThickness++)
                {
                    int targetY = currentY + (startY == 0 ? borderThickness : -borderThickness);
                    targetY = Mathf.Clamp(targetY + yOffset, 0, _height - 1);

                    for (int wx = x; wx < Mathf.Min(x + _borderIrregularity, _width); wx++)
                    {
                        _map[wx, targetY] = 1;

                        
                        if (_prng.Next(0, 100) < 20 && borderThickness == 0)
                        {
                            int protrusion = _prng.Next(1, 3);
                            for (int py = targetY; py < Mathf.Min(targetY + protrusion, _height); py++)
                            {
                                _map[wx, py] = 1;
                            }
                        }
                    }
                }
            }
        }
        else
        {
           
            for (int y = 0; y < _height; y += _borderIrregularity)
            {
                int xOffset = _prng.Next(-_borderIrregularity, _borderIrregularity + 1);
                int currentX = startX;

                for (int borderThickness = 0; borderThickness < _borderWidth; borderThickness++)
                {
                    int targetX = currentX + (startX == 0 ? borderThickness : -borderThickness);
                    targetX = Mathf.Clamp(targetX + xOffset, 0, _width - 1);

                    for (int wy = y; wy < Mathf.Min(y + _borderIrregularity, _height); wy++)
                    {
                        _map[targetX, wy] = 1;

                       
                        if (_prng.Next(0, 100) < 20 && borderThickness == 0)
                        {
                            int protrusion = _prng.Next(1, 3);
                            for (int px = targetX; px < Mathf.Min(targetX + protrusion, _width); px++)
                            {
                                _map[px, wy] = 1;
                            }
                        }
                    }
                }
            }
        }
    }

    private void SmoothBorders()
    {
        for (int i = 0; i < _borderSmoothing; i++)
        {
            for (int y = 1; y < _height - 1; y++)
            {
                for (int x = 1; x < _width - 1; x++)
                {
                    
                    if (IsBorderCell(x, y))
                    {
                        int wallCount = GetSurroundingWallCount(x, y);
                        if (wallCount > 4) _map[x, y] = 1;
                        else if (wallCount < 4) _map[x, y] = 0;
                    }
                }
            }
        }
    }

    private bool IsBorderCell(int x, int y)
    {
        
        return x < _borderWidth * 2 || x > _width - _borderWidth * 2 - 1 ||
               y < _borderWidth * 2 || y > _height - _borderWidth * 2 - 1;
    }

    private void AddDecorations()
    {
        if (_biomes.Count == 0) return;

        foreach (var position in _groundTilemap.cellBounds.allPositionsWithin)
        {
            if (_groundTilemap.HasTile(position))
            {

                if (currentBiome != null && currentBiome.decorations != null && currentBiome.decorations.Length > 0)
                {
                    if (Random.value < currentBiome.decorationDensity)
                    {
                        TileBase decoration = currentBiome.decorations[Random.Range(0, currentBiome.decorations.Length)];
                        _decorTilemap.SetTile(position, decoration);
                    }
                }
            }
        }
    }

    private void GetBiome(int coefficient)
    {
        int currentLevel = levelManager.GetCurrentLevel();
        int indexBiome = currentLevel <= coefficient ? 0 : 1;

        currentBiome = _biomes[indexBiome];
    }


    private void RandomFillMap()
    {
        _seed = Random.Range(-100, 100);
        _prng = new System.Random(_seed);

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                
                _map[x, y] = (_prng.Next(0, 100) < _randomFillPercent) ? 1 : 0;
            }
        }
    }

    private void SmoothMap()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > _smoothingThreshold)
                    _map[x, y] = 1;
                else if (neighbourWallTiles < _smoothingThreshold)
                    _map[x, y] = 0;
            }
        }
    }

    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < _width && neighbourY >= 0 && neighbourY < _height)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += _map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    private void RenderCave()
    {
        _groundTilemap.ClearAllTiles();
        _wallTilemap.ClearAllTiles();
        _decorTilemap.ClearAllTiles();

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);

                if (_map[x, y] == 1)
                {
                    if (_biomes.Count > 0)
                    {
                        TileBase wallTile = currentBiome.wallTile;
                        _wallTilemap.SetTile(position, wallTile);
                    }
                }
                else
                {
                    if (_biomes.Count > 0)
                    {
                        TileBase groundTile = currentBiome.groundTiles[Random.Range(0, currentBiome.groundTiles.Length)];
                        _groundTilemap.SetTile(position, groundTile);
                    }
                }
            }
        }
    }

    private void AddWallShadows()
    {
        if (_shadowTilemap == null) return;

        _shadowTilemap.ClearAllTiles();

        foreach (var position in _wallTilemap.cellBounds.allPositionsWithin)
        {
            if (_wallTilemap.HasTile(position))
            {
                Vector3Int belowPosition = new Vector3Int(position.x, position.y - 1, position.z);

                if (ShouldAddShadow(belowPosition))
                {
                    if (_biomes.Count > 0 && currentBiome.wallShadowTile != null)
                    {
                        _shadowTilemap.SetTile(belowPosition, currentBiome.wallShadowTile);
                    }
                }
            }
        }
    }

    private bool ShouldAddShadow(Vector3Int position)
    {
        if (position.x < 0 || position.x >= _width || position.y < 0 || position.y >= _height)
            return false;

        if (_wallTilemap.HasTile(position))
            return false;

        if (_groundTilemap.HasTile(position))
            return true;

        return false;
    }

    [ContextMenu("Перегенерировать пещеру")]
    private void RegenerateCave()
    {
        GenerateCave();
        AddWallShadows();
        AddDecorations();
    }

    
}