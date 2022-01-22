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

    private Mesh _triangulationMesh;

    [field: SerializeField]
    public List<PolygonVertex> Vertices { get; set; } = new List<PolygonVertex>();

    private GameObject _polygonVertex;

    /// <summary>
    /// The edges of the polygon.
    /// </summary>
    [field: SerializeField]
    public List<Edge> Edges { get; set; } = new List<Edge>();

    /// <summary>
    /// The edges of the triangulation of the polygon.
    /// </summary>
    public List<GameEdge> TriangulationGameEdges { get; set; } = new List<GameEdge>();

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
    public bool ShowVertexTypeColor = false;

    [SerializeField]
    public Color VertexColor = new Color(0f, 0f, 0f);

    [SerializeField]
    public float VertexScale = 30f;

    [SerializeField]
    public bool ShowTriangulationEdges = false;

    [SerializeField]
    public bool ShowBackgroundColor = false;

    [SerializeField]
    public Color BackgroundColor = new Color(1f, 1f, 1f, 0.5f);


    void OnEnable()
    {
        Debug.Log("[Polygon] OnEnable");
    }

    void Awake()
    {
        Debug.Log("[Polygon] Awake");
        _polygonVertex = Instantiate(Resources.Load("Vertex", typeof(GameObject)), transform) as GameObject;
        _polygonVertex.GetComponent<SpriteRenderer>().color = VertexColor;
        _polygonVertex.transform.localScale = new Vector3(VertexScale, VertexScale, 0);
    }

    void Start()
    {
        //
    }
    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T)) {
            ShowVertexTypeColor = !ShowVertexTypeColor;
            foreach(PolygonVertex vertex in Vertices) {
                vertex.showTypeColor = ShowVertexTypeColor;
            }
        }

        if(Input.GetKeyDown(KeyCode.E)) {
            ShowTriangulationEdges = !ShowTriangulationEdges;
            foreach(GameEdge edge in TriangulationGameEdges) {
                edge.show = ShowTriangulationEdges;
            }
        }

        if(ShowBackgroundColor) {
            DrawBackground(BackgroundColor);
        }
    }

    

    public PolygonVertex Add(PolygonVertex v)
    {
        Vertices.Add(v);
        // Recalculate direction.
        _direction = Direction;
        return v;
    }

    public PolygonVertex Add(Vector2 vector)
    {
        PolygonVertex vertex = Instantiate(_polygonVertex, vector, Quaternion.identity, transform).GetComponent<PolygonVertex>();
        vertex.gameObject.name = "Vertex " + vector;
        return Add(vertex);
    }

    public void Empty()
    {
        foreach(PolygonVertex v in Vertices) {
            Destroy(v.gameObject);
        }
        Vertices.Clear();

        foreach(GameEdge e in TriangulationGameEdges) {
            Destroy(e.gameObject);
        }
        TriangulationGameEdges.Clear();
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
                Triangulate();
            }
        }
    }

    // Assign vertex types (split/merge/start/end) to each vertex.
    // Complexity: n
    private void AssignVertexTypes()
    {
        for(var i = 0; i < Vertices.Count; i++)
        {
            PolygonVertex e = Vertices[i];
            PolygonVertex prev = i == 0 ? Vertices[Vertices.Count - 1] : Vertices[i - 1];
            PolygonVertex next = i != Vertices.Count - 1 ? Vertices[i + 1] : Vertices[0];

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
        if(_triangulationMesh) {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = color;
            Graphics.DrawMesh(_triangulationMesh, Vector2.zero, Quaternion.identity, mat, 1);
        }
    }

    private void CreateTriangulationEdges()
    {
        int[] t = _triangulationMesh.triangles;

        for(int offset = 0; offset < t.Count() - 2; offset+=3) {
            Vector3[] v = _triangulationMesh.vertices;
            List<(Vector3, Vector3)> edges = new List<(Vector3, Vector3)> {
                (v[t[offset]], v[t[offset + 1]]),
                (v[t[offset + 1]], v[t[offset + 2]]),
                (v[t[offset + 2]], v[t[offset]]),
            };
            foreach((Vector3 start, Vector3 end) in edges) {
                GameEdge gameEdge = GameEdge.Create(gameObject, start, end);
                gameEdge.color = Color.red;
                TriangulationGameEdges.Add(gameEdge);
            }
        }
    }

    // To determine on which side of the edges the polygon lies, we find out CW or CCW direction using the sum over the edges.
    public _Direction Direction
    {
        get {
            float sum = 0;
            for(var i = 0; i < Vertices.Count; i++) {
                Vector2 next = i < Vertices.Count - 1 ? Vertices[i + 1].ToVector() : new Vector2(0, 0);
                Vector2 curr = Vertices[i].ToVector();
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
            Vertices.Reverse();
        }

        List<Vector2> vector2s = Vertices.Select(v => v.ToVector()).ToList();

        _triangulationMesh = Triangulator.Triangulate(new Polygon2D(vector2s)).CreateMesh();

        CreateTriangulationEdges();
    }

    // Merge a polygon with another one.
    public Polygon Merge(Polygon p)
    {
        throw new System.Exception("Not implemented");
    }

    public PolygonVertex PolygonYMax()
    {
        var maxY = this.Vertices[0].y;
        var maxVertex = this.Vertices[0];
        for (int i = 0; i < this.Vertices.Count; i++)
        {
            if (Vertices[i].y >= maxY)
            {
                maxY = Vertices[i].y;
                maxVertex = Vertices[i];
            }
        }
        return maxVertex;
    }

    public bool PointInPolygon(Vector2 pos)
    {
        bool result = false;
        int j = Vertices.Count - 1;
        for (int i = 0; i < Vertices.Count; i++){

            if (Vertices[i].y < pos.y && Vertices[j].y >= pos.y || Vertices[j].y < pos.y && Vertices[i].y >= pos.y)
            {
                if (Vertices[i].x + (pos.y - Vertices[i].y) / (Vertices[j].y - Vertices[i].y) * (Vertices[j].x - Vertices[i].x) < pos.x)
                {
                    result = !result;
                }
            
            }
            j = i;

        }
        return result;
    }
}
