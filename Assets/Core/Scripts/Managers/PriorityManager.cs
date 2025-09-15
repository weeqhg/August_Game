using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// нужен чтобы вызывать в нужном порядке скрипты
/// в остальных скрпитах Start использовать только для получений компонентов
///
/// </summary>
public class PriorityManager : MonoBehaviour
{
    private Spawn _spawn;
    private CaveGenerator _caveGenerator;
    private TilemapNavMeshGenerator _navMeshGenerator;
    private SaveSystem _saveSystem;
    private LevelManager _levelManager;

    private void Awake()
    {
        GameManager.Instance.Register(this);
    }

    private void Start()
    {
        GetAllComponent();
        PriorityStart();
    }

    private void GetAllComponent()
    {
        _spawn = GameManager.Instance.Get<Spawn>();
        _caveGenerator = GameManager.Instance.Get<CaveGenerator>();
        _navMeshGenerator = GameManager.Instance.Get<TilemapNavMeshGenerator>();
        _saveSystem = GameManager.Instance.Get<SaveSystem>();
        _levelManager = GameManager.Instance.Get<LevelManager>();
    }

    private void PriorityStart()
    {
        SaveSystemActive();
        _levelManager.RandomSetting(_spawn);
        _caveGenerator.StartGenerate();
        _navMeshGenerator.BuildNavMesh();
        _spawn.StartSpawn();
    }

    private void SaveSystemActive()
    {
        _saveSystem.LoadGame();
        GameData data = _saveSystem.GetCurrentGameData();
        if (data == null)
        {
            _saveSystem.CreateNewGame();
        }
    }

    public void ResetGame()
    {
        _caveGenerator.StartGenerate();
        _navMeshGenerator.BuildNavMesh();
        _levelManager.RandomSetting(_spawn);
        _spawn.StartSpawn();
    }
}
