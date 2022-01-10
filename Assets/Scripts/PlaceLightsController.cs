using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlaceLightsController : MonoBehaviour
{
    public Camera mainCam;

    public GameObject mouseLight;

    public List<PolygonVertex> lights;

    private List<PolygonVertex> eventQueue;

    public Polygon visibilityPolygon;

    private Polygon challengePolygon;

    private LineRenderer visibilityPolygonLine;


    // Start is called before the first frame update
    void Start()
    {    
        mouseLight = Instantiate(mouseLight);
        mouseLight.name = "Mouse Light";
        mouseLight.GetComponent<SpriteRenderer>().color = Color.yellow;

        visibilityPolygon = Instantiate(new GameObject().AddComponent<Polygon>());
        visibilityPolygonLine = new GameObject().AddComponent<LineRenderer>();
        visibilityPolygonLine.material = new Material(Shader.Find("Sprites/Default"));
        visibilityPolygonLine.name = "Visibility Polygon Line";
        visibilityPolygonLine.material.color = Color.yellow;
        visibilityPolygonLine.widthMultiplier = 10;
        visibilityPolygonLine.numCornerVertices = 1;
        visibilityPolygonLine.numCapVertices = 1;
    }

    // Update is called once per frame
    void Update()
    {
        mouseLight.transform.position = GetMousePosition();

        // if (Input.GetButtonDown("Fire1")){
            GenerateVisibilityPolygon();
        // }
    }

    void GenerateVisibilityPolygon()
    {
        Vector3 mPos = GetMousePosition();

        visibilityPolygon.empty();
        
        var visibilityPolygonEdges = GenerateVisibilityPolygon(mPos);

        // Goed checken
        foreach (var edge in visibilityPolygonEdges)
        {
            // Start and end moet van het event worden niet van de edge
            visibilityPolygon.Add(edge.start); 
            visibilityPolygon.Add(edge.end);
        }
        visibilityPolygon.Add(visibilityPolygonEdges.First().start);

        visibilityPolygonLine.SetPositions(visibilityPolygon.vertices.Select(v =>
        {
            return new Vector3(v.x, v.y, 0);
        }).ToArray());

        visibilityPolygonLine.positionCount = visibilityPolygon.vertices.Count;



        visibilityPolygon.empty();


        List<Edge> edges = challengePolygon.edges;

        //for(var i = 0; i < eventQueue.Count; i++) {
        //    PolygonVertex vertex = eventQueue[i].GetComponent<PolygonVertex>();
        //    Edge rayCast = new Edge(mPos, vertex.transform.position);
        //    Debug.DrawLine(mPos, vertex.transform.position, Color.blue, .1f);
        //    float length = rayCast.Length;
        //    bool intersected = false;
        //    for(var j = 0; j < edges.Count; j++) {
        //        Debug.DrawLine(edges[j].start, edges[j].end, Color.red, .1f);
        //        Vector3? intersection = rayCast.Crosses(edges[j]);
        //        if(intersection.HasValue) {
                    
        //            intersected = true;
        //            visibilityPolygon.Add(intersection.Value);
        //            break;
        //        }
        //    }
        //    if(!intersected) {
        //        visibilityPolygon.Add(vertex.ToVector());
        //    }
        //}

        //visibilityPolygonLine.SetPositions(visibilityPolygon.vertices.Select(v => {
        //    return new Vector3(v.x, v.y, 0);
        //}).ToArray());

        //visibilityPolygonLine.positionCount = visibilityPolygon.vertices.Count;

        visibilityPolygon.empty();
    }

    // Generate an event queue (radial sweep) for the visibility polygon from point mPos.
    private List<Event> GenerateEventQueue(Vector3 mPos)
    {
        // gewoon sorten en dan checken voor start en end voor adden

        /// General position cases
        /// 1. Start en end event coss => start event voor end niet relevant gewoon checken voor toevoegen
        /// 2. Start en start lijn trekken naar end event van beide de gene die niet intersect met de edge staat voor
        /// 3. End en end event beide direct uit de queue halen en alleen de voorste submitten

        var unsortedQueue = new List<Event>();

        // Find edge with smallest distance
        // set its degrees to to 0

        var tuple = SafeVertexFinder.Find(challengePolygon.edges, mPos);
        var startVertex = tuple.Item2;
        var startEdge = tuple.Item1;

        float minDegrees = PolarCoordinateBuilder.Build(startVertex, mPos).y;

        int edgeId = 0;
        foreach (var edge in challengePolygon.edges)
        {
            // TODO dubbel check x and y
            var Polar1 = PolarCoordinates(mPos, edge.start);
            var degrees1 = Polar1.y - minDegrees;
            if (degrees1 < 0)
            {
                degrees1 += 2 * Mathf.PI;
            }
            var Polar2 = PolarCoordinates(mPos, edge.end);
            var degrees2 = Polar2.y - minDegrees;
            if (degrees2 < 0)
            {
                degrees2 += 2 * Mathf.PI;
            }

            if (SafeVertexFinder.IsStartVertex(edge, mPos))
            {
                var startEvent = new Event(Polar1.x, Polar2.x, degrees1, edge, EventType.Start);
                unsortedQueue.Add(startEvent);
                // Since the edges are non crossing we use the start event distance for easy searching in the binary search tree
                var endEvent = new Event(Polar1.x, Polar2.x, degrees2, edge, EventType.End);
                startEvent.Id = edgeId;
                endEvent.Id = edgeId;
                unsortedQueue.Add(endEvent);
            }
            else
            {
                var startEvent = new Event(Polar2.x, Polar1.x, degrees2, edge, EventType.Start);
                unsortedQueue.Add(startEvent);
                // Since the edges are non crossing we use the start event distance for easy searching in the binary search tree
                var endEvent = new Event(Polar2.x, Polar1.x, degrees1, edge, EventType.End);
                startEvent.Id = edgeId;
                endEvent.Id = edgeId;
                unsortedQueue.Add(endEvent);
            }

            edgeId++;
        }

        return unsortedQueue.OrderBy(queueItem => queueItem.Degrees).ToList();
    }

    public float PDistance(Vector2 point, Edge edge)
    {
        float A = point.x - edge.start.x; // position of point rel one end of line
        float B = point.y - edge.start.y;
        float C = edge.end.x - edge.start.x; // vector along line
        float D = edge.end.y - edge.start.y;
        float E = -D; // orthogonal vector
        float F = C;

        float dot = A * E + B * F;
        float len_sq = E * E + F * F;

        return (float)Mathf.Abs(dot) / Mathf.Sqrt(len_sq);
    }

    private List<Edge> GenerateVisibilityPolygon(Vector3 mPos)
    {
        var polygon = new List<Edge>();
        var queue = GenerateEventQueue(mPos);
        var state = new RedBlackTree<Event>();

        for (int i = 0; i < queue.Count; i+=2)
        {
            var event1 = queue[i];
            var event2 = queue[i+1];

            if (event1.Type == EventType.Start && event2.Type == EventType.Start)
            {
                state.Add(event1);
                state.Add(event2);
            }
            else if (event1.Type == EventType.Start && event2.Type == EventType.Start)
            {
                state.Delete(event1);
                state.Delete(event2);
            }
            else if (event1.Type == EventType.Start && event2.Type == EventType.End)
            {
                state.Add(event1);
                state.Delete(event2);
            }
            else if (event1.Type == EventType.End && event2.Type == EventType.Start)
            {
                state.Delete(event1);
                state.Add(event2);
            }

            var minEvent = state.FindMin();

            if (minEvent != null)
            {
                polygon.Add(minEvent.Edge);
            }
        }

        return polygon.Distinct().ToList();
    }

    private Edge FindStartIndex(Edge edge1, Edge edge2, Vector3 mPos)
    {
        var ray1 = CreateRayCastToLongestVertex(edge1, mPos);

        if (ray1.Crosses(edge2) != null)
        {
            // The ray crossed the second edge, thus we return its value
            return edge2;
        }

        return edge1;
    }

    private Edge CreateRayCastToLongestVertex(Edge edge, Vector2 mPos)
    {
        var firstRay = new Edge(mPos, edge.start);
        var secondRay = new Edge(mPos, edge.end);

        return firstRay.Length > secondRay.Length ? firstRay : secondRay;
    }

    private Vector3 PolarCoordinates(Vector3 mPos, Vector2 vertex)
    {
        // Set coordinates relative to the current mouse position.
        Vector3 vPos = mPos - new Vector3(vertex.x, vertex.y, 0);

        // https://www.mathsisfun.com/polar-cartesian-coordinates.html
        float hypothenuse = Mathf.Sqrt(Mathf.Pow(vPos.x, 2) + Mathf.Pow(vPos.y, 2));

        float tangent = Mathf.Atan(vPos.y / vPos.x);

        // Quadrant correction
        if (vPos.x < 0)
        {
            // Quadrant 2 or 3.
            tangent += Mathf.PI;
        }
        else if (vPos.x > 0 && vPos.y < 0)
        {
            // Quadrant 4.
            tangent += 2 * Mathf.PI;
        }

        // Assign polar coordinates to the vertex object.
        return new Vector3(hypothenuse, tangent);
    }

    Vector3 GetMousePosition()
    {
        Vector3 mPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mPos.z = 10;
        return mPos;
    }

    public void SetValues(Polygon polygon)
    {
        challengePolygon = polygon;
    }
}
