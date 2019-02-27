using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRPPathsGenerator : MonoBehaviour
{
    private GameObject[] enemies;
    private GameObject[] friends;
    public TerrainManager terrain_manager;
    private List<int>[] result;
    private List<Vector3> enemy_positions = new List<Vector3>();


    // Start is called before the first frame update
    void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        friends = GameObject.FindGameObjectsWithTag("Player");
        
        for (int i = 0; i < enemies.Length; ++i)
        {
            enemy_positions.Add(enemies[i].transform.position);
        }
        Vector3 carAverage = new Vector3();
        foreach(GameObject car in friends)
        {
            carAverage += car.transform.position;
        }
        enemy_positions.Insert(0, carAverage / friends.Length);
        result = new VRP(enemy_positions, friends.Length, terrain_manager).result;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<Vector3> GetRouteForCar(int carNumber)
    {
        List<Vector3> route = new List<Vector3>();

        for(int i = 1; i < result[carNumber].Count; i++)
        {
            route.Add(enemy_positions[result[carNumber][i]]);
        }

        return route;
    }

    private void OnDrawGizmos()
    {
        if (result == null) return;
        for(int i = 0; i < result.Length; i++)
        {
            if (i == 0) Gizmos.color = Color.red;
            else if (i == 1) Gizmos.color = Color.blue;
            else Gizmos.color = Color.green;
            for(int j = 0; j < result[i].Count - 1; j++)
            {
                Gizmos.DrawLine(enemy_positions[result[i][j]], enemy_positions[result[i][j + 1]]);
            }
        }
    }
}
