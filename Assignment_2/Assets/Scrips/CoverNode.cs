using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverNode 
{
    public Vector3 position;
    public bool isObstacle;
    public bool isCovered;
    public Vector3[] cornerPositions;

    private static float gridCellSize = 20f - 2f;

    public CoverNode(Vector3 position, bool isObstacle)
    {
        this.position = position;
        this.isObstacle = isObstacle;
        this.isCovered = false;
        this.cornerPositions = new Vector3[4];
        cornerPositions[0] = position + new Vector3(gridCellSize/2, 0f, gridCellSize / 2);
        cornerPositions[1] = position + new Vector3(-gridCellSize / 2, 0f, gridCellSize / 2);
        cornerPositions[2] = position + new Vector3(gridCellSize / 2, 0f, -gridCellSize / 2);
        cornerPositions[3] = position + new Vector3(-gridCellSize / 2, 0f, -gridCellSize / 2);
    }

    public bool IsCoveredBy(CoverNode node)
    {
        

        foreach(Vector3 cornerPositon in cornerPositions)
        {
            float distance = (cornerPositon - node.position).magnitude;
            Vector3 direction = (cornerPositon - node.position).normalized;
            RaycastHit hit;
            int layer_mask = LayerMask.GetMask("CubeWalls");
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(node.position + direction, direction, out hit, distance, layer_mask)) return false;
        }

        return true;
    }
}
