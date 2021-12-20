using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon : MonoBehaviour
{
    public List<GameObject> vertices;

    // @TODO i might not even need the edges since the vertices are in order.
    public List<Edge> edges;

    private void Awake()
    {
        //
    }

    public void Add(GameObject v)
    {
        vertices.Add(v);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Assign vertex types (split/merge/start/end) to each vertex.
    public void IdentifyVertexTypes()
    {
        for(var i = 0; i < vertices.Count; i++)
        {
            // Traversing the vertices list happens in CW order, thus the polygon is to the 
            // right of the edge.
            PolygonVertex e = vertices[i].GetComponent<PolygonVertex>();
            PolygonVertex prev = i == 0 ? vertices[vertices.Count - 1].GetComponent<PolygonVertex>() : vertices[i - 1].GetComponent<PolygonVertex>();
            PolygonVertex next = i != vertices.Count - 1 ? vertices[i + 1].GetComponent<PolygonVertex>() : vertices[0].GetComponent<PolygonVertex>();

            float angle = Vector2.SignedAngle(new Edge(e, prev).ToVector(), new Edge(e, next).ToVector());
            if(angle < 0) {
                // Change to 360 degree notation instead of 180, -180.
                angle = 360 - angle * -1;
            }

            // Assign types to polygon vertices.
            if(e.y > prev.y && e.y > next.y && e.x > prev.x && angle < 180) {
                e.Type = PolygonVertex._Type.Start;
            } else if(e.y > prev.y && e.y > next.y && e.x < prev.x && angle < 180) { 
                e.Type = PolygonVertex._Type.Split;
            } else if(e.y < prev.y && e.y < next.y && e.x < prev.x && angle > 180) {
                e.Type = PolygonVertex._Type.End;
            } else if(e.y < prev.y && e.y < next.y && e.x > prev.x && angle > 180) {
                e.Type = PolygonVertex._Type.Merge;
            } else {
                e.Type = PolygonVertex._Type.Regular;
            }
        }
    }

    // Triangulate a polygon.
    public void Triangulate()
    {
        //
    }

    private void TriangulateYMonotone()
    {
        //
    }

    // Merge a polygon with another one.
    public Polygon Merge(Polygon p)
    {
        throw new System.Exception("Not implemented");
    }

    private List<Polygon> CreateYMonotone()
    {
        return new List<Polygon>();
    }
}
