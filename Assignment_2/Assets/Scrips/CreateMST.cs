using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMST : MonoBehaviour
{
    public List<List<GraphNode>> graph;
    public List<GraphNode> cells;
    public List<List<GraphNode>> mst;

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

        mst = Prim();
    }

    void FixedUpdate()
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

    public List<List<GraphNode>> Prim()
    {
        List<List<GraphNode>> tree = new List<List<GraphNode>>();
        for (int i = 0; i < cells.Count; ++i)
        {
            tree.Add(new List<GraphNode>());
        }

        List<GraphNode> remaining = new List<GraphNode>();
        bool[] addedToRemaining = new bool[cells.Count];
        bool[] inTree = new bool[cells.Count];

        int startNodeIndex;
        do
        {
            startNodeIndex = UnityEngine.Random.Range(0, cells.Count - 1);
        } while (cells[startNodeIndex].obstacle);

        addedToRemaining[startNodeIndex] = true;
        inTree[startNodeIndex] = true;

        List<GraphNode> startNeighbours = graph[startNodeIndex];
        foreach (GraphNode neighbour in startNeighbours)
        {
            if (!addedToRemaining[neighbour.i * gridNoZ + neighbour.j])
            {
                remaining.Add(neighbour);
                addedToRemaining[neighbour.i * gridNoZ + neighbour.j] = true;
            }
        }

        while (remaining.Count > 0)
        {
            //Pick random node to add to the tree
            int randomIndex = UnityEngine.Random.Range(0, remaining.Count - 1);
            GraphNode node = remaining[randomIndex];
            remaining.RemoveAt(randomIndex);

            List<GraphNode> neighbours = graph[node.i * gridNoZ + node.j];
            bool addedToTree = false;
            foreach(GraphNode neighbour in neighbours)
            {
                //Find node in tree that this node is connected to
                if(inTree[neighbour.i * gridNoZ + neighbour.j] && !addedToTree)
                {
                    //Add this node to the tree
                    addedToTree = true;
                    inTree[node.i * gridNoZ + node.j] = true;

                    tree[node.i * gridNoZ + node.j].Add(neighbour);
                    tree[neighbour.i * gridNoZ + neighbour.j].Add(node);
                }

                //Add its neighbours to the list
                if (!addedToRemaining[neighbour.i * gridNoZ + neighbour.j])
                {
                    remaining.Add(neighbour);
                    addedToRemaining[neighbour.i * gridNoZ + neighbour.j] = true;
                }
            }
        }

        return tree;
    }

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying)
        {
            return;
        }

        /*Gizmos.color = Color.blue;
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
        }*/
        Gizmos.color = Color.white;
        for (int i = 0; i < mst.Count; ++i)
        {

            float x = terrain_manager.myInfo.get_x_pos(cells[i].i);
            float z = terrain_manager.myInfo.get_z_pos(cells[i].j);

            for (int j = 0; j < mst[i].Count; ++j)
            {
                float nX = terrain_manager.myInfo.get_x_pos(mst[i][j].i);
                float nZ = terrain_manager.myInfo.get_z_pos(mst[i][j].j);
                Gizmos.DrawLine(new Vector3(x, 1, z), new Vector3(nX, 1, nZ));
            }
        }
    }
}
