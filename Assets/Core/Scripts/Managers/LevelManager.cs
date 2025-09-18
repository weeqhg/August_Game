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
    public List<GameObject> enemies = new List<GameObject>();
}
public class LevelManager : MonoBehaviour
{
    [Header("Необходимые ссылки")]
    [SerializeField] private Spawn spawn;
    [Header("Настройки")]
    [SerializeField] private int enemiesCount;
    [SerializeField] private int items;
    //Массив массивов для разных типов врагов по уровням
    [SerializeField] private List<GameObjectList> enemyPrefabsByLevelList = new List<GameObjectList>();




    private int countEnemy;
    private int levelIndex = 0;


    private void Awake()
    {
        GameManager.Instance.Register(this);
    }


    public (GameObject[], int, int) RandomSetting()
    {
        GameObject[] enemyArray = GetEnemiesForLevel(levelIndex);
        int newEnemyCount = enemiesCount;
        int newItemsCount = items;

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
            
            //потом, нужно убрать
            if (levelIndex >= 2)
            {
                levelIndex = 0;
            }
        }
    }

    // Использование
    public GameObject[] GetEnemiesForLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < enemyPrefabsByLevelList.Count)
        {
            return enemyPrefabsByLevelList[levelIndex].enemies.ToArray();
        }
        return new GameObject[0];
    }
}
