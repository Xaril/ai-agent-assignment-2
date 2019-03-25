using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreateMST : MonoBehaviour
{
    public List<List<GraphNode>> graph;
    public List<List<int>> graph_indeces;
    public List<GraphNode> cells;
    public List<List<GraphNode>>[] MSTs;

    TreeST[] treeSTs;
    public List<Vector3>[] paths;

    private int n_robots;
    private int gridNoX;
    private int gridNoZ;
    private int numberOfGridCells;
    private int[] pathOf;
    public int[][] cost_matrix;

    public GameObject terrain_manager_game_object;
    TerrainManager terrain_manager;

    // Start is called before the first frame update
    void Start()
    {
        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();

        graph = new List<List<GraphNode>>();
        graph_indeces = new List<List<int>>();
        cells = new List<GraphNode>();
        gridNoX = terrain_manager.myInfo.x_N;
        gridNoZ = terrain_manager.myInfo.z_N;
        this.numberOfGridCells = gridNoX * gridNoZ;
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
                    graph_indeces.Add(new List<int>());
                }
                else
                {
                    List<GraphNode> neighbours = FindCellNeighbours(i, j);
                    graph.Add(neighbours);
                    List<int> neighbours_indeces = new List<int>();
                    foreach (var neighbor in neighbours)
                    {
                        neighbours_indeces.Add(neighbor.ij);
                    }
                    graph_indeces.Add(neighbours_indeces);
                }
            }
        }
        InitCostMatrix();
        MSTs = MSTC();
        PrintPathOptimalityEvaluation();
        //PostProcessTrees();
        FindPaths();
    }

    private void PrintPathOptimalityEvaluation()
    {
        int[] counter = new int[n_robots];
        for (int i = 0; i < cells.Count; i++)
        {
            if (pathOf[i] == -1)
            {
                continue;
            }
            counter[pathOf[i]]++;
        }
        for (int i = 0; i < n_robots; i++)
        {
            Debug.Log(string.Format("Car: {0}\t Path length: {1}", i, counter[i]));
        }
    }

    private void PostProcessTrees()
    {
        int n_iterations = 3;
        int robot;

        for (int iter = 0; iter < n_iterations; iter++)
        {
            GraphNode[] best_nodes = new GraphNode[cells.Count];
            for (int cell = 0; cell < cells.Count; cell++)
            {
                if (cells[cell].obstacle)
                {
                    continue;
                }

                if (IsLeaf(cell))
                {
                    robot = pathOf[cell];

                    best_nodes[cell] = TryNewConnection(cell, robot);

                    //Debug.Log(string.Format("Cell {0} is leaf. ({1}, {2})",
                    //    cell, cells[cell].i, cells[cell].j));
                }
            }

            for (int cell = 0; cell < cells.Count; cell++)
            {
                robot = pathOf[cell];
                if (best_nodes[cell] == null)
                {
                    continue;
                }

                if (BreaksTree(cell, best_nodes[cell]))
                {
                    continue;
                }

                // Remove previous connection
                GraphNode previous_node = MSTs[robot][cell][0];
                MSTs[robot][cell].Remove(previous_node);
                MSTs[robot][previous_node.ij].Remove(cells[cell]);
                // Connect to the new best found node
                MSTs[robot][cell].Add(best_nodes[cell]);
                MSTs[robot][best_nodes[cell].ij].Add(cells[cell]);
            }

        }
    }

    private bool BreaksTree(int cell, GraphNode graphNode)
    {
        int robot = pathOf[cell];
        GraphNode prev = cells[cell];
        GraphNode current = graphNode;
        int i = 0;

        GraphNode previous_node = MSTs[robot][cell][0];
        
        MSTs[robot][cell].Remove(previous_node);
        MSTs[robot][previous_node.ij].Remove(cells[cell]);
        // Connect to the new best found node
        MSTs[robot][cell].Add(graphNode);
        MSTs[robot][graphNode.ij].Add(cells[cell]);

        while (MSTs[robot][current.ij].Count == 2)
        {
            i++;
            // Select next, different from current
            if (MSTs[robot][current.ij][0].Equals(prev))
            {
                prev = current;
                current = MSTs[robot][current.ij][1];
            }
            else
            {
                prev = current;
                current = MSTs[robot][current.ij][0];
            }
            if (i > 100)
            {
                // Reverse
                MSTs[robot][cell].Add(previous_node);
                MSTs[robot][previous_node.ij].Add(cells[cell]);
                MSTs[robot][cell].Remove(graphNode);
                MSTs[robot][graphNode.ij].Remove(cells[cell]);
                Debug.Assert(MSTs[robot][cell].Count > 0, string.Format("Cell {0} is not connected", cell));
                return true;
            }
        }
        MSTs[robot][cell].Add(previous_node);
        MSTs[robot][previous_node.ij].Add(cells[cell]);
        MSTs[robot][cell].Remove(graphNode);
        MSTs[robot][graphNode.ij].Remove(cells[cell]);
        Debug.Assert(MSTs[robot][cell].Count > 0, string.Format("Cell {0} is not connected", cell));
        return false;
    }

    private GraphNode TryNewConnection(int cell, int robot)
    {
        // TODO: connect with node that has longest single path
        int n_connections;
        GraphNode current_connected_node = MSTs[robot][cell][0];
        GraphNode best_neighbor = null;
        GraphNode node = cells[cell];
        int lowest_n_connections = MSTs[robot][current_connected_node.ij].Count-1;
        // Check neighbors belonging to same path
        foreach (var neighbor in graph[cell])
        {
            if (pathOf[neighbor.ij] != robot)
            {
                continue;
            }
            n_connections = MSTs[robot][neighbor.ij].Count;
            if (n_connections < lowest_n_connections)
            {
                lowest_n_connections = n_connections;
                best_neighbor = neighbor;
            }
        }

        //if (best_neighbor == null)
        //{
        //    return;
        //}

        //// Remove previous connection
        //MSTs[robot][cell].Remove(current_connected_node);
        //MSTs[robot][current_connected_node.ij].Remove(node);
        //// Connect to the new best found node
        //MSTs[robot][cell].Add(best_neighbor);
        //MSTs[robot][best_neighbor.ij].Add(node);
        return best_neighbor;
    }

    private bool IsLeaf(int cell)
    {
        return MSTs[pathOf[cell]][cell].Count == 1;
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

    public List<List<GraphNode>>[] MSTC()
    {
        // Initializiation
        pathOf = Enumerable.Repeat(-1, cells.Count).ToArray(); // Indicates to which robot's path a cell belongs to

        List<int[]> starting_positions = initStartingPositions();
        //List<int[]> starting_positions = new List<int[]> {
        //    new int[] {8, 9},
        //    new int[] {8, 10},
        //    new int[] {8, 11}
        //};

        // Returned value
        List<List<GraphNode>>[] all_trees = new List<List<GraphNode>>[n_robots];
        for (int robot = 0; robot < n_robots; robot++)
        {
            all_trees[robot] = new List<List<GraphNode>>();
            for (int i = 0; i < cells.Count; ++i)
            {
                all_trees[robot].Add(new List<GraphNode>());
            }
        }

        
        List<List<GraphNode>> remaining_for_robot = new List<List<GraphNode>>();
        bool[][] addedToRemaining = new bool[n_robots][];

        for (int robot = 0; robot < n_robots; robot++)
        {
            addedToRemaining[robot] = new bool[cells.Count];
            remaining_for_robot.Add(new List<GraphNode>());
        }

        for (int robot = 0; robot < n_robots; robot++)
        {
            Debug.Log(string.Format("Robot: {0}, pos: ({1}, {2})", robot,  starting_positions[robot][0], starting_positions[robot][1]));
            GraphNode starting_node = GetGraphNode(starting_positions[robot][0], starting_positions[robot][1]);

            pathOf[Get1Dindex(starting_positions[robot][0], starting_positions[robot][1])] = robot;

            addNeighbours(starting_node, addedToRemaining, remaining_for_robot, robot);

            for (int i = 0; i < n_robots; i++)
            {
                addedToRemaining[i][starting_node.ij] = true;
                remaining_for_robot[i].Remove(starting_node);
            }
        }
        // End initialization

        // Computes the a tree for each robot
        while (!AllAreEmpty(remaining_for_robot))
        {
            for (int robot = 0; robot < n_robots; robot++)
            {
                if (remaining_for_robot[robot].Count == 0)
                {
                    // Tree cannot be expanded anymore
                    continue;
                }
                Debug.Log("Expanding bot " + robot);
                // For each remaining cell, pick the one farthest from other robots'paths
                float best_min_dist = float.NegativeInfinity; // The bigger, the better
                GraphNode best_node = null;
                foreach (var remaining_node in remaining_for_robot[robot])
                {
                    float min_dist = ComputeMinBFSDistance(remaining_node.ij, robot);
                    //float min_dist = ComputeMinManhattanDistance(remaining_node, robot);
                    if (min_dist > best_min_dist)
                    {
                        best_min_dist = min_dist;
                        best_node = remaining_node;
                    }
                }
                // Remove this best candidate from all the remaining lists and block it
                for (int i = 0; i < n_robots; i++)
                {
                    remaining_for_robot[i].Remove(best_node);
                    addedToRemaining[i][best_node.ij] = true;
                }
                addNeighbours(best_node, addedToRemaining, remaining_for_robot, robot);
                pathOf[best_node.ij] = robot;
                // Find parent
                GraphNode parent = FindParent(best_node, robot);
                all_trees[robot][best_node.ij].Add(parent);
                all_trees[robot][parent.ij].Add(best_node);
            }
        }

        return all_trees;
    }

    private List<int[]> initStartingPositions()
    {
        List<int[]> initialPositions = new List<int[]>();
        int first_point= -1, second_point= -1, third_point = -1;
        // 1st point:
        // Take the point most distant from all the others
        int max_sum = int.MinValue;
        for (int row = 0; row < cost_matrix.Length; row++)
        {
            if (cells[row].obstacle) continue;
     
            int sum = cost_matrix[row].Sum();
            if (max_sum < sum)
            {
                max_sum = sum;
                first_point = row;
            }
        }
        initialPositions.Add(new int[] { cells[first_point].i , cells[first_point].j  });

        // 2nd point:
        // Take the point farthest from the 1st point
        int max_distance = int.MinValue;
        for (int col = 0; col < cost_matrix.Length; col++)
        {
            if (col == first_point) continue;
            if (cells[col].obstacle) continue;

            int distance = cost_matrix[first_point][col];
            if (max_distance < distance)
            {
                max_distance = distance;
                second_point = col;
            }
        }
        initialPositions.Add(new int[] { cells[second_point].i, cells[second_point].j});

        // 3rd point:
        // Take the point farthest from the 1st point and the second point
        max_distance = int.MinValue;
        for (int row = 0; row < cost_matrix.Length; row++)
        {
            if (cells[row].obstacle) continue;
            if (row == first_point || row == second_point) continue;
            int distance = cost_matrix[row][first_point] + cost_matrix[row][second_point];

            if (max_distance < distance)
            {
                max_distance = distance;
                third_point = row;
            }
            initialPositions.Add(new int[] { cells[third_point].i, cells[third_point].j});

        }

        return initialPositions;
    }

    private bool AllAreEmpty(List<List<GraphNode>> remaining_for_robot)
    {
        foreach (var list_nodes in remaining_for_robot)
        {
            if (list_nodes.Count > 0)
            {
                return false;
            }
        }
        return true;
    }

    private GraphNode FindParent(GraphNode node, int robot)
    {
        List<GraphNode> neighbours = graph[node.ij];

        foreach (var neighbor in neighbours)
        {
            if (pathOf[neighbor.ij] == robot)
            {
                return neighbor;
            }
        }
        Debug.Log("This should never happen D: Check FindParent()");
        return null;
    }

    private int ComputeMinBFSDistance(int remaining_node, int robot)
    {
        // Initialization
        Queue<int> queue = new Queue<int>();
        int[] distances = Enumerable.Repeat(-1, cells.Count).ToArray();

        distances[remaining_node] = 0;
        queue.Enqueue(remaining_node);

        while (queue.Count != 0)
        {
            int node = queue.Dequeue();

            // If node belongs to another robot's path, return the distance
            if (pathOf[node] != robot && pathOf[node] != -1)
            {
                return distances[node];
            }

            foreach (int neighbor_index in graph_indeces[node])
            {
                if (distances[neighbor_index] != -1)
                {
                    continue;
                }
                distances[neighbor_index] = distances[node] + 1;
                queue.Enqueue(neighbor_index);
            }
        }

        Debug.LogError("Oh no! This should never happen! D:\t Check BFS search");
        return int.MaxValue;
    }

    private int FindClosestFreePosition(int very_initial_position, int robot)
    {
        // Initialization
        Queue<int> queue = new Queue<int>();
        int[] distances = Enumerable.Repeat(-1, cells.Count).ToArray();
        List<int> possible_initial_positions = new List<int>();

        distances[very_initial_position] = 0;
        queue.Enqueue(very_initial_position);

        while (queue.Count != 0)
        {
            int node = queue.Dequeue();

            // If cell is free, add to the list of possible initial positions
            if (pathOf[node] == -1)
            {
                possible_initial_positions.Add(node);
            }

            if(possible_initial_positions.Count >= 50)
            {
                float min_dist = float.MaxValue;
                int best_pos = -1;
                foreach (int pos in possible_initial_positions)
                {
                    int i = cells[pos].i, j = cells[pos].j;
                    int v_i = cells[very_initial_position].i, v_j = cells[very_initial_position].j;

                    float distance = Vector2.Distance(
                        new Vector2(i, j),
                        new Vector2(v_i, v_j));
                    if (distance < min_dist)
                    {
                        min_dist = distance;
                        best_pos = pos;
                    }
                }
                return best_pos;
            }

            foreach (int neighbor_index in graph_indeces[node])
            {
                if (distances[neighbor_index] != -1)
                {
                    continue;
                }
                distances[neighbor_index] = distances[node] + 1;
                queue.Enqueue(neighbor_index);
            }
        }

        Debug.LogError("Oh no! This should never happen! D:\t Check BFS search");
        return int.MaxValue;
    }

    private int ComputeMinManhattanDistance(GraphNode remaining_node, int robot)
    {
        int i = remaining_node.i, j = remaining_node.j;
        int i_check, j_check, ij_check;
        
        for (int manhattan_radius = 0; manhattan_radius < gridNoZ+gridNoX; manhattan_radius++)
        {
            for (int k = 0; k <= manhattan_radius; k++)
            {
                i_check = i - manhattan_radius;
                j_check = j - manhattan_radius + k;
                ij_check = Get1Dindex(i_check, j_check);
                if (IsValid(i_check, j_check))
                {
                    if(pathOf[ij_check] != -1 && pathOf[ij_check] != robot)
                    {
                        //Debug.Log(string.Format("Robot: {0}\ti: {1}\tj: {2}",
                        //    robot, i_check, j_check));
                        return manhattan_radius;
                    }
                }
                i_check = i - manhattan_radius + k;
                j_check = j + manhattan_radius;
                ij_check = Get1Dindex(i_check, j_check);
                if (IsValid(i_check, j_check))
                {
                    if (pathOf[ij_check] != -1 && pathOf[ij_check] != robot)
                    {
                        //Debug.Log(string.Format("Robot: {0}\ti: {1}\tj: {2}",
                        //     robot, i_check, j_check));
                        return manhattan_radius;
                    }
                }
                i_check = i + manhattan_radius;
                j_check = j + manhattan_radius - k;
                ij_check = Get1Dindex(i_check, j_check);
                if (IsValid(i_check, j_check))
                {
                    if (pathOf[ij_check] != -1 && pathOf[ij_check] != robot)
                    {
                        //Debug.Log(string.Format("Robot: {0}\ti: {1}\tj: {2}",
                        //     robot, i_check, j_check));
                        return manhattan_radius;
                    }
                }
                i_check = i + manhattan_radius - k;
                j_check = j - manhattan_radius;
                ij_check = Get1Dindex(i_check, j_check);
                if (IsValid(i_check, j_check))
                {
                    if (pathOf[ij_check] != -1 && pathOf[ij_check] != robot)
                    {
                        //Debug.Log(string.Format("Robot: {0}\ti: {1}\tj: {2}",
                        //     robot, i_check, j_check));
                        return manhattan_radius;
                    }
                }
            }
        }
        // This should never be executed
        Debug.Log("Oh No! Your code it's not working properly! :(");

        return -1; 
    }

    private void addNeighbours(GraphNode node, bool[][] addedToRemaining, List<List<GraphNode>> remaining_for_robot, int robot)
    {
        List<GraphNode> neighbours = graph[node.ij];
        foreach (var neighbor in neighbours)
        {
            if (!addedToRemaining[robot][neighbor.ij] && pathOf[neighbor.ij] == -1)
            {
                remaining_for_robot[robot].Add(neighbor);
                addedToRemaining[robot][neighbor.ij] = true;
            }
        }
    }
    
    private int Get1Dindex(int i, int j)
    {
        return i * gridNoZ + j;
    }

    private GraphNode GetGraphNode(int i, int j)
    {
        return cells[Get1Dindex(i,j)];
    }
    


    private bool IsValid(int i, int j)
    {
        if (i < 0 || i >= gridNoZ || j < 0 || j >= gridNoX)
        {
            return false;
        }
        if (IsObstacle(i, j))
        {
            return false;
        }

        return true;
    }

    private bool IsObstacle(int i, int j)
    {
        return terrain_manager.myInfo.traversability[i, j] > 0.5f;
    }

    public void FindPaths()
    {
        paths = new List<Vector3>[n_robots];
        Vector3[,] grid = new Vector3[2 * gridNoX, 2 * gridNoZ];
        float gridLength = (terrain_manager.myInfo.x_high - terrain_manager.myInfo.x_low) / terrain_manager.myInfo.x_N;
        bool[,] visited = new bool[2 * gridNoX, 2 * gridNoZ];
        for (int i = 0; i < gridNoX; ++i)
        {
            for (int j = 0; j < gridNoZ; ++j)
            {
                Vector3 bigPosition = new Vector3(
                    terrain_manager.myInfo.get_x_pos(i),
                    0,
                    terrain_manager.myInfo.get_z_pos(j)
                );
                grid[2 * i, 2 * j] = bigPosition + new Vector3(
                    -gridLength / 4,
                    0,
                    -gridLength / 4
                );
                grid[2 * i + 1, 2 * j] = bigPosition + new Vector3(
                    gridLength / 4,
                    0,
                    -gridLength / 4
                );
                grid[2 * i, 2 * j + 1] = bigPosition + new Vector3(
                    -gridLength / 4,
                    0,
                    gridLength / 4
                );
                grid[2 * i + 1, 2 * j + 1] = bigPosition + new Vector3(
                    gridLength / 4,
                    0,
                    gridLength / 4
                );
                if (terrain_manager.myInfo.traversability[i, j] > 0.5f)
                {
                    visited[2 * i, 2 * j] = true;
                    visited[2 * i + 1, 2 * j] = true;
                    visited[2 * i, 2 * j + 1] = true;
                    visited[2 * i + 1, 2 * j + 1] = true;
                }
            }
        }


        for (int i = 0; i < n_robots; ++i)
        {
            int checkedNodes = 0;
            int numberOfNodesToCheck = 0;
            for(int j = 0; j < MSTs[i].Count; ++j)
            {
                if(MSTs[i][j].Count > 0)
                {
                    numberOfNodesToCheck+=4;
                }
            }

            paths[i] = new List<Vector3>();
            int cellI = 0;
            int cellJ = 0;
            do
            {
                cellI = UnityEngine.Random.Range(0, 2 * gridNoX - 1);
                cellJ = UnityEngine.Random.Range(0, 2 * gridNoZ - 1);
            } while (/*visited[cellI, cellJ] &&*/ pathOf[Get1Dindex(cellI/2, cellJ/2)] != i);

            while (checkedNodes < numberOfNodesToCheck)
            {
                paths[i].Add(grid[cellI, cellJ]);
                checkedNodes++;
                visited[cellI, cellJ] = true;

                int bigGridI = cellI / 2;
                int bigGridJ = cellJ / 2;

                if (cellI % 2 == 0 && cellJ % 2 == 0)
                {
                    bool foundWall = false;
                    foreach (GraphNode node in MSTs[i][Get1Dindex(bigGridI, bigGridJ)])
                    {
                        if (node.i < bigGridI)
                        {
                            cellI--;
                            foundWall = true;
                            break;
                        }
                    }
                    if (!foundWall)
                    {
                        cellJ++;
                    }
                }
                else if (cellI % 2 == 0 && cellJ % 2 == 1)
                {
                    bool foundWall = false;
                    foreach (GraphNode node in MSTs[i][Get1Dindex(bigGridI, bigGridJ)])
                    {
                        if (node.j > bigGridJ)
                        {
                            cellJ++;
                            foundWall = true;
                            break;
                        }
                    }
                    if (!foundWall)
                    {
                        cellI++;
                    }
                }
                else if (cellI % 2 == 1 && cellJ % 2 == 1)
                {
                    bool foundWall = false;
                    foreach (GraphNode node in MSTs[i][Get1Dindex(bigGridI, bigGridJ)])
                    {
                        if (node.i > bigGridI)
                        {
                            cellI++;
                            foundWall = true;
                            break;
                        }
                    }
                    if (!foundWall)
                    {
                        cellJ--;
                    }
                }
                else if (cellI % 2 == 1 && cellJ % 2 == 0)
                {
                    bool foundWall = false;
                    foreach (GraphNode node in MSTs[i][Get1Dindex(bigGridI, bigGridJ)])
                    {
                        if (node.j < bigGridJ)
                        {
                            cellJ--;
                            foundWall = true;
                            break;
                        }
                    }
                    if (!foundWall)
                    {
                        cellI--;
                    }
                }
            }
        }
    }

    private void InitCostMatrix()
    {
        cost_matrix = new int[numberOfGridCells][];

        for (int starting_node = 0; starting_node < numberOfGridCells; starting_node++)
        {
            int i_starting_node = starting_node / gridNoZ;
            int j_starting_node = starting_node % gridNoZ;

            // Initialization
            cost_matrix[starting_node] = Enumerable.Repeat(-1, numberOfGridCells).ToArray();
            Queue<int> queue = new Queue<int>();

            cost_matrix[starting_node][starting_node] = 0;
            queue.Enqueue(starting_node);

            while (queue.Count != 0)
            {
                int node = queue.Dequeue();

                foreach (int neighbor_index in graph_indeces[node])
                {
                    if (cost_matrix[starting_node][neighbor_index] != -1)
                    {
                        continue;
                    }
                    cost_matrix[starting_node][neighbor_index] = cost_matrix[starting_node][node] + 1;
                    queue.Enqueue(neighbor_index);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying)
        {
            return;
        }

        Color[] robot_colors = new Color[]
        {
            Color.red, Color.blue, Color.black
        };
        
        for(int k = 0; k < MSTs.Length; ++k)
        {
            Gizmos.color = robot_colors[k];
            for (int i = 0; i < MSTs[k].Count; ++i)
            {

                float x = terrain_manager.myInfo.get_x_pos(cells[i].i);
                float z = terrain_manager.myInfo.get_z_pos(cells[i].j);

                if (i == 0)
                {
                    Gizmos.DrawCube(new Vector3(x, 1, z), Vector3.one * 25);
                }

                for (int j = 0; j < MSTs[k][i].Count; ++j)
                {
                    float nX = terrain_manager.myInfo.get_x_pos(MSTs[k][i][j].i);
                    float nZ = terrain_manager.myInfo.get_z_pos(MSTs[k][i][j].j);
                    Gizmos.DrawLine(new Vector3(x, 1, z), new Vector3(nX, 1, nZ));
                }
            }
        }
    }
}
