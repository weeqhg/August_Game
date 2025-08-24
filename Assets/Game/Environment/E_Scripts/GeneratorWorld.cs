using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GeneratorWorld : MonoBehaviour
{
    [Header("Настройка карты")]
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private float _scale;
    [SerializeField] private int _seed;

    [Header("Фрактальные настройки")]
    [SerializeField] private int _octaves;
    [SerializeField] private float _persistence;
    [SerializeField] private float _lacunarity;


    [Header("Локации")]
    [SerializeField] private List<Biome> _biomes = new List<Biome>();

    [Header("ТайлМап")]
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _wallTilemap;
    [SerializeField] private Tilemap _shadowTilemap;


    [System.Serializable]
    public class Biome
    {
        public string name;
        public float minHeight;
        public float maxHeight;
        public TileBase groundTile;
        public TileBase[] wall;
        public TileBase wallShadowTile;
        public float detailDensity;
    }

    private float[,] _heightMap;
    private float[,] _moistureMap;


    private void Start()
    {
        GeneratorWorlds();
        AddWallShadows();
    }
    private void AddWallShadows()
    {
        // Проходим по всем тайлам на карте препятствий
        foreach (var position in _wallTilemap.cellBounds.allPositionsWithin)
        {
            if (_wallTilemap.HasTile(position))
            {
                // Получаем тайл в текущей позиции
                TileBase currentTile = _wallTilemap.GetTile(position);

                // Проверяем, является ли этот тайл стеной (ищем соответствующий биом)
                Biome biomeWithThisWall = FindBiomeWithWallTile(currentTile);

                if (biomeWithThisWall != null && biomeWithThisWall.wallShadowTile != null)
                {
                    // Позиция под текущей стеной
                    Vector3Int belowPosition = new Vector3Int(position.x, position.y - 1, position.z);

                    // Проверяем условия для добавления тени
                    if (ShouldAddShadow(belowPosition))
                    {
                        // Добавляем тень из соответствующего биома
                        _shadowTilemap.SetTile(belowPosition, biomeWithThisWall.wallShadowTile);

                        // Убираем коллайдер с тайла тени (если нужно)
                        // Для этого可能需要 отдельный tilemap для теней
                    }
                }
            }
        }
    }

    private Biome FindBiomeWithWallTile(TileBase wallTile)
    {
        foreach (var biome in _biomes)
        {
            if (biome.wall != null)
            {
                foreach (var wall in biome.wall)
                {
                    if (wall == wallTile)
                    {
                        return biome;
                    }
                }
            }
        }
        return null;
    }

    private bool ShouldAddShadow(Vector3Int position)
    {
        // Не добавляем тень если:
        // 1. Позиция выходит за пределы карты
        if (position.x < 0 || position.x >= _width || position.y < 0 || position.y >= _height)
            return false;

        // 2. Уже есть тайл препятствия на этой позиции
        if (_wallTilemap.HasTile(position))
            return false;

        // 3. Нет земли под этим местом
        if (!_groundTilemap.HasTile(position))
            return false;

        return true;
    }
    public void GeneratorWorlds()
    {
        GenerateHeightMap();
        GenerateMoistureMap();
        RenderWorld();

    }
    
    private void GenerateHeightMap()
    {
        _heightMap = GenerateFractalNoise(_seed);
    }
    private void GenerateMoistureMap()
    {
        _moistureMap = GenerateFractalNoise(_seed + 1);
    }



    private float[,] GenerateFractalNoise(int seed)
    {
        float[,] map = new float[_width, _height];
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[_octaves];

        for (int i = 0; i < _octaves; i++)
        {
            float offsetX = prng.Next(-10000, 10000);
            float offsetY = prng.Next(-10000, 10000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < _octaves; i++)
                {
                    float sampleX = x / _scale * frequency + octaveOffsets[i].x;
                    float sampleY = y / _scale * frequency + octaveOffsets[i].y;

                    float perLineValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perLineValue * amplitude;

                    amplitude *= _persistence;
                    frequency *= _lacunarity;
                }

                map[x, y] = (noiseHeight + 1) / 2f;
            }
        }
        return map;
    }

    private void RenderWorld()
    {
        _groundTilemap.ClearAllTiles();
        _wallTilemap.ClearAllTiles();

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                float height = _heightMap[x, y];
                float moisture = _moistureMap[x, y];

                Biome biome = GetBiome(height, moisture);
                Debug.Log(biome);
                if (biome != null)
                {
                    _groundTilemap.SetTile(position, biome.groundTile);

                    if (biome.wall.Length > 0 && Random.value < biome.detailDensity)
                    {
                        TileBase detailTile = biome.wall[Random.Range(0, biome.wall.Length)];
                        _wallTilemap.SetTile(position, detailTile);
                    }
                }

            }
        }


    }

    private Biome GetBiome(float height, float moisture)
    {
        Debug.Log($"Поиск биома для: height={height:F2}, moisture={moisture:F2}");

        foreach (var biome in _biomes)
        {
            Debug.Log($"Проверка биома '{biome.name}': {biome.minHeight:F2} - {biome.maxHeight:F2}");

            if (height >= biome.minHeight && height <= biome.maxHeight)
            {
                Debug.Log($"Найден биом: {biome.name}");
                return biome;
            }
        }

        Debug.LogWarning($"Биом не найден для height={height:F2}");
        return null;
    }
}
