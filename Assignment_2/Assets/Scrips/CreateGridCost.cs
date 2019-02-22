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
    public GameObject[] players;
    private readonly int minAreaOffset = 4;

    public List<Vector3> path;

    private readonly int turretRangeCost = 20;

    public void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        players = GameObject.FindGameObjectsWithTag("Player");
        Point startPoint = new Point((int) players[0].transform.position.x, (int)players[0].transform.position.z);

        GenerateGrid();
        GenerateCosts();

        this.path = FindPath(startPoint, players[0].transform.rotation.eulerAngles.y);


    }

    private void GetAdjesantArea(int i, int j, int cost,  ref bool[,] isIncluded,  ref Vector3 positionSum, ref int numberOfCells)
    {
        if (isIncluded[i, j] || grid[i, j].isObstacle) return;
        if(grid[i,j].cost == cost)
        {
            isIncluded[i, j] = true;
            positionSum += grid[i, j].position;
            numberOfCells++;
            GetAdjesantArea(i + 1, j, cost, ref isIncluded, ref positionSum, ref numberOfCells);
            GetAdjesantArea(i - 1, j, cost, ref isIncluded, ref positionSum, ref numberOfCells);
            GetAdjesantArea(i, j + 1, cost, ref isIncluded, ref positionSum, ref numberOfCells);
            GetAdjesantArea(i, j - 1, cost, ref isIncluded, ref positionSum, ref numberOfCells);
        }
    }

    private void UpdateCostGrid(int indexOfDestroyedTurret)
    {
        GameObject turret = enemies[indexOfDestroyedTurret];
        foreach (CostGridCell cell in grid)
        {
            if (cell.isObstacle) continue;
            float distance = (turret.transform.position - cell.position).magnitude;
            Vector3 direction = (cell.position - turret.transform.position).normalized;
            RaycastHit hit;
            int layer_mask = LayerMask.GetMask("CubeWalls");
            // Does the ray intersect any objects excluding the player layer
            if (!Physics.Raycast(turret.transform.position + direction, direction, out hit, distance - 0.5f, layer_mask))
            {
                cell.cost /= turretRangeCost;
            }
        }
    }

    private List<Vector3> FindPath(Point startPoint, float angle)
    {
        List<Vector3> path = new List<Vector3>();
        Point point = startPoint;
        for(int i = 0; i < 4; i++)
        {
            List<SameCostArea> oneTurretAreas = FindAreas(turretRangeCost);
            path.AddRange(FindBestPath(point, 0f, oneTurretAreas));
            point = new Point((int)path[path.Count - 1].x, (int)path[path.Count - 1].z);
        }

        return path;
    }

    private List<Vector3> FindBestPath(Point startPoint, float angle, List<SameCostArea> areas)
    {
        List<Vector3> path = new List<Vector3>();
        PathGenerator aStar = new PathGenerator(manager, grid);
        int minPathCost = int.MaxValue;
        int numberOfCellsInMinPathArea = 0;
        foreach (SameCostArea area in areas)
        {

            int currentPathCost = 0;
            List<Vector3> currentPath = aStar.GetPath(startPoint, new Point((int) area.position.x,(int) area.position.z), angle);
            /*
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = area.position;
            cube.transform.localScale = new Vector3(3, 20, 3);
            */
            foreach(Vector3 v in currentPath)
            {
                int i = (int)(v.x - xlow);
                int j = (int)(v.z - zlow);
                currentPathCost += grid[i,j].cost;
            }

            if(currentPathCost <= minPathCost)
            {
                minPathCost = currentPathCost;
                numberOfCellsInMinPathArea = area.numberOfCells;
                path = currentPath;
            }
        }


        for(int i = 0; i < enemies.Length;i++)
        {
            if (enemies[i] == null) continue;
            float distance = (enemies[i].transform.position - path[path.Count - 1]).magnitude;
            Vector3 direction = (path[path.Count - 1] - enemies[i].transform.position).normalized;
            RaycastHit hit;
            int layer_mask = LayerMask.GetMask("CubeWalls");
            // Does the ray intersect any objects excluding the player layer
            if (!Physics.Raycast(enemies[i].transform.position + direction, direction, out hit, distance - 0.5f, layer_mask))
            {
                UpdateCostGrid(i);
            }
        }

        return path;
    }


    private List<SameCostArea> FindAreas(int cost)
    {
        bool[,] isIncluded = new bool[width, height];
        List<SameCostArea> list = new List<SameCostArea>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (isIncluded[i, j] || grid[i,j].isObstacle) continue;
                Vector3 positionSum = new Vector3(0f,0f,0f);
                int numberOfCells = 0;
                if (grid[i,j].cost == cost)
                {
                    GetAdjesantArea(i, j, cost, ref isIncluded, ref positionSum, ref numberOfCells);
                    
                    Vector3 averagePosition =  positionSum / numberOfCells;
                    if (CheckIfAreaBigEnough(averagePosition, cost))
                    {
                        list.Add(new SameCostArea(averagePosition,numberOfCells,cost));
                    }
                }
            }
        }

        return list;
    }

    private bool CheckIfAreaBigEnough(Vector3 centerPosition, int cost)
    {
        int i = (int) (centerPosition.x - xlow);
        int j = (int) (centerPosition.z - zlow);
        
        for(int s = 0; s < minAreaOffset; s++)
        {
            if (grid[i + s, j].cost > cost || grid[i + s, j].isObstacle || !IsInGrid(i + s, j)) return false;
            if (grid[i - s, j].cost > cost || grid[i - s, j].isObstacle || !IsInGrid(i - s, j)) return false;
            if (grid[i, j + s].cost > cost || grid[i, j + s].isObstacle || !IsInGrid(i, j + s)) return false;
            if (grid[i, j - s].cost > cost || grid[i, j - s].isObstacle || !IsInGrid(i, j - s)) return false;
        }

        return true;
    }

    private bool IsInGrid(int i, int j)
    {
        return i < width && i >= 0 && j < height && j >= 0;
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
                if (!Physics.Raycast(turret.transform.position + direction, direction, out hit, distance - 0.5f, layer_mask))
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
                grid[j, i] = new CostGridCell(new Vector3(j + xlow + 0.5f, 0f, i+zlow + 0.5f), canTraverse == 1);
            }
        }

    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (path == null) return;
        foreach(Vector3 p in path)
        {
            Gizmos.DrawCube(p, Vector3.one);
        }
        return;
        if (grid == null) return;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (grid[i, j].isObstacle) continue;

                if (grid[i,j].cost < turretRangeCost)
                {
                    Gizmos.color = Color.green;
                } else if (grid[i, j].cost < Mathf.Pow(turretRangeCost,2))
                {
                    Gizmos.color = Color.yellow;
                } else if (grid[i, j].cost < Mathf.Pow(turretRangeCost, 3))
                {
                    Gizmos.color = new Color(1.0f, 0.64f, 0.0f); ; //orange
                } else if (grid[i,j].cost < Mathf.Pow(turretRangeCost, 4))
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

public class SameCostArea
{
    public Vector3 position;
    public int numberOfCells;
    public int cost;

    public SameCostArea(Vector3 position, int numberOfCells, int cost)
    {
        this.position = position;
        this.numberOfCells = numberOfCells;
        this.cost = cost;
    }
}

