using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityManager : MonoBehaviour
{
    private Spawn _spawn;
    private CaveGenerator _caveGenerator;


    private void Start()
    {
        GetAllComponent();
        PriorityStart();
    }

    private void GetAllComponent()
    {
        _spawn = GameManager.Instance.Get<Spawn>();
        _caveGenerator = GameManager.Instance.Get<CaveGenerator>();
    }

    private void PriorityStart()
    {
        _caveGenerator.StartGenerate();
        _spawn.SpawnPlayer();
        _spawn.SpawnEnemies();
        _spawn.SpawnItemsInteract();
    }
}
