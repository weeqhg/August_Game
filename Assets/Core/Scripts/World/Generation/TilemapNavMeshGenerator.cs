using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using NavMeshPlus.Components;
using UnityEngine.AI;
using System.Collections.Generic;

public class TilemapNavMeshGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap[] _walkableTilemaps;
    [SerializeField] private Tilemap[] _obstacleTilemaps;
    [SerializeField] private NavMeshSurface _navMeshSurface;

    private void Awake()
    {
        GameManager.Instance.Register(this);
    }

    // Построение NavMesh
    public void BuildNavMesh()
    {
        if (_navMeshSurface == null)
        {
            _navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
        }

        // Строим NavMesh
        _navMeshSurface.BuildNavMesh();

        Debug.Log("NavMesh построен успешно!");
    }

    // Метод для обновления NavMesh при изменении карты
    public void UpdateNavMesh()
    {
        if (_navMeshSurface != null && _navMeshSurface.navMeshData != null)
        {
            _navMeshSurface.UpdateNavMesh(_navMeshSurface.navMeshData);
            Debug.Log("NavMesh обновлен!");
        }
    }

    // Метод для отладки - показывает границы NavMesh
    private void OnDrawGizmosSelected()
    {
        if (_navMeshSurface != null && _navMeshSurface.navMeshData != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(_navMeshSurface.navMeshData.sourceBounds.center, _navMeshSurface.navMeshData.sourceBounds.size);
        }
    }
}