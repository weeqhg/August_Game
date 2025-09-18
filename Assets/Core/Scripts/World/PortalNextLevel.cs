using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalNextLevel : MonoBehaviour
{
    [SerializeField, HideInInspector] private PriorityManager priorityManager;
    [SerializeField, HideInInspector] private GameObject player;
    [SerializeField, HideInInspector] private List<GameObject> enemies;
    [SerializeField, HideInInspector] private List<GameObject> interactItems;

    private int levelIndex;
    void Start()
    {
        priorityManager = GameManager.Instance.Get<PriorityManager>();
    }


    public void Initialize(int level)
    {
        levelIndex = level;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CreateNextLevel(levelIndex);
        }
    }

    private void CreateNextLevel(int level)
    {
        // Сбрасываем игру через PriorityManager
        priorityManager.ResetLevel(levelIndex);

        Destroy(gameObject);
    }

}
