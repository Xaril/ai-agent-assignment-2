using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMST : MonoBehaviour
{
    public List<List<GraphNode>> graph;
    public List<GraphNode> cells;

    private int gridNoX;
    private int gridNoZ;

    public GameObject terrain_manager_game_object;
    TerrainManager terrain_manager;

    // Start is called before the first frame update
    void Start()
    {
        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();

        graph = new List<List<GraphNode>>();
        cells = new List<GraphNode>();
        gridNoX = terrain_manager.myInfo.x_N;
        gridNoZ = terrain_manager.myInfo.z_N;
        for (int i = 0; i < gridNoX; ++i)
        {
            for (int j = 0; j < gridNoZ; ++j)
            {
                cells.Add(new GraphNode(i, j, terrain_manager.myInfo.traversability[i, j] > 0.5f));
            }
        }


        for (int i = 0; i < gridNoX; ++i)
        {
            for (int j = 0; j < gridNoZ; ++j)
            {
                if (terrain_manager.myInfo.traversability[i, j] > 0.5f)
                {
                    graph.Add(new List<GraphNode>());
                }
                else
                {
                    List<GraphNode> neighbours = FindCellNeighbours(i, j);
                    graph.Add(neighbours);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<GraphNode> FindCellNeighbours(int i, int j)
    {
        List<GraphNode> neighbours = new List<GraphNode>();

        if(i > 0) 
        {
            GraphNode left = cells[(i-1) * gridNoZ + j];
            if(!left.obstacle)
            {
                neighbours.Add(left);
            }
        }
        if (i < gridNoX - 1)
        {
            GraphNode right = cells[(i + 1) * gridNoZ + j];
            if (!right.obstacle)
            {
                neighbours.Add(right);
            }
        }
        if (j > 0)
        {
            GraphNode down = cells[i * gridNoZ + (j-1)];
            if (!down.obstacle)
            {
                neighbours.Add(down);
            }
        }
        if (j < gridNoZ - 1)
        {
            GraphNode up = cells[i * gridNoZ + (j + 1)];
            if (!up.obstacle)
            {
                neighbours.Add(up);
            }
        }

        return neighbours;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        for(int i = 0; i < graph.Count; ++i)
        {

            float x = terrain_manager.myInfo.get_x_pos(cells[i].i);
            float z = terrain_manager.myInfo.get_z_pos(cells[i].j);

            for(int j = 0; j < graph[i].Count; ++j)
            {
                float nX = terrain_manager.myInfo.get_x_pos(graph[i][j].i);
                float nZ = terrain_manager.myInfo.get_z_pos(graph[i][j].j);
                Gizmos.DrawLine(new Vector3(x, 1, z), new Vector3(nX, 1, nZ));
            }
        }
    }
}
