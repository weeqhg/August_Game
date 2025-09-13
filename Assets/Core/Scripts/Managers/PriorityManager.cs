using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityManager : MonoBehaviour
{
    private Spawn _spawn;
    private CaveGenerator _caveGenerator;
    private TilemapNavMeshGenerator _navMeshGenerator;
    private SaveSystem _saveSystem;

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
    }

    private void PriorityStart()
    {
        SaveSystemActive();
        _caveGenerator.StartGenerate();
        _navMeshGenerator.BuildNavMesh();
        _spawn.SpawnOnWorld();
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
}
