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

    List<Vector3> path;

    private int turretRangeCost = 20;

    public void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log(enemies.Length);
        GenerateGrid();
        GenerateCosts();


        Point startPoint = new Point(420, 100);
        Point endPoint = new Point(250, 180);


        //PathGenerator aStar = new PathGenerator(manager, grid);
        //path = aStar.GetPath(startPoint, endPoint, transform.rotation.eulerAngles.y);

    }

    int i = 0;
    public void FixedUpdate()
    {
        i++;
        if (i % 8 == 0) x++;
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
                    cell.cost *= turretRangeCost;
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


    private int x = 0;
    private void OnDrawGizmos()
    {
        if (path != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < x; i++)
            {
                Gizmos.DrawCube(path[i], Vector3.one);
            }
        }

        if (grid == null) return;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (grid[i, j].isObstacle) continue;

                if (grid[i,j].cost < 20)
                {
                    Gizmos.color = Color.green;
                } else if (grid[i, j].cost < 400)
                {
                    Gizmos.color = Color.yellow;
                } else if (grid[i, j].cost < 8000)
                {
                    Gizmos.color = new Color(1.0f, 0.64f, 0.0f); ; //orange
                } else if (grid[i,j].cost < 160000)
                {
                    Gizmos.color = Color.red;
                } else
                {
                    Gizmos.color = Color.black;
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
        this.cost = 1;
    }
}

