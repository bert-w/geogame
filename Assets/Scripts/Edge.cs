using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public Vector2 start { get; set; }
    public Vector2 end { get; set; }

    public Edge(GameObject startVertex, GameObject endVertex)
    {
        this.start = startVertex.GetComponent<PolygonVertex>().transform.position;
        this.start = endVertex.GetComponent<PolygonVertex>().transform.position;
    }

    public Edge(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.end = end;
    }
}

