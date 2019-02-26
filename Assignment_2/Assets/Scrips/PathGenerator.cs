using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGenerator
{
    private AStar aStar;
    private Grid grid;

    public PathGenerator(TerrainManager terrain_manager, CostGridCell[,] turretCost)
    {
        grid = new Grid(terrain_manager, turretCost);
    }

    public List<Vector3> GetPath(Point startPoint, Point endPoint, float carangle)
    {
        this.aStar = new AStar(grid);
        aStar.init(startPoint.x - grid.xlow, startPoint.y - grid.zlow, endPoint.x - grid.xlow, endPoint.y - grid.zlow, carangle);
        aStar.findPath();
        List<Vector3> path = new List<Vector3>();
        for (int i = aStar.result.Count - 1; i >= 0; i--)
        {
            Node n = aStar.result[i];
            path.Add(new Vector3(startPoint.x + (n.location.x - aStar.result[aStar.result.Count - 1].location.x) + 0.5f, 0.5f, startPoint.y + (n.location.y - aStar.result[aStar.result.Count - 1].location.y) + 0.5f));
        }
        return path;
    }
}
