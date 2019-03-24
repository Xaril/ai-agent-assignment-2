using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : CarAction
{
    Vector3 goalPosition;
    List<Vector3> path;

    public MoveAction(GameObject car, Vector3 goalPosition)
    {
        this.car = car;
        this.goalPosition = goalPosition;
    }

    public MoveAction(GameObject car, List<Vector3> path) {
        this.car = car;
        this.path = path;
    }

    public override void DoAction()
    {
        
    }
}
