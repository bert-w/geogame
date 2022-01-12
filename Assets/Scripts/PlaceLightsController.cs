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
    
    public float LineWidth;

    public Color LineColor = Color.black;

    // List with "confirmed" visibility polygons
    public List<Polygon> visibilityPolygonList; 
    // List with polygons to visualize, I.e. while still moving the light
    public List<Polygon> drawVisibilityPolygonList; 

    private Polygon challengePolygon;

    private LineRenderer visibilityPolygonLine;


    // Start is called before the first frame update
    void Start()
    {    
        mouseLight = Instantiate(mouseLight, transform);
        mouseLight.name = "Mouse Light";
        mouseLight.GetComponent<SpriteRenderer>().color = Color.yellow;

        visibilityPolygon = new GameObject().AddComponent<Polygon>();
        visibilityPolygon.name = "Visibility Polygon";
        visibilityPolygon.transform.SetParent(transform);
        visibilityPolygonLine = GetComponent<LineRenderer>();
        visibilityPolygonLine.material.color = LineColor;
        visibilityPolygonLine.widthMultiplier = LineWidth;
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
            // TODO de punten staan in order can visibility dus voor het teken moet je alleen kijken wel punt overlap en op basis daarvan start en end swappen
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

        visibilityPolygon.empty();
    }

    // Generate an event queue (radial sweep) for the visibility polygon from point mPos.
    private List<Event> GenerateEventQueue(Vector3 mPos)
    {
        var unsortedQueue = new List<Event>();

        var tuple = SafeVertexFinder.Find(challengePolygon.edges, mPos);
        var startVertex = tuple.Item2;
        var startEdge = tuple.Item1;

        float minDegrees = PolarCoordinateBuilder.Build(startVertex, mPos).y;

        int edgeId = 0;
        foreach (var edge in challengePolygon.edges)
        {
            // TODO dubbel check x and y
            var Polar1 = PolarCoordinateBuilder.Build(mPos, edge.start);
            var degrees1 = Polar1.y - minDegrees;
            if (degrees1 < 0)
            {
                degrees1 += 2 * Mathf.PI;
            }
            var Polar2 = PolarCoordinateBuilder.Build(mPos, edge.end);
            var degrees2 = Polar2.y - minDegrees;
            if (degrees2 < 0)
            {
                degrees2 += 2 * Mathf.PI;
            }

            if (SafeVertexFinder.IsStartVertex(edge, mPos))
            {
                var startEvent = new Event(mPos, Polar1.x, Polar2.x, degrees1, edge, EventType.Start);
                unsortedQueue.Add(startEvent);
                // Since the edges are non crossing we use the start event distance for easy searching in the binary search tree
                var endEvent = new Event(mPos, Polar1.x, Polar2.x, degrees2, edge, EventType.End);
                startEvent.Id = edgeId;
                endEvent.Id = edgeId;
                unsortedQueue.Add(endEvent);
            }
            else
            {
                var startEvent = new Event(mPos, Polar2.x, Polar1.x, degrees2, edge, EventType.Start);
                unsortedQueue.Add(startEvent);
                // Since the edges are non crossing we use the start event distance for easy searching in the binary search tree
                var endEvent = new Event(mPos, Polar2.x, Polar1.x, degrees1, edge, EventType.End);
                startEvent.Id = edgeId;
                endEvent.Id = edgeId;
                unsortedQueue.Add(endEvent);
            }

            edgeId++;
        }

        return unsortedQueue.OrderBy(queueItem => queueItem.Degrees).ToList();
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
            else if (event1.Type == EventType.End && event2.Type == EventType.End)
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

                minEvent.Edge.DebugDraw();
            }

            if (minEvent != null)
            {
                polygon.Add(minEvent.Edge);
            }
        }

        return polygon.Distinct().ToList();
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

    // merges currently existing visibility polygons with the latest polygon
    // can be used both for updating the polygons to be drawn and 
    // placing a new light
    public void MergeVisibilityPolygons(List<Polygon> visibilityPolygonList, Polygon newVisibility)
    {
        // all polygons in visibilityPolygonList do not intersect
        // create list where we store all polygons which intersect with new visibility
        // iteratively union the latest polygon with the old polygons
        // finally, return new polygon visibility list
        var tempPolygonList = new List<Polygon>();
        
    }

    public void MergeTwoPolygons(Polygon polygon1, Polygon polygon2, List<(PolygonVertex, Edge)> links)
    {
        // Find point with highest x
        PolygonVertex highest1 = polygon1.PolygonYMax();
        PolygonVertex highest2 = polygon2.PolygonYMax();
        PolygonVertex highestVertex = (highest1.y > highest2.y) ? highest1 : highest2;
        PolygonVertex currVertex = highestVertex;
        Edge prevEdge = polygon1.edges[1]; // PLACEHOLDER EDGE
        // @TODO start new polygon
        // @TODO "move" over the edge starting at the highest point
        // @TODO something to ensure we are moving in the right direction
        while (!currVertex.IsEqual(highestVertex))
        {
            var maxAngle = CalcAngleEdges(prevEdge, currVertex.nextEdge);
            var maxEdge = prevEdge;
            foreach (var item in links)
            {
                if ((item.Item1.IsEqual(currVertex))){
                    var currAngle = CalcAngleEdges(prevEdge, item.Item2);
                    if (currAngle > maxAngle){
                        maxAngle = currAngle;
                        maxEdge = item.Item2;
                    }
                }
            }

            // @TODO add maxEdge to polygon

        }
        
        // @TODO return polygon

    }

    public float CalcAngleEdges(Edge edge1, Edge edge2)
    {
        return 10.0f;
    }


    // calculates the intersections between 2 visibility polygons
    // places new vertices on segments with intersections
    // we naively check intersections between each segment
    // since segments might overlap, we need special cases for overlapiing segments
    // returns a list with vertices and which edges from different polygons they are linked to
    // @TODO implement as sweepline?
    // @TODO move part of the code to edge class?
    // @TODO make dictionary?
    public List<(PolygonVertex, Edge)> CalculateIntersections(Polygon polygon1, Polygon polygon2)
    {
        List <(PolygonVertex, Edge)> newLinks = new List<(PolygonVertex, Edge)>();

        for (int i = 0; i < polygon1.edges.Count; i++)
        {
            Edge edge1 = polygon1.edges[i];
            for (int j = 0; j < polygon2.edges.Count; j++)
            {
                Edge edge2 = polygon2.edges[j];
                var crossingPoint = edge1.Crosses(edge2);
                // Case 1: both edges are the same edge
                if (edge1.IsEqual(edge2))
                {
                    // if the edges are "reversed"
                    if (edge1.IsReversed(edge2)) {
                        // start vertex of edge 1 is end vertex of edge 2
                        newLinks.Add((edge1.startVertex, edge2));
                        newLinks.Add((edge1.startVertex, edge2.endVertex.nextEdge));
                        newLinks.Add((edge1.endVertex, edge2));
                        newLinks.Add((edge1.endVertex, edge2.startVertex.prevEdge));

                        newLinks.Add((edge2.startVertex, edge1));
                        newLinks.Add((edge2.startVertex, edge1.endVertex.nextEdge));
                        newLinks.Add((edge2.endVertex, edge1));
                        newLinks.Add((edge2.endVertex, edge1.startVertex.prevEdge));

                    }
                    // otherwise
                    else
                    {
                        newLinks.Add((edge1.startVertex, edge2));
                        newLinks.Add((edge1.startVertex, edge2.startVertex.prevEdge));
                        newLinks.Add((edge1.endVertex, edge2));
                        newLinks.Add((edge1.endVertex, edge2.endVertex.nextEdge));

                        newLinks.Add((edge2.startVertex, edge1));
                        newLinks.Add((edge2.startVertex, edge1.startVertex.prevEdge));
                        newLinks.Add((edge2.endVertex, edge1));
                        newLinks.Add((edge2.endVertex, edge1.endVertex.prevEdge));

                    }
                }

                // Case 2: Edges overlap
                else if (edge1.Overlaps(edge2))
                {
                    // @TODO calculate "intersections"
                    // @TODO insert new edges and vertices in both polygons
                    // @TODO create "jumping points" between polygons and add them to newlinks
                }

                // Case 3: Edges intersect
                else if (null != crossingPoint)
                {
                    // @TODO insert vertex for both polygons
                    // @TODO split edge in two edges for both polygons
                    // @TODO create "jumping points" between polygons and add them to newlinks
                }
            }
        }

        return newLinks;

    }
}
