using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon : MonoBehaviour
{
    public enum _Direction {None, CW, CCW}
    public List<GameObject> vertices;

    // @TODO i might not even need the edges since the vertices are in order.
    public List<Edge> edges;

    [SerializeField]
    private _Direction _direction = _Direction.None;

    private void Awake()
    {
        //
    }

    public void Add(GameObject v)
    {
        vertices.Add(v);
        // Recalculate direction.
        _direction = Direction;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Assign vertex types (split/merge/start/end) to each vertex.
    public void IdentifyVertexTypes()
    {
        if(Direction == _Direction.CCW) {
            vertices.Reverse();
        }

        for(var i = 0; i < vertices.Count; i++)
        {
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

    // To determine on which side of the edges the polygon lies, we find out CW or CCW direction using the sum over the edges.
    public _Direction Direction
    {
        get {
            float sum = 0;
            for(var i = 0; i < vertices.Count; i++) {
                Vector2 next = i < vertices.Count - 1 ? vertices[i + 1].GetComponent<PolygonVertex>().ToVector() : new Vector2(0, 0);
                Vector2 curr = vertices[i].GetComponent<PolygonVertex>().ToVector();
                sum += (next.x - curr.x)*(next.y + curr.y);
            }
            _direction = sum < 0 ? _Direction.CCW : _Direction.CW;
            return _direction;
        }
        set {
            //
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
