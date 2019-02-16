using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TSP
{
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
            path[i] = best;
            used[bestIndex] = true;
        }
        return path;
    }
}
