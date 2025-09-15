using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalNextLevel : MonoBehaviour
{
    [SerializeField, HideInInspector] private PriorityManager priorityManager;
    [SerializeField, HideInInspector] private GameObject player;
    [SerializeField, HideInInspector] private List<GameObject> enemies;
    [SerializeField, HideInInspector] private List<GameObject> interactItems;

    void Start()
    {
        priorityManager = GameManager.Instance.Get<PriorityManager>();
    }

    public void GetClearObject(GameObject playerSpawn, List<GameObject> enemiesSpawn, List<GameObject> interactItemSpawn)
    {
        player = playerSpawn;
        enemies = enemiesSpawn;
        interactItems = interactItemSpawn;
        Debug.Log(interactItems.Count);
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CreateNextLevel();
        }
    }

    private void CreateNextLevel()
    {
        // Сбрасываем игру через PriorityManager
        priorityManager.ResetGame();

        Destroy(gameObject);
    }

}
