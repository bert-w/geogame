using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon : MonoBehaviour
{
    public enum _Direction {None, CW, CCW}
    public List<PolygonVertex> vertices;

    public GameObject polygonVertex;

    // @TODO i might not even need the edges since the vertices are in order.
    public List<Edge> edges;

    [SerializeField]
    private _Direction _direction = _Direction.None;

    void Start()
    {
        polygonVertex = Instantiate(Resources.Load("Vertex", typeof(GameObject))) as GameObject;
        polygonVertex.transform.SetParent(transform);
    }

    private void Awake()
    {
        //
    }

    public PolygonVertex Add(PolygonVertex v)
    {
        vertices.Add(v);
        // Recalculate direction.
        _direction = Direction;
        return v;
    }

    public PolygonVertex Add(Vector2 vector)
    {
        PolygonVertex vertex = Instantiate(polygonVertex, vector, Quaternion.identity).GetComponent<PolygonVertex>();
        vertex.gameObject.name = "Polygon Vertex " + vertices.Count;
        // Assign parent as this object.
        vertex.transform.SetParent(transform);
        return Add(vertex);
    }

    public void empty()
    {
        for(var i = 0; i < vertices.Count; i++) {
            Destroy(vertices[i].gameObject);
        }
        vertices.Clear();
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
            PolygonVertex e = vertices[i];
            PolygonVertex prev = i == 0 ? vertices[vertices.Count - 1] : vertices[i - 1];
            PolygonVertex next = i != vertices.Count - 1 ? vertices[i + 1] : vertices[0];

            float angle = Vector2.SignedAngle(new Edge(e, prev).ToVector(), new Edge(e, next).ToVector());
            if(angle < 0) {
                // Change to 360 degree notation instead of 180, -180.
                angle = 360 - angle * -1;
            }

            // Assign types to polygon vertices.
            if(e.y > prev.y && e.y > next.y && e.x > prev.x && angle < 180) {
                e.Type = PolygonVertex._Type.Start;
                e.Color = Color.cyan;
            } else if(e.y > prev.y && e.y > next.y && e.x < prev.x && angle > 180) { 
                e.Type = PolygonVertex._Type.Split;
                e.Color = Color.red;
            } else if(e.y < prev.y && e.y < next.y && angle < 180) {
                e.Type = PolygonVertex._Type.End;
                e.Color = Color.blue;
            } else if(e.y < prev.y && e.y < next.y && e.x > prev.x && angle > 180) {
                e.Type = PolygonVertex._Type.Merge;
                e.Color = Color.yellow;
            } else {
                e.Color = Color.magenta;
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
                Vector2 next = i < vertices.Count - 1 ? vertices[i + 1].ToVector() : new Vector2(0, 0);
                Vector2 curr = vertices[i].ToVector();
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
