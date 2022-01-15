using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.Geometry.Polygon;
using Util.Algorithms.Polygon;
using Util.Geometry.Contour;
using UnityEngine.UI;

public class PlaceLightsController : MonoBehaviour
{
    public Camera mainCam;

    private GameObject mouseLight;
    public GameObject mouseLightPrefab;

    public List<PolygonVertex> lights;

    private List<PolygonVertex> eventQueue;

    public Polygon visibilityPolygon;

    public float LineWidth;

    public Color LineColor = Color.black;

    public GameEventController gameEventController;

    public GameObject gameOverScreen;

    // List with "confirmed" visibility polygons
    public List<Polygon> visibilityPolygonList;
    // List with polygons to visualize, I.e. while still moving the light
    public List<Polygon> drawVisibilityPolygonList;

    private Polygon challengePolygon;

    private LineRenderer visibilityPolygonLine;

    private UnionSweepLine unionSweepLine = new UnionSweepLine();

    private ContourPolygon contourPolygon;
    private bool challengeFinished = false;

    public Text percentageText;
    public Text scorePlayer1;
    public Text scorePlayer2;
    public Text playerTurn;
    private string playerTurnString = "Player 1's turn";

    private float coverPercentage = 0f;
    private float challengePolygonArea;

    // Start is called before the first frame update
    private void OnEnable()
    {
        mouseLight = Instantiate(mouseLightPrefab, transform);
        mouseLight.name = "Mouse Light";
        mouseLight.GetComponent<SpriteRenderer>().color = Color.yellow;

        playerTurnString = playerTurnString == "Player 1's turn" ? "Player 2's turn" : "Player 1's turn";
        playerTurn.text = playerTurnString;

        visibilityPolygon = new GameObject().AddComponent<Polygon>();
        visibilityPolygon.name = "Visibility Polygon";
        visibilityPolygon.transform.SetParent(transform);
        visibilityPolygon.BackgroundColor = new Color(1f, 1f, 0f, 0.2f);
        visibilityPolygon.showBackgroundColor = true;
        visibilityPolygonLine = GetComponent<LineRenderer>();
        visibilityPolygonLine.material.color = LineColor;
        visibilityPolygonLine.widthMultiplier = LineWidth;

        percentageText.text = string.Format("{0:P2}", (coverPercentage));

        // @ TODO calculate this by triangulation
        challengePolygonArea = ChangePolToPol2D(challengePolygon).Area;
        challengeFinished = false;
        coverPercentage = 0f;
    }
    /*
    void Start()
    {
        mouseLight = Instantiate(mouseLight, transform);
        mouseLight.name = "Mouse Light";
        mouseLight.GetComponent<SpriteRenderer>().color = Color.yellow;

        playerTurnString = playerTurnString == "Player 1's turn" ? "Player 2's turn" : "Player 1's turn";
        playerTurn.text = playerTurnString;

        visibilityPolygon = new GameObject().AddComponent<Polygon>();
        visibilityPolygon.name = "Visibility Polygon";
        visibilityPolygon.transform.SetParent(transform);
        visibilityPolygonLine = GetComponent<LineRenderer>();
        visibilityPolygonLine.material.color = LineColor;
        visibilityPolygonLine.widthMultiplier = LineWidth;

        percentageText.text = string.Format("{0:P2}", (coverPercentage));

        // @ TODO calculate this by triangulation
        challengePolygonArea = ChangePolToPol2D(challengePolygon).Area;
        challengeFinished = false;
        coverPercentage = 0f;
    }*/

    // Update is called once per frame
    void Update()
    {
        mouseLight.transform.position = GetMousePosition();

        GenerateVisibilityPolygon();
        if (Input.GetButtonDown("Fire1") && !challengeFinished){
            visibilityPolygonList.Add(CreateNewVisibilityPolygon());
            MergeVisibilityPolygons(visibilityPolygonList);
        }
    }

    Polygon ChangePol2DToPol(Polygon2D pol2d)
    {
        Polygon pol = new GameObject().AddComponent<Polygon>();
        pol.transform.SetParent(transform);
        foreach (var item in pol2d.Vertices)
        {
            pol.Vertices.Add(new PolygonVertex { x = item.x, y = item.y });
        }
        return pol;
    }

    Polygon ChangeContourToPol(Contour contour)
    {
        Polygon pol = new GameObject().AddComponent<Polygon>();
        pol.transform.SetParent(transform);

        foreach (var item in contour.Vertices)
        {
            pol.Add(new PolygonVertex { x = (float)item.x, y = (float)item.y });
        }

        return pol;
    }

    Polygon2D ChangePolToPol2D(Polygon pol)
    {
        var vectorList = new List<Vector2>();
        foreach (var vert in pol.Vertices)
        {
            vectorList.Add(vert.ToVector());
        }
        IEnumerable<Vector2> collection = vectorList;
        return new Polygon2D(collection);
    }

    void GenerateVisibilityPolygon()
    {
        Vector3 mPos = GetMousePosition();

        visibilityPolygon.empty();

        var visibilityPolygonEdges = GenerateVisibilityPolygon(mPos);

        Vector2 previous = visibilityPolygonEdges[0].start;
        Edge previousEdge = visibilityPolygonEdges[0];
        visibilityPolygon.Add(previous);

        for (int i = 1; i < visibilityPolygonEdges.Count; i++)
        {
            var edge = visibilityPolygonEdges[i];

            if (edge.start == previous)
            {
                previous = edge.end;
            }
            else
            {
                if (edge.end != previous)
                {
                    //Debug.Log(previous.ToString() + edge.ToString());
                }
                previous = edge.start;
            }

            visibilityPolygon.Add(previous);
        }

        visibilityPolygon.Add(visibilityPolygonEdges.First().start);


        // @TODO dit moet weg
        // RemoveDuplicate(visibilityPolygon, mPos);


        visibilityPolygonLine.SetPositions(visibilityPolygon.Vertices.Select(v =>
        {
            return new Vector3(v.x, v.y, 0);
        }).ToArray());

        visibilityPolygonLine.positionCount = visibilityPolygon.Vertices.Count;

        visibilityPolygon.Completed = true;
    }


    // @TODO update this with latest code
    Polygon CreateNewVisibilityPolygon()
    {
        Polygon visibilityPolygon = new GameObject().AddComponent<Polygon>();
        visibilityPolygon.transform.SetParent(transform);
        Vector3 mPos = GetMousePosition();
        var visibilityPolygonEdges = GenerateVisibilityPolygon(mPos);

        Vector2 previous = visibilityPolygonEdges[0].start;
        Edge previousEdge = visibilityPolygonEdges[0];
        visibilityPolygon.Add(previous);

        for (int i = 1; i < visibilityPolygonEdges.Count; i++)
        {
            var edge = visibilityPolygonEdges[i];

            if (edge.start == previous)
            {
                previous = edge.end;
            }
            else
            {
                if (edge.end != previous)
                {
                    //Debug.Log(previous.ToString() + edge.ToString());
                }
                previous = edge.start;
            }

            visibilityPolygon.Add(previous);
        }

        visibilityPolygon.Add(visibilityPolygonEdges.First().start);

        //RemoveDuplicate(newVis, mPos);

        return visibilityPolygon;
    }


    /// <summary>
    ///  removes duplicate vertices
    ///  sorts the in order of polar coordinates
    /// </summary>
    /// <param name="pol"></param>
    void RemoveDuplicate(Polygon pol, Vector2 mPos)
    {
        List<int> dltVertices = new List<int>();

        // looking for duplicate vertices
        for (int i = 0; i < pol.Vertices.Count; i++)
        {
            for (int j = 0; j < i; j++)
            {
                if (pol.Vertices[i] == pol.Vertices[j])
                {
                    dltVertices.Add(i);
                    break;
                }
            }
        }

        //Debug.Log("Removed vertices: " + dltVertices.Count);


        // removing duplicate vertices
        foreach (var item in dltVertices.OrderByDescending(v => v))
        {
            pol.Vertices.RemoveAt(item);
        }

        // sorting by polar coordinate angle, then by distance to mPos
        // does not work perfectly and needs to be changed
        pol.Vertices = pol.Vertices.OrderBy(o => PolarCoordinateBuilder.Build(o.ToVector(), mPos).y).ToList();
    }
    // Generate an event queue (radial sweep) for the visibility polygon from point mPos.
    private List<Event> GenerateEventQueue(Vector3 mPos)
    {
        var unsortedQueue = new List<Event>();

        var tuple = SafeVertexFinder.Find(challengePolygon.Edges, mPos);
        var startVertex = tuple.Item2;
        var startEdge = tuple.Item1;

        float minDegrees = PolarCoordinateBuilder.Build(startVertex, mPos).y;

        int edgeId = 0;
        foreach (var edge in challengePolygon.Edges)
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
                var reveredEdge = new Edge(edge.end, edge.start);
                var startEvent = new Event(mPos, Polar2.x, Polar1.x, degrees2, reveredEdge, EventType.Start);
                unsortedQueue.Add(startEvent);
                // Since the edges are non crossing we use the start event distance for easy searching in the binary search tree
                var endEvent = new Event(mPos, Polar2.x, Polar1.x, degrees1, reveredEdge, EventType.End);
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
        Edge previousEmittedEdge = null;
        Vector2? partialVisibleEdge = null;

        for (int i = 0; i < queue.Count; i += 2)
        {
            var event1 = queue[i];
            var event2 = queue[i + 1];

            if (event1.Type == EventType.Start && event2.Type == EventType.Start)
            {
                var overlappingVertex = event1.Edge.FindOverlappingVertex(event2.Edge).Value;
                var intersectionPoint = GetRayIntersectionPoint(mPos, overlappingVertex, state);

                if (intersectionPoint != null)
                {
                    partialVisibleEdge = intersectionPoint;
                    var previousEdge = polygon.Last();
                    polygon[polygon.Count - 1] = new Edge(previousEdge.start, intersectionPoint.Value);
                    var intersectionEdge = new Edge(overlappingVertex, intersectionPoint.Value);
                    intersectionEdge.DebugDraw(Color.blue, 1);
                    polygon.Add(intersectionEdge);
                }

                state.Add(event1);
                state.Add(event2);

            }
            else if (event1.Type == EventType.End && event2.Type == EventType.End)
            {
                state.Delete(event1);
                state.Delete(event2);

                var overlappingVertex = event1.Edge.FindOverlappingVertex(event2.Edge).Value;
                var intersectionPoint = GetRayIntersectionPoint(mPos, overlappingVertex, state);

                if (intersectionPoint != null)
                {
                    var minItem = state.FindMin();
                    if (minItem != null)
                    {
                        partialVisibleEdge = intersectionPoint;
                        var intersectionEdge = new Edge(overlappingVertex, intersectionPoint.Value);
                        var backEdge = new Edge(intersectionPoint.Value, minItem.Edge.end);
                        intersectionEdge.DebugDraw(Color.green, 1);
                        polygon.Add(intersectionEdge);
                        polygon.Add(backEdge);
                    }

                    //polygon.Add(new Edge(overlappingVertex, intersectionPoint.Value));
                    partialVisibleEdge = intersectionPoint;
                }

                continue;
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
                previousEmittedEdge = minEvent.Edge;
            }
        }

        foreach (var item in polygon.Distinct())
        {
            item.DebugDraw();
        }

        return polygon.Distinct().ToList();
    }

    private Vector2? GetRayIntersectionPoint(Vector2 lightPoint, Vector2 intersectionPoint, IBinarySearchTree<Event> state)
    {
        var direction = intersectionPoint - lightPoint;
        var ray = new Ray(lightPoint, direction);

        // TODO don't use large point but vector directly
        var largePoint = ray.GetPoint(10000);
        var rayEdge = new Edge(intersectionPoint, largePoint);

        var minEvent = state.FindMin();
        if (minEvent != null)
        {
            return rayEdge.Crosses(minEvent.Edge);
        }
        return null;
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
    public void MergeVisibilityPolygons(List<Polygon> visibilityPolygonList)
    {

        ICollection<Polygon2D> pol2dCol = new List<Polygon2D>();
        foreach (var item in visibilityPolygonList)
        {
            pol2dCol.Add(ChangePolToPol2D(item));
        }

        contourPolygon = (ContourPolygon)unionSweepLine.Union(pol2dCol);

        // something seems to go wrong here

        coverPercentage = contourPolygon.Area / challengePolygonArea;

        percentageText.text = string.Format("{0:P2}", (coverPercentage));
        if (coverPercentage >=0.99999f)
        {
            //Debug.Log("%:" + coverPercentage);
            challengeFinished = true;
            // @ TODO change way score is calculated
            var score = challengePolygon.Vertices.Count / visibilityPolygonList.Count;
            if (playerTurnString == "Player 1's turn")
            {
                PlayerScore.player1Score += score;
                scorePlayer1.text = string.Format("Player 1: {0}", PlayerScore.player1Score);
            }
            else
            {
                PlayerScore.player2Score += score;
                scorePlayer2.text = string.Format("Player 2: {0}", PlayerScore.player2Score);
            }
            // Disable this controller.
            // destroy all children
            foreach (Transform child in transform)
            {
                //if(child.gameObject.name != "Mouse Light")
                    Destroy(child.gameObject);
            }
            Destroy(challengePolygon.gameObject);
            visibilityPolygonList.Clear();
            visibilityPolygonLine.positionCount = 0;

            if (PlayerScore.CheckPlayerWon())
            {
                Debug.Log(PlayerScore.winningPlayer);
                gameOverScreen.SetActive(true);
            }
            else
                gameEventController.enabled = true;


            enabled = false;
            return;
        }


        /*foreach (var item in mergedVisibilityPolygon.Contours)
        {
            var poly = ChangeContourToPol(item);
            RemoveDuplicate(poly);
        }*/
            
        


    }

}



