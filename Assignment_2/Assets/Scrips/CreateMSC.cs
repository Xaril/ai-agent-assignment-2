using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMSC : MonoBehaviour
{
    public GameObject terrain_manager_game_object;
    TerrainManager terrain_manager;
    private int gridNoX;
    private int gridNoZ;
    private CoverNode[,] grid;
    public List<CoverNode> coverNodes;
    private int numberOfObstecles;
    private int numberOfGridCells;
    public List<GameObject> cars;

    // Start is called before the first frame update
    void Start()
    {
        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
        gridNoX = terrain_manager.myInfo.x_N;
        gridNoZ = terrain_manager.myInfo.z_N;
        grid = new CoverNode[gridNoX, gridNoZ];
        coverNodes = new List<CoverNode>();
        this.numberOfGridCells = gridNoX * gridNoZ;
        this.numberOfObstecles = 0;
        for(int i = 0; i < gridNoX; ++i)
        {
            for(int j = 0; j < gridNoZ; ++j)
            {
                bool isObstecle = terrain_manager.myInfo.traversability[i, j] > 0.5f;
                if (isObstecle) numberOfObstecles++;
                grid[i, j] = new CoverNode(new Vector3(terrain_manager.myInfo.get_x_pos(i), 0.5f, terrain_manager.myInfo.get_z_pos(j)), isObstecle);
            }
        }
        createMSC();
    }

    public List<Vector3>[] GetPaths()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach(CoverNode node in coverNodes)
        {
            positions.Add(node.position);
        }
        List<Vector3>[] paths = TSP.GenerateCarPaths(TSP.GreedyPath(positions));
        return paths;
    }

    private void createMSC()
    {
        // Marke all nodes that are covered at the start
        int numberOfNodesCoveredAtStart = 0;
        foreach (GameObject car in cars)
        {
            int i = terrain_manager.myInfo.get_i_index(car.transform.position.x);
            int j = terrain_manager.myInfo.get_j_index(car.transform.position.z);
            CoverNode carNode = grid[i,j];
            for (int s = 0; s < gridNoX; ++s)
            {
                for (int t = 0; t < gridNoZ; ++t)
                {
                    if (grid[s,t].isCovered || grid[s, t].isObstacle) continue;
                    if(grid[s,t].IsCoveredBy(carNode)) {
                        grid[s, t].isCovered = true;
                        numberOfNodesCoveredAtStart++;
                    }
                }
            }
        }
        // Greedy algorithm
        int cellsCovered = numberOfObstecles + numberOfNodesCoveredAtStart;
        while (cellsCovered < numberOfGridCells)
        {
            int maxNumberCovered = 0;
            Vector2 maxCoverNodeIndex = new Vector2();
            // Find the node that covers the most of other oncovered nodes
            for (int i = 0; i < gridNoX; ++i)
            {
                for (int j = 0; j < gridNoZ; ++j)
                {
                    if (grid[i, j].isCovered || grid[i, j].isObstacle) continue;
                    int numberCovered = 0;
                    for (int s = 0; s < gridNoX; ++s)
                    {
                        for (int t = 0; t < gridNoZ; ++t)
                        {
                            if (grid[s, t].isCovered || grid[s, t].isObstacle) continue;

                            if (grid[s, t].IsCoveredBy(grid[i, j]))
                            {
                                numberCovered++;
                            }
                        }
                    }

                    if (numberCovered > maxNumberCovered)
                    {
                        maxNumberCovered = numberCovered;
                        maxCoverNodeIndex = new Vector2(i, j);
                    }
                }
            }
            cellsCovered += maxNumberCovered;
            coverNodes.Add(grid[(int)maxCoverNodeIndex.x, (int)maxCoverNodeIndex.y]);
            for (int i = 0; i < gridNoX; ++i)
            {
                for (int j = 0; j < gridNoZ; ++j)
                {
                    if(grid[i,j].IsCoveredBy(grid[(int)maxCoverNodeIndex.x, (int)maxCoverNodeIndex.y]))
                    {
                        grid[i, j].isCovered = true;
                    }
                }
            }
        }
        
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }


        Gizmos.color = Color.red;

        foreach (CoverNode node in coverNodes)
        {
            Gizmos.DrawCube(node.position, new Vector3(5, 5, 5));
        }
        

        /*
        for (int i = 0; i < gridNoX; ++i)
        {
            for (int j = 0; j < gridNoZ; ++j)
            {
                if (grid[i, j].isObstacle) continue;
                foreach(Vector3 cornerPosition in grid[i,j].cornerPositions)
                {
                    Gizmos.DrawLine(grid[i, j].position, cornerPosition);
                }
            }
        }

        Gizmos.color = Color.red;

        for (int i = 0; i < gridNoX; ++i)
        {
            for (int j = 0; j < gridNoZ; ++j)
            {
                if (grid[i, j].isCovered)
                {
                    Gizmos.DrawLine(grid[8, 8].position, grid[i, j].position);
                }
            }
        } */
    }
}