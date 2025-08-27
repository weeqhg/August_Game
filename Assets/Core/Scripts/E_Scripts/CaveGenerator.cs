using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CaveGenerator : MonoBehaviour
{
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

    [Header("Биомы пещеры")]
    [SerializeField] private List<Biome> _biomes = new List<Biome>();

    [Header("Tilemaps")]
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _wallTilemap;
    [SerializeField] private Tilemap _shadowTilemap;
    [SerializeField] private Tilemap _decorTilemap;

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
        Debug.Log("генерация");
        GenerateCave();
        AddWallShadows();
        AddDecorations();
    }

    public void GenerateCave()
    {
        _map = new int[_width, _height];
        RandomFillMap();

        for (int i = 0; i < _smoothingIterations; i++)
        {
            SmoothMap();
        }

        AddIrregularBorders(); 
        SmoothBorders(); 
        RenderCave();
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
                Biome biome = GetBiomeForPosition(position.x, position.y);

                if (biome != null && biome.decorations != null && biome.decorations.Length > 0)
                {
                    if (Random.value < biome.decorationDensity)
                    {
                        TileBase decoration = biome.decorations[Random.Range(0, biome.decorations.Length)];
                        _decorTilemap.SetTile(position, decoration);
                    }
                }
            }
        }
    }

    private Biome GetBiomeForPosition(int x, int y)
    {
        float depth = (float)y / _height;

        foreach (var biome in _biomes)
        {
            if (depth >= biome.minDepth && depth <= biome.maxDepth)
            {
                if (Random.value <= biome.spawnChance)
                {
                    return biome;
                }
            }
        }

        return _biomes[0];
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

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);

                if (_map[x, y] == 1)
                {
                    if (_biomes.Count > 0)
                    {
                        TileBase wallTile = _biomes[0].wallTile;
                        _wallTilemap.SetTile(position, wallTile);
                    }
                }
                else
                {
                    if (_biomes.Count > 0)
                    {
                        TileBase groundTile = _biomes[0].groundTiles[Random.Range(0, _biomes[0].groundTiles.Length)];
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
                    if (_biomes.Count > 0 && _biomes[0].wallShadowTile != null)
                    {
                        _shadowTilemap.SetTile(belowPosition, _biomes[0].wallShadowTile);
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