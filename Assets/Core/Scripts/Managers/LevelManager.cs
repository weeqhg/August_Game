using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Должен отвечать за настройку уровня, какие мобы и объекты будут появляться, их кол-во
/// нужно контролировать что на каждой локации должно быть 5 уровней
/// </summary>




public class LevelManager : MonoBehaviour
{
    [SerializeField] private int enemiesCount;
    [SerializeField] private int items;

    private Spawn spawn;
    private int countEnemy;
    private int levelIndex = 0;


    [Header("Настройки уровней")]
    // Массив массивов для разных типов врагов по уровням
    [SerializeField] private GameObject[][] enemyPrefabsByLevel;

    [Header("Префабы для уровня 1")]
    [SerializeField] private GameObject[] level1Enemies;

    [Header("Префабы для уровня 2")]
    [SerializeField] private GameObject[] level2Enemies;

    [Header("Префабы для уровня 3")]
    [SerializeField] private GameObject[] level3Enemies;

    private void Awake()
    {
        GameManager.Instance.Register(this);
        InitializeEnemyArrays();
    }

    private void InitializeEnemyArrays()
    {
        enemyPrefabsByLevel = new GameObject[3][];
        enemyPrefabsByLevel[0] = level1Enemies;
        enemyPrefabsByLevel[1] = level2Enemies;
        enemyPrefabsByLevel[2] = level3Enemies;
    }

    public void RandomSetting(Spawn newSpawn)
    {
        GameObject[] enemyArray = enemyPrefabsByLevel[levelIndex];
        spawn = newSpawn;
        spawn.SettingSpawnEnemy(enemyArray, enemiesCount, items);
    }


    public void GetCountEnemy(int count)
    {
        countEnemy = count;
    }

    public void CounterDiedEnemy()
    {
        countEnemy--;
        CheckLocation();
    }

    private void CheckLocation()
    {
        if (countEnemy == 0)
        {
            spawn.SpawnPortalNextLevel();
            levelIndex++;
            
            //Для потом нужно убрать
            if (levelIndex >= 2)
            {
                levelIndex = 0;
            }
        }
    }
}
