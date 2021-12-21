using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Edge
{

    public Vector2 start { get; set; }
    public Vector2 end { get; set; }

    // Helper of this edge which we need for triangulation.
    [field: SerializeField]
    public PolygonVertex HelperVertex { get; set; }

    public Edge(GameObject startVertex, GameObject endVertex)
    {
        this.start = startVertex.GetComponent<PolygonVertex>().transform.position;
        this.end = endVertex.GetComponent<PolygonVertex>().transform.position;
    }

    public Edge(PolygonVertex startVertex, PolygonVertex endVertex)
    {
        this.start = new Vector2(startVertex.x, startVertex.y);
        this.end = new Vector2(endVertex.x, endVertex.y);
    }

    public Edge(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.end = end;
    }

    // Determine edge crossings. Note that this function needs to be called twice (once reversed).
    private Vector2? _Crosses(Edge edge)
    {
        // https://stackoverflow.com/a/563275/1346367
        // NOTE: there is a shortcoming in the original implementation: both a->b and b->a need to be checked.
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

    // Determine whether this edge crosses the given one. If it does, the intersection point is returned.
    public Vector2? Crosses(Edge edge)
    {
        var a = edge._Crosses(this);
        var b = _Crosses(edge);
        if(a.HasValue && b.HasValue) {
            // Values should be the same so we return a.
            return a;
        }
        return null;
    }

    public float Length
    {
        get
        {
            return Mathf.Sqrt(Mathf.Pow(start.x - end.x, 2) + Mathf.Pow(start.y - end.y, 2));
        }
    }

    // Convert the edge to a vector, with the start at (0, 0).
    public Vector2 ToVector()
    {
        Vector2 _end = end - start;
        return new Vector2(_end.x, _end.y);
    }

    public void DebugDraw()
    {
        DebugDraw(Color.red, 1f);
    }

    public void DebugDraw(Color color, float duration)
    {
        Debug.Log("[Edge] " + start + " - " + end);
        Debug.DrawLine(start, end, color, duration);
    }
}

