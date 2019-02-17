using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class TSP
{
    //TODO: Change distance heuristic to think about obstacles
    public static List<Vector3> GreedyPath(List<Vector3> points)
    {
        List<Vector3> path = new List<Vector3>() { points[0] };
        bool[] used = new bool[points.Count];
        used[0] = true;

        for(int i = 1; i < points.Count; ++i)
        {
            Vector3 best = Vector3.one;
            int bestIndex = 0;
            for(int j = 1; j < points.Count; ++j)
            {
                if(!used[j] && (best == Vector3.one || Vector3.Distance(path[i - 1], points[j]) < Vector3.Distance(path[i - 1], best)))
                {
                    best = points[j];
                    bestIndex = j;
                }
            }
            path.Add(best);
            used[bestIndex] = true;
        }
        return path;
    }

    public static List<Vector3>[] GenerateCarPaths(List<Vector3> path)
    {
        System.Random random = new System.Random();
        int first = random.Next() % path.Count;
        int second;
        do
        {
            second = random.Next() % path.Count;
        } while (first == second);
        int third;
        do
        {
            third = random.Next() % path.Count;
        } while (third == first || third == second);

        int[] startPoints = { first, second, third };
        Array.Sort(startPoints);

        List<Vector3>[] paths = new List<Vector3>[startPoints.Length];
        for(int i = 0; i < startPoints.Length; ++i)
        {
            paths[i] = new List<Vector3>();
            int j = startPoints[i];
            paths[i].Add(path[j]);
            ++j;
            j = j % path.Count;
            while (j % path.Count != startPoints[i])
            {
                paths[i].Add(path[j]);
                ++j;
                j = j % path.Count;
            }
        }

        return paths;
    }
}
