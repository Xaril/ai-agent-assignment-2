using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGridCost: MonoBehaviour
{
    private CostGridCell[,] grid; //Assume 1 = obstacle, 0 = free road
    private int width, height;
    private int xlow, zlow;
    public TerrainManager manager;
    public GameObject[] enemies;

    private int turretRangeCost = 20;

    public void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GenerateGrid();
        //enemies[5] = null;
        GenerateCosts();
    }

    public void FixedUpdate()
    {
        
    }

    private void GenerateCosts()
    { 
        foreach (GameObject turret in enemies)
        {
            if (turret == null) continue;
            foreach (CostGridCell cell in grid)
            {
                if (cell.isObstacle) continue;
                float distance = (turret.transform.position - cell.position).magnitude;
                Vector3 direction = (cell.position - turret.transform.position).normalized;
                RaycastHit hit;
                int layer_mask = LayerMask.GetMask("CubeWalls");
                // Does the ray intersect any objects excluding the player layer
                if (!Physics.Raycast(turret.transform.position + direction, direction, out hit, distance, layer_mask))
                {
                    cell.cost += turretRangeCost;
                }
            }
        }
    }

    private void GenerateGrid()
    {
        xlow = (int)manager.myInfo.x_low;
        zlow = (int)manager.myInfo.z_low;
        int xhigh = (int)manager.myInfo.x_high;
        int zhigh = (int)manager.myInfo.z_high;
        width = xhigh - xlow;
        height = zhigh - zlow;

        grid = new CostGridCell[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int tmp_x = manager.myInfo.get_i_index(xlow + j);
                int tmp_z = manager.myInfo.get_j_index(zlow + i);
                int canTraverse = (int)manager.myInfo.traversability[tmp_x, tmp_z];
                grid[j, i] = new CostGridCell(new Vector3(j + xlow, 0f, i+zlow), canTraverse == 1);
            }
        }

    }



    private void OnDrawGizmos()
    {

        if (grid == null) return;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (grid[i, j].isObstacle) continue;

                if (grid[i,j].cost < 20)
                {
                    Gizmos.color = Color.green;
                } else if (grid[i, j].cost < 40)
                {
                    Gizmos.color = Color.yellow;
                } else if (grid[i, j].cost < 60)
                {
                    Gizmos.color = new Color(1.0f, 0.64f, 0.0f); ; //orange
                } else
                {
                    Gizmos.color = Color.red;
                }

                //Gizmos.color = Color.Lerp(Color.green, Color.red, Mathf.InverseLerp(0, enemies.Length * turretRangeCost, grid[i, j].cost));
                Gizmos.DrawCube(grid[i,j].position, Vector3.one);
            }
        }
       
    }
}

public class CostGridCell
{
    public Vector3 position;
    public bool isObstacle;
    public int cost;

    public CostGridCell(Vector3 position, bool isObstacle)
    {
        this.position = position;
        this.isObstacle = isObstacle;
        this.cost = 0;
    }
}

