using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRP 
{

    private List<Vector3> stations;
    private int numberOfCars;
    private int[,] distances;
    private TerrainManager terrain_manager;
    public List<int>[] result;
    private readonly int max_iterations = 100;

    public VRP(List<Vector3> points, int nrOfCars, TerrainManager terrain_manager)
    {
        this.stations = points;
        this.numberOfCars = nrOfCars;
        this.distances = new int[points.Count, points.Count];
        this.terrain_manager = terrain_manager;
        GenerateDistances();
        Solve();
    }

    private List<int>[] GenerateOneIteration()
    {
        // find initial solution

        List<int>[] newResult = new List<int>[numberOfCars];
        int numberOfAddedStations = 1 + numberOfCars;
        bool[] added = new bool[stations.Count];
        added[0] = true;
        for (int i = 0; i < newResult.Length; i++)
        {
            newResult[i] = new List<int>() { 0, GetRandomPoint(ref added) };
        }

        while (numberOfAddedStations < stations.Count)
        {
            for (int i = 0; i < newResult.Length; i++)
            {
                if (numberOfAddedStations == stations.Count) break;
                int next = FindNextNeighbour(ref newResult, ref added, i);
                newResult[i].Add(next);
                numberOfAddedStations++;
                added[next] = true;
            }
        }

        return newResult;
    }

    private int CaclulateDistance(ref List<int>[] res)
    {
        int maxDistance = 0;
        for(int i = 0; i < res.Length; i++)
        {
            int distance = 0;
            for (int j = 0; j < res[i].Count - 1; j++)
            {
                distance += distances[res[i][j], res[i][j + 1]];
            }
            if(distance > maxDistance)
            {
                maxDistance = distance;
            }
        }
        return maxDistance;
    }

    private void Solve( )
    {
        List<int>[] bestResult = GenerateOneIteration();
        int bestDistance = CaclulateDistance(ref bestResult);
        int iteration = 0;

        while(iteration < max_iterations)
        {
            List<int>[] currentResult = GenerateOneIteration();
            int currentDistance = CaclulateDistance(ref currentResult);
            if(currentDistance < bestDistance)
            {
                bestResult = currentResult;
                bestDistance = currentDistance;
            }

            iteration++;
        }

        result = bestResult;
    }

    private int GetRandomPoint(ref bool[] added)
    {
        int random = Random.Range(1, stations.Count - 1);
        while(added[random])
        {
            random = Random.Range(1, stations.Count - 1);
        }
        added[random] = true;
        return random;
    }

    private int FindNextNeighbour(ref List<int>[] currentResult, ref bool[] added, int carNumber)
    {
        int bestStation = -1;
        int minDistance = int.MaxValue;
        for(int i = 0; i < stations.Count; i++)
        {
            if (added[i]) continue;
            int currentDistance = (int)(distances[currentResult[carNumber][currentResult[carNumber].Count - 1], i]);
            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                bestStation = i;
            }

        }
        return bestStation;
    }


    private void GenerateDistances()
    {
        //PathGenerator aStar = new PathGenerator(terrain_manager, null);
        
        for (int i = 0; i < stations.Count; i++)
        {
            for(int j = 0; j < stations.Count; j++)
            {
                if (i == j)
                {
                    distances[i, j] = 0;
                } else
                {
                    /*
                    Point startPoint = new Point((int)stations[i].x, (int)stations[i].z);
                    Point endPoint = new Point((int)stations[j].x, (int)stations[j].z);
                    // TODO: have a heuristic for determing an angle between them.
                    distances[i,j] =  aStar.GetPath(startPoint, endPoint, 0f).Count;
                    */
                    distances[i, j] = (int)(stations[i] - stations[j]).magnitude;
                }
            }
        }
    }
}
