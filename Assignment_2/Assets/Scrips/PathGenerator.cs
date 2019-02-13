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

    public void GetPath(Point startPoint, Point endPoint, float carangle)
    {
        aStar = new AStar(grid);
        aStar.init(startPoint.x - grid.xlow, startPoint.y - grid.zlow, endPoint.x - grid.xlow, endPoint.y - grid.zlow, carangle);
        aStar.findPath();
        Debug.LogError(aStar.result.Count);
        GameObject[] path = new GameObject[aStar.result.Count];
        aStar.result.Reverse();
        int i = 0;
        foreach (Node n in aStar.result)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(startPoint.x + (n.location.x - aStar.result[0].location.x), 0.5f, startPoint.y + (n.location.y - aStar.result[0].location.y));
            cube.GetComponent<BoxCollider>().enabled = false;
            cube.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
            //cube.GetComponent<MeshRenderer>().enabled = showTrajectoryCubes;
            path[i] = cube;
            i++;
        }
    }
}
