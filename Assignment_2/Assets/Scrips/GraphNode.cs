using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode
{
    public int i;
    public int j;
    public bool obstacle;
    public int ij;

    public GraphNode(int i, int j, bool obstacle, int n_columns)
    {
        this.i = i;
        this.j = j;
        this.ij = n_columns * i + j;
        this.obstacle = obstacle;
    }

    public override bool Equals(object obj)
    {
        var node = obj as GraphNode;
        return node != null &&
               ij == node.ij;
    }

    public string ToString()
    {
        return string.Format("({0}, {1}) -> {2}", i, j, ij);
    }

}
