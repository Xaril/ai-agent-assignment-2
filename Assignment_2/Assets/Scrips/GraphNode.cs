using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode
{
    public int i;
    public int j;
    public bool obstacle;

    public GraphNode(int i, int j, bool obstacle)
    {
        this.i = i;
        this.j = j;
        this.obstacle = obstacle;
    }
}
