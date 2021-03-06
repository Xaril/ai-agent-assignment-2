﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public interface IHasNeighbours<N>
{
    IEnumerable<N> Neighbours { get; }
    bool sameSquare(N other);
    int GetNodeCost();
}


class Node : IHasNeighbours<Node>
{
    public static readonly int UP = 0, UPRIGHT = 1, RIGHT = 2, DOWNRIGHT = 4, DOWN = 5, DOWNLEFT = 6, LEFT = 7, UPLEFT = 8;

    public readonly Point location;
    public readonly int cost;
    public readonly Grid grid;
    private readonly int carDir;
    private readonly int turns;
    private readonly bool goingBackwards;

    private static readonly int travelCost = 1;
    private static readonly int maxTurns = 2;

    private static readonly int backCost = 100;
    private static readonly int turnCost = 5;

    public Node(Point location, int cost, Grid grid, bool goingBackwards, int carDir = -1, int turns = 0)
    {
        this.location = location;
        this.cost = cost;
        this.grid = grid;
        this.carDir = carDir;
        this.turns = turns;
        this.goingBackwards = goingBackwards;
    }

    public Node(Node node)
    {
        location = node.location;
        cost = node.cost;
        grid = node.grid;
        carDir = node.carDir;
        turns = node.turns;
        goingBackwards = node.goingBackwards;
    }

    public Node Up
    {
        get
        {
            if (carDir != UP && turns > 0)
                return null;
            if (carDir == RIGHT || carDir == LEFT)
                return null;
       //     Point nextP = new Point(location.x, location.y+1);
            if (carDir == DOWNLEFT || carDir == DOWNRIGHT || carDir == DOWN)
                return grid.GetNode(new Point(location.x, location.y - 1), travelCost + (!goingBackwards ? backCost : 0), !goingBackwards, UP, maxTurns);
            if (carDir != UP)
                return grid.GetNode(new Point(location.x, location.y - 1), travelCost + turnCost + (goingBackwards ? backCost: 0), goingBackwards, UP, maxTurns);
            return grid.GetNode(new Point(location.x, location.y - 1), travelCost + (goingBackwards ? backCost : 0), goingBackwards, UP, turns - 1);
        }
    }

    public Node UpLeft
 
    {
        get
        {
            if (carDir != UPLEFT && turns > 0)
                return null;
            if (carDir == UPRIGHT || carDir == DOWNLEFT)
                return null;
          //  Point nextP = new Point(location.x-1, location.y + 1);

            if (carDir == DOWN || carDir == RIGHT || carDir == DOWNRIGHT)
                return grid.GetNode(new Point(location.x - 1, location.y - 1), travelCost + (!goingBackwards ? backCost : 0), !goingBackwards, UPLEFT, maxTurns);

            if (carDir != UPLEFT)
                return grid.GetNode(new Point(location.x - 1, location.y - 1), travelCost + turnCost + (goingBackwards ? backCost : 0), goingBackwards, UPLEFT, maxTurns);
            return grid.GetNode(new Point(location.x - 1, location.y - 1), travelCost + (goingBackwards ? backCost : 0), goingBackwards, UPLEFT, turns - 1);
        }
    }

    public Node UpRight
    {
        get
        {
            if (carDir != UPRIGHT && turns > 0)
                return null;
            if (carDir == UPLEFT || carDir == DOWNRIGHT)
                return null;
          //  Point nextP = new Point(location.x, location.y + 1);

            if (carDir == DOWN || carDir == LEFT || carDir == DOWNLEFT)
                return grid.GetNode(new Point(location.x + 1, location.y - 1), travelCost + (!goingBackwards ? backCost : 0), !goingBackwards, UPRIGHT, maxTurns);

            if (carDir != UPRIGHT)
                return grid.GetNode(new Point(location.x + 1, location.y - 1), travelCost + (goingBackwards ? backCost : 0) + turnCost, goingBackwards, UPRIGHT, maxTurns);
            return grid.GetNode(new Point(location.x + 1, location.y - 1), travelCost + (goingBackwards ? backCost : 0), goingBackwards, UPRIGHT, turns - 1);
        }
    }

    public Node DownRight
    {
        get
        {
            if (carDir != DOWNRIGHT && turns > 0)
                return null;
            if (carDir == DOWNLEFT || carDir == UPRIGHT)
                return null;

            if (carDir == UP || carDir == LEFT || carDir == UPLEFT)
                return grid.GetNode(new Point(location.x + 1, location.y + 1), travelCost + (!goingBackwards ? backCost : 0), !goingBackwards, DOWNRIGHT, maxTurns);

            if (carDir != DOWNRIGHT)
                return grid.GetNode(new Point(location.x + 1, location.y + 1), travelCost + (goingBackwards ? backCost : 0) + turnCost, goingBackwards, DOWNRIGHT, maxTurns);
            return grid.GetNode(new Point(location.x + 1, location.y + 1), travelCost + (goingBackwards ? backCost : 0), goingBackwards, DOWNRIGHT, turns - 1);
        }
    }

    public Node DownLeft
    {
        get
        {
            if (carDir != DOWNLEFT && turns > 0)
                return null;
            if (carDir == UPLEFT || carDir == DOWNRIGHT)
                return null;

            if (carDir == UP || carDir == RIGHT || carDir == UPRIGHT)
                return grid.GetNode(new Point(location.x - 1, location.y + 1), travelCost + (!goingBackwards ? backCost : 0), !goingBackwards, DOWNLEFT, maxTurns);

            if (carDir != DOWNLEFT)
                return grid.GetNode(new Point(location.x - 1, location.y + 1), travelCost + (goingBackwards ? backCost : 0) + turnCost, goingBackwards, DOWNLEFT, maxTurns);
            return grid.GetNode(new Point(location.x - 1, location.y + 1), travelCost + (goingBackwards ? backCost : 0), goingBackwards, DOWNLEFT, turns - 1);
        }
    }

    public Node Down
    {
        get
        {
            if (carDir != DOWN && turns > 0)
                return null;
            if (carDir == RIGHT || carDir == LEFT)
                return null;

            if (carDir == UPLEFT || carDir == UPRIGHT || carDir == UP)
                return grid.GetNode(new Point(location.x, location.y + 1), travelCost + (!goingBackwards ? backCost : 0), !goingBackwards, DOWN, maxTurns);

            if (carDir != DOWN)
                return grid.GetNode(new Point(location.x, location.y + 1), travelCost + (goingBackwards ? backCost : 0) + turnCost, goingBackwards, DOWN, maxTurns);
            return grid.GetNode(new Point(location.x, location.y + 1), travelCost + (goingBackwards ? backCost : 0), goingBackwards, DOWN, turns - 1);
        }
    }

    public Node Left
    {
        get
        {
            if (carDir != LEFT && turns > 0)
                return null;
            if (carDir == UP || carDir == DOWN)
                return null;

            if (carDir == DOWNRIGHT || carDir == UPRIGHT || carDir == RIGHT)
                return grid.GetNode(new Point(location.x - 1, location.y), travelCost + (!goingBackwards ? backCost : 0), !goingBackwards, LEFT, maxTurns);

            if (carDir != LEFT)
                return grid.GetNode(new Point(location.x - 1, location.y), travelCost + (goingBackwards ? backCost : 0) + turnCost, goingBackwards, LEFT, maxTurns);
            return grid.GetNode(new Point(location.x - 1, location.y), travelCost + (goingBackwards ? backCost : 0), goingBackwards, LEFT, turns - 1);
        }
    }

    public Node Right
    {
        get
        {
            if (carDir != RIGHT && turns > 0)
                return null;
            if (carDir == UP || carDir == DOWN)
                return null;

            if (carDir == UPLEFT || carDir == DOWNLEFT || carDir == LEFT)
                return grid.GetNode(new Point(location.x + 1, location.y), travelCost + (!goingBackwards ? backCost : 0), !goingBackwards, RIGHT, maxTurns);

            if (carDir != RIGHT)
                return grid.GetNode(new Point(location.x + 1, location.y), travelCost + (goingBackwards ? backCost : 0) + turnCost, goingBackwards, RIGHT, maxTurns);
            return grid.GetNode(new Point(location.x + 1, location.y), travelCost + (goingBackwards ? backCost : 0), goingBackwards, RIGHT, turns - 1);
        }
    }

    public IEnumerable<Node> Neighbours
    {
        get
        {
            Node[] neighbors = new Node[] { Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight };
            foreach (Node neighbor in neighbors)
            {
                if (neighbor != null)
                {
                    yield return neighbor;
                }
            }
        }
    }

    public override bool Equals(object obj)
    {
        var item = obj as Node;
        if (item == null)
        {
            return false;
        }
        return location.x == item.location.x && location.y == item.location.y && carDir == item.carDir;
    }

    public override int GetHashCode()
    {
        return (location.x.GetHashCode() * 7) ^ (location.y.GetHashCode() * 3) ^ carDir;
    }

    public override string ToString()
    {
        return "X: " + location.x + " Y: " + location.y;
    }

    public bool sameSquare(Node other)
    {
        return location.x == other.location.x && location.y == other.location.y;
    }

    public int GetNodeCost()
    {
        return cost;
    }
}

public class Point
{
    public readonly int x;
    public readonly int y;
    public Point(Point point)
    {
        x = point.x;
        y = point.y;
    }

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object obj)
    {
        var item = obj as Point;
        if (item == null)
            return false;
        return x == item.x && y == item.y;
    }

    public override int GetHashCode()
    {
        return (x.GetHashCode() * 7) ^ (y.GetHashCode() * 3);
    }

}
