using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGenerator
{
    private AStar aStar;
    private Grid grid;

    public PathGenerator(TerrainManager terrain_manager)
    {
        grid = new Grid(terrain_manager);
    }

    public List<Vector3> GetPath(Point startPoint, Point endPoint, float carangle)
    {
        this.aStar = new AStar(grid);
        aStar.init(startPoint.x - grid.xlow, startPoint.y - grid.zlow, endPoint.x - grid.xlow, endPoint.y - grid.zlow, carangle);
        aStar.findPath();
        Debug.LogError(aStar.result.Count);
        List<Vector3> path = new List<Vector3>();
        aStar.result.Reverse();
        foreach (Node n in aStar.result)
        {
            path.Add(new Vector3(startPoint.x + (n.location.x - aStar.result[0].location.x), 0.5f, startPoint.y + (n.location.y - aStar.result[0].location.y)));
        }
        return path;
    }
}
