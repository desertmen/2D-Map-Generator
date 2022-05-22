using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathFinder
{
    int xGridSize;
    int yGridSize;
    Vector2 start;
    Vector2 end;
    Vector2 dir1;
    Vector2 dir2;
    Node[,] nodeGrid;

    public PathFinder(bool[,] walkableGrid)
    {
        xGridSize = walkableGrid.GetLength(0);
        yGridSize = walkableGrid.GetLength(1);
        nodeGrid = new Node[xGridSize, yGridSize];
        for(int y = 0; y < yGridSize; y++)
        {
            for(int x = 0; x < xGridSize; x++)
            {
                nodeGrid[x, y] = new Node(!walkableGrid[x, y], x, y);
            }
        }
    }

    public List<Node> findPath(int xStart, int yStart, int xEnd, int yEnd, Vector2 _dir1, Vector2 _dir2)
    {
        start = new Vector2(xStart, yStart);
        end = new Vector2(xEnd, yEnd);
        dir1 = _dir1;
        dir2 = _dir2;
        Node startNode = nodeGrid[xStart, yStart];
        Node endNode = nodeGrid[xEnd, yEnd];
        int heapMaxSize = xGridSize * yGridSize;

        Heap<Node> openSet = new Heap<Node>(heapMaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.add(startNode);

        while(openSet.Count > 0)
        {
            Node currentNode = openSet.removeFirst();
            closedSet.Add(currentNode);

            if(currentNode == endNode)
            {
                return RetracePath(startNode, endNode);
            }
            foreach (Node neighbour in getNeighbours(currentNode))
            {
                if(neighbour.walkable == false || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + getDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = getDistance(neighbour, endNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.add(neighbour);
                    }
                }
            }
        }
        return null;
    }

    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        
        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(currentNode);
        path.Reverse();
        return path;
    }

    public Vector2[] getPathPositions(List<Node> path)
    {
        Vector2[] vectorPath = new Vector2[path.Count];
        for(int i = 0; i < path.Count; i++)
        {
            vectorPath[i] = new Vector2(path[i].gridX, path[i].gridY);
        }
        return vectorPath;
    }

    // cannot move diagonaly
    List<Node> getNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for(int i = -1; i < 2; i += 2)
        {
            if(node.gridX + i >= 0 && node.gridX + i < xGridSize)
            {
                neighbours.Add(nodeGrid[node.gridX + i, node.gridY]);
            }
            if(node.gridY + i >= 0 && node.gridY + i < yGridSize)
            {
                neighbours.Add(nodeGrid[node.gridX, node.gridY + i]);
            }
        }
        return neighbours;
    }

    // TODO start and end in dir -> smaller distance, turns -> bigger distance
    int getDistance(Node nodeA, Node nodeB)
    {
        int dist = 0;
        dist += Mathf.Abs(nodeA.gridX - nodeB.gridX);
        dist += Mathf.Abs(nodeA.gridY - nodeB.gridY);
        Vector2Int center = new Vector2Int(nodeB.gridX - nodeA.gridX/2, nodeB.gridY - nodeA.gridY/2);
        int distFromCenter = Mathf.Min(Mathf.Abs(nodeB.gridX - center.x), Mathf.Abs(nodeB.gridY - center.y));

        // turns add to distance
        if (nodeA.parent != null)
        {
            Node previous = nodeA.parent;
            if (nodeB.gridX != previous.gridX && nodeB.gridY != previous.gridY)
            {
                dist += distFromCenter;
            }
        }
        else if (nodeB.parent != null)
        {
            Node previous = nodeB.parent;
            if (nodeA.gridX != previous.gridX && nodeA.gridY != previous.gridY)
            {
                dist += distFromCenter;
            }
        }
        // start in dir or get bigger dist
        if (nodeB.gridX - nodeA.gridX != dir1.x && nodeB.gridY - nodeA.gridY != dir1.y && nodeA.gCost < 5)
        {
            dist += distFromCenter;
        }
        if(nodeA.gridX - nodeB.gridX != dir2.x && nodeA.gridY - nodeB.gridY != dir2.y && nodeA.hCost < 5)
        {
            dist += distFromCenter;
        }

        return dist;
    }
}
