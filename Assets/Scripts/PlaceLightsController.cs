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

        // Find edge with smallest distance
        // set its degrees to to 0

        float minDistance = float.MaxValue;
        float minDegrees = float.MaxValue;
        Edge contendors = challengePolygon.edges[0];
        foreach (var edge in challengePolygon.edges)
        {
            var Polar1 = PolarCoordinates(mPos, edge.start);
            var Polar2 = PolarCoordinates(mPos, edge.end);

            if (Polar1.x == minDistance)
            {
                var correctEdge = FindStartIndex(contendors, edge, mPos);
                if (correctEdge == edge)
                {
                    minDistance = Polar1.x;
                    minDegrees = Polar1.y;
                }

                break;
            }

            if (Polar2.x == minDistance)
            {
                var correctEdge = FindStartIndex(contendors, edge, mPos);
                if (correctEdge == edge)
                {
                    minDistance = Polar1.x;
                    minDegrees = Polar1.y;
                }

                break;
            }

            if (Polar1.x < minDistance)
            {
                minDistance = Polar1.x;
                minDegrees = Polar1.y;
                contendors = edge;
            }

            if (Polar2.x < minDistance)
            {
                minDistance = Polar2.x;
                minDegrees = Polar2.y;
                contendors = edge;
            }
        }
        


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

            if (degrees1 < degrees2)
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

        var sortedQueue = unsortedQueue.OrderBy(queueItem => queueItem.Degrees).ToList();

        Event event1 = sortedQueue[0];
        Event event2 = sortedQueue[1];
        var edge1Found = false;
        var edge2Found = false;
        for (int i = 2; i < sortedQueue.Count; i++)
        {
            var eventItem = sortedQueue[i];
            if (!edge2Found && eventItem.Edge == event1.Edge)
            {
                edge1Found = true;
            }

            if (!edge1Found && eventItem.Edge == event2.Edge)
            {
                edge2Found = true;
            }

            if (edge2Found && eventItem.Edge == event1.Edge)
            {
                event1.Type = EventType.End;
                event1.Degrees = Mathf.PI * 2;
                event1.SwapDistances();
                eventItem.Type = EventType.Start;
                eventItem.SwapDistances();
                break;
            }

            if (edge1Found && eventItem.Edge == event2.Edge)
            {
                event2.Type = EventType.End;
                event2.Degrees = Mathf.PI * 2;
                event1.SwapDistances();
                eventItem.Type = EventType.Start;
                eventItem.SwapDistances();
                break;
            }
        }

        return sortedQueue.OrderBy(queueItem => queueItem.Degrees).ToList();
    }

    private List<Edge> GenerateVisibilityPolygon(Vector3 mPos)
    {
        var polygon = new List<Edge>();
        var queue = GenerateEventQueue(mPos);
        var state = new RedBlackTree<Event>();

        // Find the event with the smallest start start distance
        //var startIndex = FindStartIndex(queue, mPos);
        //var startIndex = queue
        //    .FindIndex(eventItem =>
        //        eventItem.Type == EventType.Start &&
        //        // Dit kan nog een conflict zijn als er twee punten zijn met dezelfde start distance
        //        eventItem.StartDistance == queue.Where(eventItem => eventItem.Type == EventType.Start).Min(eventItem => eventItem.StartDistance));
        var startIndex = 0;

        // Set initial state
        polygon.Add(queue[startIndex].Edge);
        state.Add(queue[startIndex]);

        Debug.Log(startIndex);

        //TODO remove
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

            // General possition cases
            if (currentEvent.Degrees == nextEvent.Degrees)
            {
                if (currentEvent.Type == EventType.Start &&
                    nextEvent.Type == EventType.End)
                {
                    // Current event cannot be visible because it is blocked by
                    // the end vertex of the next event which has the same position

                    // First delete current event from the state
                    state.Delete(nextEvent);

                    // Add current
                    state.Add(currentEvent);

                    // Do not submit and skip next iteration by increasing the counter
                    counter++;

                    // TODO check
                    var minEvent2 = state.FindMin();

                    if (minEvent2 != null)
                    {
                        polygon.Add(minEvent2.Edge);
                    }

                    continue;
                } else if (currentEvent.Type == EventType.End &&
                    nextEvent.Type == EventType.End)
                {
                    state.Delete(currentEvent);
                    state.Delete(nextEvent);

                    // TODO check
                    var minEvent2 = state.FindMin();

                    if (minEvent2 != null)
                    {
                        polygon.Add(minEvent2.Edge);
                    }

                    continue;

                    // Check for add
                }

                //TODO current and next are end delete both skip iteration
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
