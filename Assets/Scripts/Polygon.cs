using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.Algorithms.Triangulation;
using Util.Geometry.Triangulation;
using Util.Geometry.Polygon;

public class Polygon : MonoBehaviour
{
    public enum _Direction {None, CW, CCW}

    public Mesh triangulationMesh;

    [field: SerializeField]
    public List<PolygonVertex> vertices { get; set; } = new List<PolygonVertex>();

    private GameObject polygonVertex;

    /// <summary>
    /// The edges of the polygon.
    /// </summary>
    [field: SerializeField]
    public List<Edge> edges { get; set; } = new List<Edge>();

    /// <summary>
    /// The edges of the triangulation of the polygon.
    /// </summary>
    public List<GameEdge> triangulationGameEdges { get; set; } = new List<GameEdge>();

    [field: SerializeField]
    public List<Edge> triangulation { get; set; } = new List<Edge>();

    /// <summary>
    /// Determines if the polygon has been completed.
    /// </summary>
    [SerializeField]
    private bool _completed = false;

    /// <summary>
    /// The direction (CW/CCW) of the vertices to determine the inner polygon.
    /// </summary>
    [SerializeField]
    private _Direction _direction = _Direction.None;

    [SerializeField]
    private bool showVertexTypeColor = false;

    [SerializeField]
    private bool showTriangulationEdges = false;

    [SerializeField]
    private bool showBackgroundColor = false;

    [SerializeField]
    private Color backgroundColor = new Color(1f, 1f, 1f, 0.5f);


    void Awake()
    {
        // NOTE: im not sure why we need this repeated here, some lifecycle issue.
        polygonVertex = Instantiate(Resources.Load("Vertex", typeof(GameObject)), transform) as GameObject;
    }

    void Start()
    {
        polygonVertex = Instantiate(Resources.Load("Vertex", typeof(GameObject)), transform) as GameObject;
    }
    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T)) {
            showVertexTypeColor = !showVertexTypeColor;
            foreach(PolygonVertex vertex in vertices) {
                vertex.showTypeColor = showVertexTypeColor;
            }
        }

        if(Input.GetKeyDown(KeyCode.E)) {
            showTriangulationEdges = !showTriangulationEdges;
            foreach(GameEdge edge in triangulationGameEdges) {
                edge.show = showTriangulationEdges;
            }
        }

        if(showBackgroundColor) {
            DrawBackground(backgroundColor);
        }
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
        vertex.gameObject.name = "Vertex " + vector;
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
            if(e.y > prev.y && e.y > next.y && angle < 180) {
                e.Type = PolygonVertex._Type.Start;
            } else if(e.y > prev.y && e.y > next.y && angle > 180) { 
                e.Type = PolygonVertex._Type.Split;
            } else if(e.y < prev.y && e.y < next.y && angle < 180) {
                e.Type = PolygonVertex._Type.End;
            } else if(e.y < prev.y && e.y < next.y && angle > 180) {
                e.Type = PolygonVertex._Type.Merge;
            } else {
                e.Type = PolygonVertex._Type.Regular;
            }
        }
    }
    
    private void DrawBackground(Color color)
    {
        if(triangulationMesh) {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = color;
            Graphics.DrawMesh(triangulationMesh, Vector2.zero, Quaternion.identity, mat, 1);
        }
    }

    private void CreateTriangulationEdges()
    {
        int[] t = triangulationMesh.triangles;
        for(int offset = 0; offset < t.Count() - 2; offset+=3) {
            Vector3[] v = triangulationMesh.vertices;
            List<(Vector3, Vector3)> edges = new List<(Vector3, Vector3)> {
                (v[t[offset]], v[t[offset + 1]]),
                (v[t[offset + 1]], v[t[offset + 2]]),
                (v[t[offset + 2]], v[t[offset]]),
            };
            foreach((Vector3 start, Vector3 end) in edges) {
                GameEdge gameEdge = GameEdge.Create(gameObject, start, end);
                gameEdge.color = Color.red;
                this.triangulationGameEdges.Add(gameEdge);
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
            // Not settable.
        }
    }

    // Triangulate a polygon.
    public void Triangulate()
    {
        if(Direction == _Direction.CCW) {
            vertices.Reverse();
        }

        List<Vector2> vector2s = vertices.Select(v => v.ToVector()).ToList();

        triangulationMesh = Triangulator.Triangulate(new Polygon2D(vector2s)).CreateMesh();

        CreateTriangulationEdges();
    }

    // Merge a polygon with another one.
    public Polygon Merge(Polygon p)
    {
        throw new System.Exception("Not implemented");
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
