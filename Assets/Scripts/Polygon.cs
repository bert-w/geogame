using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Polygon : MonoBehaviour
{
    public enum _Direction {None, CW, CCW}

    [field: SerializeField]
    public List<PolygonVertex> vertices { get; set; } = new List<PolygonVertex>();

    private GameObject polygonVertex;

    // @TODO i might not even need the edges since the vertices are in order.
    // Edges might be nice for polygon merging
    [field: SerializeField]
    public List<Edge> edges { get; set; } = new List<Edge>();

    [field: SerializeField]
    public List<Edge> triangulation { get; set; } = new List<Edge>();

    // Determines if the polygon has been completed.
    [SerializeField]
    private bool _completed = false;

    // The direction (CW/CCW) of the vertices to determine the inner polygon.
    [SerializeField]
    private _Direction _direction = _Direction.None;

    [SerializeField]
    private bool showColors = false;

    void Start()
    {
        polygonVertex = Instantiate(Resources.Load("Vertex", typeof(GameObject)), transform) as GameObject;
    }
    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T)) {
            showColors = !showColors;
            foreach(PolygonVertex vertex in vertices) {
                vertex.ShowColors(showColors);
            }
        }
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
        PolygonVertex vertex = Instantiate(polygonVertex, vector, Quaternion.identity, transform).GetComponent<PolygonVertex>();
        vertex.gameObject.name = "Polygon Vertex " + vertices.Count;
        return Add(vertex);
    }

    public void empty()
    {
        for(var i = 0; i < vertices.Count; i++) {
            Destroy(vertices[i].gameObject);
        }
        vertices.Clear();
    }

    
    public bool Completed {
        get {
            return _completed;
        }
        set {
            _completed = value;
            if(value) {
                // When completed, calculate the vertex types.
                AssignVertexTypes();
            }
        }
    }

    // Assign vertex types (split/merge/start/end) to each vertex.
    // Complexity: n
    private void AssignVertexTypes()
    {
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
            } else if(e.y > prev.y && e.y > next.y && e.x < prev.x && angle > 180) { 
                e.Type = PolygonVertex._Type.Split;
            } else if(e.y < prev.y && e.y < next.y && angle < 180) {
                e.Type = PolygonVertex._Type.End;
            } else if(e.y < prev.y && e.y < next.y && e.x > prev.x && angle > 180) {
                e.Type = PolygonVertex._Type.Merge;
            } else {
                e.Type = PolygonVertex._Type.Regular;
            }
        }
    }

    // Assign various helper pointers to edges and vertices.
    // Complexity: 
    private void AssignVertexLeftEdges()
    {
        // Assign left edges to vertices, and helpers to edges.
        // Use MakeMonotone algorithm from book (p.53).
        // https://stackoverflow.com/questions/64908672/sweep-line-polygon-triangulation-how-to-find-edge-left-to-current-vertex
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
            // Not settable.
        }
    }

    // Triangulate a polygon.
    public void Triangulate()
    {
        if(Direction == _Direction.CCW) {
            vertices.Reverse();
        }

        AssignVertexTypes();
        AssignVertexLeftEdges();

        // foreach(PolygonVertex vertex in vertices.OrderBy(v => -v.y).ToList())
        // {
        //     if(vertex.Type == PolygonVertex._Type.Split) {
        //         // Create new edge between this vertex and the helper of its left edge.
        //         Edge edge = new Edge(vertex, vertex.LeftHelperEdge.HelperVertex);
        //         edge.DebugDraw(Color.magenta, 100f);
        //         triangulation.Add(edge);
        //     }
        // }

        TriangulateYMonotone();
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

    public PolygonVertex PolygonYMax()
    {
        var maxY = this.vertices[0].y;
        var maxVertex = this.vertices[0];
        for (int i = 0; i < this.vertices.Count; i++)
        {
            if (vertices[i].y >= maxY)
            {
                maxY = vertices[i].y;
                maxVertex = vertices[i];
            }
        }
        return maxVertex;
    }
}
