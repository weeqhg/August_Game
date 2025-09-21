using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Должен отвечать за настройку уровня, какие мобы и объекты будут появляться, их кол-во
/// нужно контролировать что на каждой локации должно быть 5 уровней
/// </summary>



[System.Serializable]
public class GameObjectList
{
    [Header("Настройки")]
    public int enemiesCount;
    public int items;
    public List<GameObject> enemies = new List<GameObject>();
}
public class LevelManager : MonoBehaviour
{
    [Header("Необходимые ссылки")]
    [SerializeField] private Spawn spawn;
    //Массив массивов для разных типов врагов по уровням
    [SerializeField] private List<GameObjectList> enemyPrefabsByLevelList = new List<GameObjectList>();
    private int countEnemy;
    private int levelIndex = 0;

    public int GetCurrentLevel() => levelIndex;
    private void Awake()
    {
        GameManager.Instance.Register(this);
    }


    public (GameObject[], int, int) RandomSetting()
    {
        GameObject[] enemyArray = GetEnemiesForLevel(levelIndex);
        int newEnemyCount = GetEnemyCount(levelIndex);
        int newItemsCount = GetItemCount(levelIndex);

        return (enemyArray, newEnemyCount, newItemsCount);
    }


    public void GetCountEnemy(int count)
    {
        countEnemy = count;
    }

    //Счетчик мобов на уровне
    public void CounterDiedEnemy()
    {
        countEnemy--;
        CheckLocation();
    }

    private void CheckLocation()
    {
        if (countEnemy == 0)
        {
            levelIndex++;
            spawn.SpawnPortalNextLevel(levelIndex);
        }
    }

    // Использование
    public GameObject[] GetEnemiesForLevel(int levelIndex)
    {
        //Защита от выхода из массива
        if (levelIndex > enemyPrefabsByLevelList.Count)
        {
            levelIndex = 0;
            return enemyPrefabsByLevelList[levelIndex].enemies.ToArray();
        }
        if (levelIndex >= 0 && levelIndex < enemyPrefabsByLevelList.Count)
        {
            return enemyPrefabsByLevelList[levelIndex].enemies.ToArray();
        }
        return enemyPrefabsByLevelList[0].enemies.ToArray();
    }

    public int GetEnemyCount(int levelIndex)
    {
        if (levelIndex > enemyPrefabsByLevelList.Count)
        {
            levelIndex = 0;
            return enemyPrefabsByLevelList[levelIndex].enemiesCount;
        }
        if (levelIndex >= 0 && levelIndex < enemyPrefabsByLevelList.Count)
        {
            return enemyPrefabsByLevelList[levelIndex].enemiesCount;
        }
        return enemyPrefabsByLevelList[0].enemiesCount;
    }

    public int GetItemCount(int levelIndex)
    {
        if (levelIndex > enemyPrefabsByLevelList.Count)
        {
            levelIndex = 0;
            return enemyPrefabsByLevelList[levelIndex].items;
        }
        if (levelIndex >= 0 && levelIndex < enemyPrefabsByLevelList.Count)
        {
            return enemyPrefabsByLevelList[levelIndex].items;
        }
        return enemyPrefabsByLevelList[0].items;
    }
}
