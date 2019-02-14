using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMST : MonoBehaviour
{
    public List<List<GraphNode>> graph;
    public List<GraphNode> cells;
    public List<List<GraphNode>> mst;

    TreeST[] treeSTs;

    private int n_robots;
    private int gridNoX;
    private int gridNoZ;
    private int[] pathOf;

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
        n_robots = GameObject.FindGameObjectsWithTag("Player").Length;
        for (int i = 0; i < gridNoX; ++i)
        {
            for (int j = 0; j < gridNoZ; ++j)
            {
                cells.Add(new GraphNode(i, j, terrain_manager.myInfo.traversability[i, j] > 0.5f, gridNoZ));
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
        MSTC();
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
            if (!addedToRemaining[neighbour.ij])
            {
                remaining.Add(neighbour);
                addedToRemaining[neighbour.ij] = true;
            }
        }

        while (remaining.Count > 0)
        {
            //Pick random node to add to the tree
            int randomIndex = UnityEngine.Random.Range(0, remaining.Count - 1);
            GraphNode node = remaining[randomIndex];
            remaining.RemoveAt(randomIndex);

            List<GraphNode> neighbours = graph[node.ij];
            bool addedToTree = false;
            foreach(GraphNode neighbour in neighbours)
            {
                //Find node in tree that this node is connected to
                if(inTree[neighbour.ij] && !addedToTree)
                {
                    //Add this node to the tree
                    addedToTree = true;
                    inTree[node.ij] = true;

                    tree[node.ij].Add(neighbour);
                    tree[neighbour.ij].Add(node);
                }

                //Add its neighbours to the list
                if (!addedToRemaining[neighbour.ij])
                {
                    remaining.Add(neighbour);
                    addedToRemaining[neighbour.ij] = true;
                }
            }
        }

        return tree;
    }

    public void MSTC() // Work in progress
    {
        // Initializiation
        List<int[]> starting_positions = new List<int[]> {
            new int[] {8, 9},
            new int[] {8, 10},
            new int[] {8, 11}
        };
     
        pathOf = new int[cells.Count]; // Indicates to which robot's path a cell belongs to
        for (int i = 0; i < pathOf.Length; i++)
        {
            pathOf[i] = -1;
        }
        
        treeSTs = new TreeST[n_robots];

        for (int i = 0; i < n_robots; i++)
        {
            GraphNode starting_node = GetGraphNode(starting_positions[i][0], starting_positions[i][1]);
            treeSTs[i] = new TreeST(new NodeST(starting_node));
            pathOf[Get1Dindex(starting_positions[i][0], starting_positions[i][1])] = i;
        }
        // End initialization

        //for (int i = 0; i < n_robots; i++)
        //{
            
        //}

    }

    private int Get1Dindex(int i, int j)
    {
        return i * gridNoZ + j;
    }

    private GraphNode GetGraphNode(int i, int j)
    {
        return cells[Get1Dindex(i,j)];
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

        Color[] robot_colors = new Color[]
        {
            Color.red, Color.blue, Color.yellow
        };

        // Printing robots'paths (hardcoded for 3 robots)
        for (int cell = 0; cell < pathOf.Length; cell++)
        {
            if(pathOf[cell] == -1)
            {
                continue;
            }

            float x = terrain_manager.myInfo.get_x_pos(cells[cell].i);
            float z = terrain_manager.myInfo.get_z_pos(cells[cell].j);

            Gizmos.color = robot_colors[pathOf[cell]];

            Gizmos.DrawCube(new Vector3(x, 2f, z), new Vector3(2f, 2f, 2f));

        }

    }
}
