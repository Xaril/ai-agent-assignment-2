using System;
using System.Collections.Generic;

public class NodeST
{
    public NodeST parent = null;
    public List<NodeST> children;
    public GraphNode value;

	public NodeST(GraphNode node)
	{
        this.value = node;
	}

    public NodeST(GraphNode node, NodeST parent)
    {
        this.value = node;
        this.parent = parent;
    }


}
