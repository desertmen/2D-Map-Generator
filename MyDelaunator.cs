using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp;
public class MyDelaunator
{
    public class delaunayPoint : IPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public delaunayPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    private delaunayPoint[] dPoints;

    private Delaunator delaunator;

    public MyDelaunator(Vector2[] points)
    {
        dPoints = new delaunayPoint[points.Length];
        for(int i = 0; i < dPoints.Length; i++)
        {
            dPoints[i] = new delaunayPoint(points[i].x, points[i].y);
        }
        delaunator = new Delaunator(dPoints);
        
    }

    public Vector2[][] getEdges()
    {
        var edges = delaunator.GetEdges();
        List<Vector2[]> edgesList = new List<Vector2[]>();
        foreach(Edge edge in edges)
        {
            Vector2 q = new Vector2((float)edge.Q.X, (float)edge.Q.Y);
            Vector2 p = new Vector2((float)edge.P.X, (float)edge.P.Y);
            edgesList.Add(new Vector2[2] { p, q });
        }
        return edgesList.ToArray();
    }
}
