using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityManager : MonoBehaviour
{
    private Spawn _spawn;
    private CaveGenerator _caveGenerator;
    private TilemapNavMeshGenerator _navMeshGenerator;

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
    }

    private void PriorityStart()
    {
        _caveGenerator.StartGenerate();
        _navMeshGenerator.BuildNavMesh();
        _spawn.SpawnOnWorld();
    }
}
