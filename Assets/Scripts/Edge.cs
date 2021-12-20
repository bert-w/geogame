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

    // Determine whether this edge crosses the given one. If it does, the intersection point is returned.
    public Vector2? Crosses(Edge edge)
    {
        // https://stackoverflow.com/a/563275/1346367
        // segment 1 = AB, segment 2 = CD
        // E = B-A = ( Bx-Ax, By-Ay )
        // F = D-C = ( Dx-Cx, Dy-Cy ) 
        // P = ( -Ey, Ex )
        // h = ( (A-C) * P ) / ( F * P )

        Vector2 E = new Vector2(end.x - start.x, end.y - start.y);
        Vector2 F = new Vector2(edge.end.x - edge.start.x, edge.end.y - edge.start.y);
        Vector2 P = new Vector2(-E.y, E.x);
        float h = Vector2.Dot(start - edge.start, P) / Vector2.Dot(F, P);

        // If h is between 0 and 1, the segments cross.
        if(h > 0 && h < 1) {
            // Return intersection point.
            return edge.start + (F * h);
        }
        // No intersection found, return null.
        return null;
    }
}

