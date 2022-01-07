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
        foreach (var edge in visibilityPolygonEdges)
        {
            visibilityPolygon.Add(edge.start); 
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

        foreach (var edge in challengePolygon.edges)
        {
            // TODO dubbel check x and y
            var startPolar = PolarCoordinates(mPos, edge.start);
            var endPolar = PolarCoordinates(mPos, edge.end);
            var startEvent = new Event(startPolar.x, endPolar.x, startPolar.y, edge, EventType.Start);
            unsortedQueue.Add(startEvent);
            // Since the edges are non crossing we use the start event distance for easy searching in the binary search tree
            var endEvent = new Event(startPolar.x, endPolar.x, endPolar.y, edge, EventType.End);
            unsortedQueue.Add(endEvent);
        }

        return unsortedQueue.OrderBy(queueItem => queueItem.Degrees).ToList();
    }

    private List<Edge> GenerateVisibilityPolygon(Vector3 mPos)
    {
        var polygon = new List<Edge>();
        var queue = GenerateEventQueue(mPos);
        var state = new RedBlackTree<Event>();

        // Find the event with the smallest start start distance
        var startIndex = queue
            .FindIndex(eventItem => 
                eventItem.Type == EventType.Start && 
                // Dit kan nog een conflict zijn als er twee punten zijn met dezelfde start distance
                eventItem.StartDistance == queue.Min(eventItem => eventItem.StartDistance));

        // Set initial state
        polygon.Add(queue[startIndex].Edge);
        state.Add(queue[startIndex]);

        var index = startIndex;
        var counter = startIndex;
        do
        {
            counter++;
            index = counter % queue.Count;

            var currentEvent = queue[index];
            Event nextEvent;
            if (index + 1 < queue.Count)
            {
                nextEvent = queue[index + 1];
            }
            else
            {
                nextEvent = queue[1];
            }

            if (currentEvent.Type == EventType.Start)
            {
                state.Add(currentEvent);
            }
            else
            {
                state.Delete(currentEvent);
            }

            var minEvent = state.FindMin();

            if (minEvent != null)
            {
                polygon.Add(minEvent.Edge);
            }
            

            // TODO handle special cases with overlapping events

        }
        while (index != startIndex);

        // circle checken
        return polygon.Distinct().ToList();
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
