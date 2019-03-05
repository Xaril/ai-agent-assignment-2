using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CreateMSC : MonoBehaviour
{
    public GameObject terrain_manager_game_object;
    TerrainManager terrain_manager;
    private int gridNoX;
    private int gridNoZ;
    private CoverNode[,] grid;
    public List<CoverNode> coverNodes;
    public List<CoverNode> completeCoverNodes; // Includes initial node
    private int numberOfObstacles;
    private int numberOfGridCells;
    public List<GameObject> cars;

    public List<List<int>> graph_indeces;
    public int[][] cost_matrix;

    public List<List<int>> cost_matrix_guarded;
    public List<List<CoverNode>> mTSP_solution;

    // Start is called before the first frame update
    void Start()
    {
        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
        gridNoX = terrain_manager.myInfo.x_N;
        gridNoZ = terrain_manager.myInfo.z_N;
        grid = new CoverNode[gridNoX, gridNoZ];
        graph_indeces = new List<List<int>>();
        coverNodes = new List<CoverNode>();
        this.numberOfGridCells = gridNoX * gridNoZ;
        this.numberOfObstacles = 0;
        for(int i = 0; i < gridNoX; ++i)
        {
            for(int j = 0; j < gridNoZ; ++j)
            {
                bool isObstacle = terrain_manager.myInfo.traversability[i, j] > 0.5f;
                if (isObstacle)
                {
                    numberOfObstacles++;
                    graph_indeces.Add(new List<int>());
                }
                else
                {
                    List<int> neighbours = GetNeighbours(i, j);
                    graph_indeces.Add(neighbours);
                }
                grid[i, j] = new CoverNode(new Vector3(terrain_manager.myInfo.get_x_pos(i), 0.5f, terrain_manager.myInfo.get_z_pos(j)), isObstacle, Get1Dindex(i,j), i, j);
            }
        }
        InitCostMatrix();
        createMSC();
        SaveProblemCostMatrixToFile();
        //string python_result = SolveWithPython();
        //Debug.LogError(string.Format("Python output: {0}", python_result));
        ReadSolutionFile();
    }

    private void ReadSolutionFile()
    {
        Debug.Log("Reading python solution");
        mTSP_solution = new List<List<CoverNode>>();

        var lines = File.ReadAllLines("python_stuff/result_P2.txt");
        foreach (var line in lines)
        {
            List<CoverNode> row_to_add = new List<CoverNode>();
            List<int> TagIds = line.Split(',').Select(int.Parse).ToList();
            foreach (var node in TagIds)
            {
                row_to_add.Add(completeCoverNodes[node]);
            }
            mTSP_solution.Add(row_to_add);
        }
    }

    private string SolveWithPython()
    {
        // full path of python interpreter 
        string python = "C:\\Users\\Lenovo\\AppData\\Local\\Programs\\Python\\Python37\\python.exe";

        // python app to call 
        string myPythonApp = "run.py";

        // Create new process start info 
        System.Diagnostics.ProcessStartInfo myProcessStartInfo = new System.Diagnostics.ProcessStartInfo(python);
        myProcessStartInfo.WorkingDirectory = Directory.GetCurrentDirectory() + "\\python_stuff";

        // make sure we can read the output from stdout 
        myProcessStartInfo.UseShellExecute = false;
        myProcessStartInfo.RedirectStandardOutput = true;

        // start python app with 2 argument
        int m = cars.Count;
        myProcessStartInfo.Arguments = myPythonApp + " " + m;

        System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
        // assign start information to the process 
        myProcess.StartInfo = myProcessStartInfo;

        // start the process 
        myProcess.Start();

        // Read the standard output of the app we called.  
        // in order to avoid deadlock we will read output first 
        // and then wait for process terminate: 
        StreamReader myStreamReader = myProcess.StandardOutput;
        string myString = myStreamReader.ReadToEnd();

        // wait exit signal from the app we called and then close it. 
        myProcess.WaitForExit();
        myProcess.Close();

        return myString;
    }

    private List<int> GetNeighbours(int i, int j)
    {
        List<int> neighbours = new List<int>();
        if (i > 0)
        {
            bool isObstacle = terrain_manager.myInfo.traversability[i-1, j] > 0.5f;
            if (!isObstacle)
            {
                neighbours.Add(Get1Dindex(i - 1, j));
            }
        }
        if (i < gridNoX - 1)
        {
            bool isObstacle = terrain_manager.myInfo.traversability[i + 1, j] > 0.5f;
            if (!isObstacle)
            {
                neighbours.Add(Get1Dindex(i + 1, j));
            }
        }
        if (j > 0)
        {
            bool isObstacle = terrain_manager.myInfo.traversability[i, j - 1] > 0.5f;
            if (!isObstacle)
            {
                neighbours.Add(Get1Dindex(i, j - 1));
            }
        }
        if (j < gridNoZ - 1)
        {
            bool isObstacle = terrain_manager.myInfo.traversability[i, j + 1] > 0.5f;
            if (!isObstacle)
            {
                neighbours.Add(Get1Dindex(i, j + 1));
            }
        }

        return neighbours;
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

    public List<Vector3>[] GetPaths()
    {
        //List<Vector3> positions = new List<Vector3>();
        //foreach(CoverNode node in coverNodes)
        //{
        //    positions.Add(node.position);
        //}
        //List<Vector3>[] paths = TSP.GenerateCarPaths(TSP.GreedyPath(positions));
        List<Vector3>[] paths = new List<Vector3>[cars.Count];
        for (int i = 0; i < cars.Count; i++)
        {
            List<Vector3> car_path_v3 = new List<Vector3>();
            foreach (var node in mTSP_solution[i])
            {
                car_path_v3.Add(node.position);
            }
            paths[i] = car_path_v3;
        }
        return paths;
    }

    private void SaveProblemCostMatrixToFile()
    {
        completeCoverNodes = new List<CoverNode>();
        int s = terrain_manager.myInfo.get_i_index(cars[1].transform.position.x);
        int t = terrain_manager.myInfo.get_j_index(cars[1].transform.position.z);

        cost_matrix_guarded = new List<List<int>>();
        completeCoverNodes.Add(grid[s, t]);
        completeCoverNodes.AddRange(coverNodes);
        for (int i = 0; i < completeCoverNodes.Count; i++)
        {
            List<int> row_to_add = new List<int>();
            for (int j = 0; j < completeCoverNodes.Count; j++)
            {
                row_to_add.Add(cost_matrix[completeCoverNodes[i].ij][completeCoverNodes[j].ij]);
            }
            cost_matrix_guarded.Add(row_to_add);
        }

        using (TextWriter tw = new StreamWriter("python_stuff/cost_matrix.txt"))
        {
            for (int j = 0; j < cost_matrix_guarded[0].Count; j++)
            {
                for (int i = 0; i < cost_matrix_guarded.Count; i++)
                {
                    tw.Write(cost_matrix_guarded[i][j]);
                    if (i != cost_matrix_guarded[0].Count-1)
                    {
                        tw.Write(",");
                    }
                }
                tw.WriteLine();
            }
        }

    }


    // New version
    private void createMSC()
    {
        // Mark all nodes that are covered at the start
        int numberOfNodesCoveredAtStart = 0;
        foreach (GameObject car in cars)
        {
            int i = terrain_manager.myInfo.get_i_index(car.transform.position.x);
            int j = terrain_manager.myInfo.get_j_index(car.transform.position.z);
            CoverNode carNode = grid[i, j];
            //if (numberOfNodesCoveredAtStart > 0 && coverNodes.Count == 0)
            //{
            //    coverNodes.Add(carNode);
            //}
            for (int s = 0; s < gridNoX; ++s)
            {
                for (int t = 0; t < gridNoZ; ++t)
                {
                    if (grid[s, t].isCovered || grid[s, t].isObstacle) continue;
                    if (grid[s, t].IsCoveredBy(carNode))
                    {
                        grid[s, t].isCovered = true;
                        numberOfNodesCoveredAtStart++;
                    }
                }
            }
        }

        // Greedy algorithm
        int cellsCovered = numberOfObstacles + numberOfNodesCoveredAtStart;
        while (cellsCovered < numberOfGridCells)
        {
            int maxNumberCovered = 0;
            Vector2 maxCoverNodeIndex = new Vector2();
            List<Vector2> equally_covering_nodes = new List<Vector2>();
            // Find the node that covers the most of other uncovered nodes
            // And that is closest to existing covering nodes
            for (int i = 0; i < gridNoX; ++i)
            {
                for (int j = 0; j < gridNoZ; ++j)
                {
                    if (grid[i, j].isObstacle) continue;
                    if (!grid[i, j].isCovered)
                    {
                        continue;
                    }
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
                    // If the node is equally good to the bests found so far, add to the list
                    if (numberCovered == maxNumberCovered)
                    {
                        maxCoverNodeIndex = new Vector2(i, j);
                        equally_covering_nodes.Add(maxCoverNodeIndex);
                    }
                    // Otherwise, empty the list and add this new best node
                    if (numberCovered > maxNumberCovered)
                    {
                        maxNumberCovered = numberCovered;
                        maxCoverNodeIndex = new Vector2(i, j);
                        equally_covering_nodes.Clear();
                        equally_covering_nodes.Add(maxCoverNodeIndex);
                    }
                }
            }

            // Find the best node from the list of those found previously
            if (coverNodes.Count > 0)
            {
                int min_dist = int.MaxValue, dist;
                for (int i = 0; i < equally_covering_nodes.Count; i++)
                {
                    int index_node = (int)(equally_covering_nodes[i].x * gridNoZ + equally_covering_nodes[i].y);
                    foreach (var covering_node in coverNodes)
                    {
                        dist = cost_matrix[covering_node.ij][index_node];
                        if (dist < min_dist)
                        {
                            min_dist = dist;
                            maxCoverNodeIndex = equally_covering_nodes[i];
                        }
                    }
                }
            }


            cellsCovered += maxNumberCovered;
            coverNodes.Add(grid[(int)maxCoverNodeIndex.x, (int)maxCoverNodeIndex.y]);
            for (int i = 0; i < gridNoX; ++i)
            {
                for (int j = 0; j < gridNoZ; ++j)
                {
                    if (grid[i, j].IsCoveredBy(grid[(int)maxCoverNodeIndex.x, (int)maxCoverNodeIndex.y]))
                    {
                        grid[i, j].isCovered = true;
                    }
                }
            }
        }

    }

    // Previous version
    //private void createMSC()
    //{
    //    // Marke all nodes that are covered at the start
    //    int numberOfNodesCoveredAtStart = 0;
    //    foreach (GameObject car in cars)
    //    {
    //        int i = terrain_manager.myInfo.get_i_index(car.transform.position.x);
    //        int j = terrain_manager.myInfo.get_j_index(car.transform.position.z);
    //        CoverNode carNode = grid[i, j];
    //        for (int s = 0; s < gridNoX; ++s)
    //        {
    //            for (int t = 0; t < gridNoZ; ++t)
    //            {
    //                if (grid[s, t].isCovered || grid[s, t].isObstacle) continue;
    //                if (grid[s, t].IsCoveredBy(carNode))
    //                {
    //                    grid[s, t].isCovered = true;
    //                    numberOfNodesCoveredAtStart++;
    //                }
    //            }
    //        }
    //    }
    //    // Greedy algorithm
    //    int cellsCovered = numberOfObstacles + numberOfNodesCoveredAtStart;
    //    while (cellsCovered < numberOfGridCells)
    //    {
    //        int maxNumberCovered = 0;
    //        Vector2 maxCoverNodeIndex = new Vector2();
    //        // Find the node that covers the most of other oncovered nodes
    //        for (int i = 0; i < gridNoX; ++i)
    //        {
    //            for (int j = 0; j < gridNoZ; ++j)
    //            {
    //                if (grid[i, j].isCovered || grid[i, j].isObstacle) continue;
    //                int numberCovered = 0;
    //                for (int s = 0; s < gridNoX; ++s)
    //                {
    //                    for (int t = 0; t < gridNoZ; ++t)
    //                    {
    //                        if (grid[s, t].isCovered || grid[s, t].isObstacle) continue;

    //                        if (grid[s, t].IsCoveredBy(grid[i, j]))
    //                        {
    //                            numberCovered++;
    //                        }
    //                    }
    //                }

    //                if (numberCovered > maxNumberCovered)
    //                {
    //                    maxNumberCovered = numberCovered;
    //                    maxCoverNodeIndex = new Vector2(i, j);
    //                }
    //            }
    //        }
    //        cellsCovered += maxNumberCovered;
    //        coverNodes.Add(grid[(int)maxCoverNodeIndex.x, (int)maxCoverNodeIndex.y]);
    //        for (int i = 0; i < gridNoX; ++i)
    //        {
    //            for (int j = 0; j < gridNoZ; ++j)
    //            {
    //                if (grid[i, j].IsCoveredBy(grid[(int)maxCoverNodeIndex.x, (int)maxCoverNodeIndex.y]))
    //                {
    //                    grid[i, j].isCovered = true;
    //                }
    //            }
    //        }
    //    }

    //}

    private int Get1Dindex(int i, int j)
    {
        return i * gridNoZ + j;
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
        int k = 0;
        foreach (var car_final_path in mTSP_solution)
        {
            if (k == 0)
            {
                Gizmos.color = Color.black;
            } else if (k == 1)
            {
                Gizmos.color = Color.blue;
            } else if (k == 2)
            {
                Gizmos.color = Color.magenta;
            }
            for (int i = 0; i < car_final_path.Count-1; i++)
            {
                Gizmos.DrawLine(car_final_path[i].position, car_final_path[i + 1].position);
            }
            Gizmos.DrawLine(car_final_path[car_final_path.Count - 1].position, car_final_path[0].position);
            k++;
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