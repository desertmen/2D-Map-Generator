using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderCreator
{
    bool[,] grid;
    int xGridSize;
    int yGridSize;
    int xStart;
    int yStart;
    int maxIterations;
    // start must be next to occupied tile
    public ColliderCreator(bool[,] _grid, int _xStart, int _yStart, int _maxIterations = 10000)
    {
        grid = _grid;
        xStart = _xStart;
        yStart = _yStart;
        xGridSize = grid.GetLength(0);
        yGridSize = grid.GetLength(1);
        maxIterations = _maxIterations;
    }

    public Vector2 [] getColliderPoints(float wallThickness)
    {
        List<Vector2> colliderPoints = new List<Vector2>();

        Rect currentRect = new Rect(new Vector2Int(xStart, yStart));
        List<Rect> obstaclesAroundFirst = getObstaclesAroundRect(currentRect);
        if(obstaclesAroundFirst.Count < 1)
        {
            Debug.Log("Collider Creator Error: didnt start next to an obstacle");
            return null;
        }
        List<Vector2[]> borders = new List<Vector2[]>();
        foreach(Rect obstacle in obstaclesAroundFirst)
        {
            List<Vector2> border = currentRect.getSharedPoints(obstacle);
            borders.Add(border.ToArray());
        }
        if(borders.Count == 1)
        {
            colliderPoints.Add(borders[0][0]);
            colliderPoints.Add(borders[0][1]);
        }
        else if(borders.Count == 2)
        {
            colliderPoints.Add(borders[0][0]);
            colliderPoints.Add(borders[0][1]);
            int index1 = colliderPoints.IndexOf(borders[1][0]);
            int index2 = colliderPoints.IndexOf(borders[1][1]);
            if(index1 != -1)
            {
                if(index1 == 0)
                {
                    colliderPoints.Insert(0, borders[1][1]);
                }
                else
                {
                    colliderPoints.Add(borders[1][1]);
                }
            }
            if(index2 != -1)
            {
                if (index1 == 0)
                {
                    colliderPoints.Insert(0, borders[1][0]);
                }
                else
                {
                    colliderPoints.Add(borders[1][0]);
                }
            }
        }
        else if(borders.Count == 3)
        {
            colliderPoints.Add(borders[0][0]);
            colliderPoints.Add(borders[0][1]);
            for(int j = 1; j < borders.Count; j++)
            {
                for (int i = j; i < borders.Count; i++)
                {
                    int index1 = colliderPoints.IndexOf(borders[i][0]);
                    int index2 = colliderPoints.IndexOf(borders[i][1]);

                    if (index1 != -1 && index2 == -1)
                    {
                        if (index1 == 0)
                        {
                            colliderPoints.Insert(0, borders[i][1]);
                        }
                        else
                        {
                            colliderPoints.Add(borders[i][1]);
                        }
                    }
                    else if (index1 == -1  && index2 != -1)
                    {
                        if (index2 == 0)
                        {
                            colliderPoints.Insert(0, borders[i][0]);
                        }
                        else
                        {
                            colliderPoints.Add(borders[i][0]);
                        }
                    }
                }
            }
        }
        else if (borders.Count == 4)
        {
            return currentRect.corners.ToArray();
        }

        int iters = 0;
        while ((iters == 0 || colliderPoints[colliderPoints.Count - 1] != colliderPoints[0]) && iters < maxIterations)
        {
            iters++;
            if(iters == 9999)
            {
                Debug.Log("Error - maximal iterration (" + maxIterations.ToString() + ") count in ColliderCreator has been exceeded");
                return null;
            }
            List<Vector2Int> neighbours = getNeighbours(currentRect.BLcorner.x, currentRect.BLcorner.y);
            foreach (Vector2Int neighbour in neighbours)
            {
                if (grid[neighbour.x, neighbour.y])
                {
                    continue;
                }
                Rect neighbourRect = new Rect(neighbour);
                if (neighbourRect.corners.Contains(colliderPoints[colliderPoints.Count - 1]) == false)
                {
                    continue;
                }

                bool foundCorrectObstacle = false;
                List<Rect> obstacles = getObstaclesAroundRect(neighbourRect);
                foreach (Rect obstacle in obstacles)
                {
                    List<Rect> ObstaclesTouchingLastPoint = findRectsTouchingPoint(obstacles, colliderPoints[colliderPoints.Count - 1]);
                    foreach(Rect obstacleTouchingPoint in ObstaclesTouchingLastPoint)
                    {
                        List<Vector2> sharedPoints = obstacleTouchingPoint.getSharedPoints(neighbourRect);
                        if (!sharedPoints.Contains(colliderPoints[colliderPoints.Count - 2]))
                        {
                            if(sharedPoints[0] != colliderPoints[colliderPoints.Count - 1])
                            {
                                colliderPoints.Add(sharedPoints[0]);
                            }
                            else
                            {
                                colliderPoints.Add(sharedPoints[1]);
                            }
                            foundCorrectObstacle = true;
                        }
                    }
                }
                if (foundCorrectObstacle)
                {
                    currentRect = neighbourRect;
                    break;
                }
            }
        }
        if(colliderPoints.Count > 0)
        {
            colliderPoints.RemoveAt(0);
        }
        return colliderPoints.ToArray();
    }

    private Vector2 vector2Abs(Vector2 vec)
    {
        return new Vector2(Mathf.Abs(vec.x), Mathf.Abs(vec.y));
    }

    private List<Vector2Int> getNeighbours(int x, int y)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();
        for(int yDiff = -1; yDiff < 2; yDiff++)
        {
            for(int xDiff = -1; xDiff < 2; xDiff++)
            {
                if(xDiff == 0 && yDiff == 0)
                {
                    continue;
                }
                int X = x + xDiff;
                int Y = y + yDiff;
                if(-1 <= X && X < xGridSize + 1 && -1 <= Y && Y < yGridSize + 1)
                {
                    neighbours.Add(new Vector2Int(X, Y));
                }
            }
        }
        return neighbours;
    }

    private List<Rect> getObstaclesAroundRect(Rect rect)
    {
        List<Rect> obstacles = new List<Rect>();

        for (int y = -1; y < 2; y += 2)
        {
            int X = rect.BLcorner.x;
            int Y = y + rect.BLcorner.y;
            if (0 <= X && X < xGridSize && 0 <= Y && Y < yGridSize)
            {
                if (grid[X, Y])
                {
                    obstacles.Add(new Rect(new Vector2Int(X, Y)));
                }
            }
        }
        for (int x = -1; x < 2; x += 2)
        {
            int X = x + rect.BLcorner.x;
            int Y = rect.BLcorner.y;
            if (0 <= X && X < xGridSize && 0 <= Y && Y < yGridSize)
            {
                if (grid[X, Y])
                {
                    obstacles.Add(new Rect(new Vector2Int(X, Y)));
                }
            }
        }
        return obstacles;
    }

    private List<Rect> findRectsTouchingPoint(List<Rect> rects, Vector2 point)
    {
        List<Rect> rectsTouchingPoints = new List<Rect>();
        foreach(Rect rect in rects)
        {
            if (rect.corners.Contains(point)){
                rectsTouchingPoints.Add(rect);
            }
        }
        return rectsTouchingPoints;
    }

    public class Rect
    {
        public List<Vector2> corners;
        public Vector2Int BLcorner;
        public bool done = false;

        public Rect(Vector2Int BLcorner)
        {
            this.BLcorner = BLcorner;
            corners = new List<Vector2>();
            corners.Add(BLcorner);
            corners.Add(new Vector2Int(BLcorner.x + 1, BLcorner.y));
            corners.Add(new Vector2Int(BLcorner.x, BLcorner.y + 1));
            corners.Add(new Vector2Int(BLcorner.x + 1, BLcorner.y + 1));
        }
        public List<Vector2> getSharedPoints(Rect rect)
        {
            List<Vector2> sharedPoints = new List<Vector2>();
            foreach(Vector2 corner in rect.corners)
            {
                if (corners.Contains(corner))
                {
                    sharedPoints.Add(corner);
                }
            }
            return sharedPoints;
        }
        public bool sharesPoint(Rect rect, Vector2 point)
        {
            List<Vector2> sharedPoints1 = getSharedPoints(rect);
            if (sharedPoints1.Contains(point))
            {
                return true;
            }
            return false;
        }
    }
}
