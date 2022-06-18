using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BorderGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        // get map dimensions
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Vector2 mapSize = sprite.bounds.size;

        // get pollygon collider
        PolygonCollider2D polygonCollider2D = GetComponent<PolygonCollider2D>();
        if (polygonCollider2D == null)
        {
            polygonCollider2D = gameObject.AddComponent<PolygonCollider2D>();
        }
        // create edge collider - add points from polygon collider
        EdgeCollider2D edgeCollider2D = gameObject.AddComponent<EdgeCollider2D>();
        List<Vector2> pointsList = polygonCollider2D.points.ToList();
        pointsList.Add(polygonCollider2D.points[0]);
        // scale collider to correct dimensions
        for (int i = 0; i < pointsList.Count; i++)
        {
            pointsList[i] = new Vector2(pointsList[i].x * mapSize.x, pointsList[i].y * mapSize.y);
        }
        edgeCollider2D.points = pointsList.ToArray();
        // destroy polygon collider
        Destroy(polygonCollider2D);
    }
}
