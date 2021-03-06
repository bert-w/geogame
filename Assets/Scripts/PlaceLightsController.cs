using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.Geometry.Polygon;
using Util.Algorithms.Polygon;
using Util.Geometry.Contour;
using UnityEngine.UI;
using System;

public class PlaceLightsController : MonoBehaviour
{
    public Camera mainCam;

    private System.Random _random = new System.Random(); 

    private GameObject mouseLight;
    public GameObject mouseLightPrefab;

    public List<PolygonVertex> lights;

    private List<PolygonVertex> eventQueue;

    public GameObject visibilityPolygonPrefab;

    public bool isDrawing;
    
    Polygon currentVisibilityPolygon;

    public float LineWidth;
    public Material mat;
    public float drawDuration = .2f;

    public Button skipButton;

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
    public Text livePercentageText;
    public Text scorePlayer1;
    public Text scorePlayer2;
    public Text playerTurn;
    private string playerTurnString = "Player 1's turn";

    private float coverPercentage = 0f;
    private float challengePolygonArea;

    // Start is called before the first frame update
    private void OnEnable()
    {
        isDrawing = false;

        skipButton.interactable = true;

        mouseLight = CreateMouseLight("Mouse Light", Vector3.zero);

        playerTurnString = playerTurnString == "Player 1's turn" ? "Player 2's turn" : "Player 1's turn";
        playerTurn.text = playerTurnString;

        currentVisibilityPolygon = Instantiate(visibilityPolygonPrefab, transform).GetComponent<Polygon>();
        currentVisibilityPolygon.name = "Mouse Visibility Polygon";
        visibilityPolygonLine = currentVisibilityPolygon.GetComponentInParent<LineRenderer>();
        visibilityPolygonLine.material.color = LineColor;
        visibilityPolygonLine.widthMultiplier = LineWidth;

        percentageText.text = string.Format("{0:P2}", (coverPercentage));

        // @ TODO calculate this by triangulation
        challengePolygonArea = challengePolygon.GetArea();
        //Debug.Log("areas are the same: " + (ChangePolToPol2D(challengePolygon).Area == challengePolygon.GetArea()));
        
        challengeFinished = false;
    }

    GameObject CreateMouseLight(string name, Vector3 position)
    {
        GameObject mouseLight = Instantiate(mouseLightPrefab, position, Quaternion.identity, transform);
        mouseLight.name = name;
        mouseLight.GetComponent<SpriteRenderer>().color = Color.yellow;
        return mouseLight;
    }

    // Update is called once per frame
    void Update()
    {
        mouseLight.transform.position = GetMousePosition();
        if(isDrawing) {
            // Stepwise execution is busy, dont accept any inputs.
            return;
        }
        if (challengePolygon.PointInPolygon(mouseLight.transform.position))
        {
            // Draw current visibility polygon every time.
            CreateNewVisibilityPolygon(currentVisibilityPolygon);
            visibilityPolygonLine.SetPositions(currentVisibilityPolygon.Vertices.Select(v =>
            {
                return (Vector3)v.ToVector();
            }).ToArray());
            visibilityPolygonLine.positionCount = currentVisibilityPolygon.Vertices.Count;

            if (Input.GetButtonDown("Fire1") && !challengeFinished)
            {
                // On click, instantiate a new visibility polygon and save it in the list.
                Polygon instance = Instantiate(visibilityPolygonPrefab, transform).GetComponent<Polygon>();
                instance.name = "Visibility Polygon " + (visibilityPolygonList.Count + 1);
                visibilityPolygonList.Add(CreateNewVisibilityPolygon(instance, PlayerScore.explanations));
                AddLightOnMousePosition();
                StartCoroutine(MergeVisibilityPolygons(visibilityPolygonList, percentageText, true));
            } else {
                List<Polygon> tempList = visibilityPolygonList.ToList();
                tempList.Add(currentVisibilityPolygon);
                StartCoroutine(MergeVisibilityPolygons(tempList, livePercentageText));
            }
        } else if(Input.GetButtonDown("Fire1")) {
            gameEventController.PlayAudio("error", 0.5f);
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

        pol.Triangulate();

        return pol;
    }

    Polygon2D ChangePolToPol2D(Polygon pol)
    {
        
        var vectorList = new List<Vector2>();
        foreach (var vert in pol.Vertices)
        {
            // generate 2 random neglible offsets for each vertex
            float x_eps = (float)(_random.NextDouble() - .5f) * 0.00001f;
            float y_eps = (float)(_random.NextDouble() - .5f) * 0.00001f;

            var _vertex = vert.ToVector();
            _vertex.x += x_eps;
            _vertex.y += y_eps;

            vectorList.Add(_vertex);
        }
        IEnumerable<Vector2> collection = vectorList;
        return new Polygon2D(collection);
    }

    /// <summary>
    /// Create a visibility polygon using the current mouse position.
    /// </summary>
    /// <param name="visibilityPolygon">An existing GameObject to use for the visibility polygon. This allows you to 
    /// pass the same gameobject if you want to re-render a given object with a new mouse position.</param>
    Polygon CreateNewVisibilityPolygon(Polygon visibilityPolygon, bool draw = false)
    {
        visibilityPolygon.Empty();
        Vector3 mPos = GetMousePosition();
        var visibilityPolygonEdges = GenerateVisibilityPolygon(mPos, draw);

        Vector2 previous = visibilityPolygonEdges[0].start;
        Edge previousEdge = visibilityPolygonEdges[0];
        visibilityPolygon.Add(previous);
        visibilityPolygon.Add(previousEdge.end);

        for (int i = 1; i < visibilityPolygonEdges.Count; i++)
        {
            var edge = visibilityPolygonEdges[i];

            if (edge.start == previousEdge.start || edge.start == previousEdge.end)
            {
                visibilityPolygon.Add(edge.end);
            }
            else if (edge.end == previousEdge.start || edge.end == previousEdge.end)
            {
                visibilityPolygon.Add(edge.start);
            }
            else
            {
                visibilityPolygon.Add(edge.start);
            }

            previousEdge = edge;
        }

        visibilityPolygon.Add(visibilityPolygonEdges.First().start);

        //RemoveDuplicate(newVis, mPos);

        // Set to completed, so triangulation can take place and the mesh will become visible.
        visibilityPolygon.Completed = true;

        return visibilityPolygon;
    }

    void AddLightOnMousePosition()
    {
        gameEventController.PlayAudio("click", 0.1f);
        lights.Add(CreateMouseLight("Light " + (lights.Count + 1), GetMousePosition()).GetComponent<PolygonVertex>());
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
            var Polar1 = PolarCoordinateBuilder.Build(edge.start, mPos);
            float degrees1 = Polar1.y - minDegrees;
            if (degrees1 < 0)
            {
                degrees1 += (2 * Mathf.PI);
            }
            var Polar2 = PolarCoordinateBuilder.Build(edge.end, mPos);
            float degrees2 = Polar2.y - minDegrees;
            if (degrees2 < 0)
            {
                degrees2 += (2 * Mathf.PI);
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

    private List<Edge> GenerateVisibilityPolygon(Vector3 mPos, bool draw = false)
    {
        var polygon = new List<Edge>();
        var queue = GenerateEventQueue(mPos);
        var state = new RedBlackTree<Event>();
        Edge previousEmittedEdge = null;
        Vector2? partialVisibleEdge = null;

        if (draw)
        {
            isDrawing = true;
            StartCoroutine(_GenerateVisiblityPolygon(mPos));
        }


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
                    polygon.Remove(previousEdge);
                    var backEdge = new Edge(previousEdge.start, intersectionPoint.Value);
                    var intersectionEdge = new Edge(overlappingVertex, intersectionPoint.Value);
                    polygon.Add(backEdge);
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
                        previousEmittedEdge = minItem.Edge;
                        var intersectionEdge = new Edge(overlappingVertex, intersectionPoint.Value);
                        var backEdge = new Edge(intersectionPoint.Value, minItem.Edge.end);
                        polygon.Add(intersectionEdge);
                        polygon.Add(backEdge);
                    }

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

            if (minEvent != null && minEvent.Edge != previousEmittedEdge)
            {
                polygon.Add(minEvent.Edge);
                previousEmittedEdge = minEvent.Edge;
            }
        }

        if (queue[0].Type == EventType.Start && queue[1].Type == EventType.Start)
        {
            var overlappingVertex = queue[0].Edge.FindOverlappingVertex(queue[1].Edge).Value;
            var intersectionPoint = GetRayIntersectionPoint(mPos, overlappingVertex, state);

            if (intersectionPoint != null)
            {
                var previousEdge = polygon.Last();
                polygon.Remove(previousEdge);
                var backEdge = new Edge(previousEdge.start, intersectionPoint.Value);
                var intersectionEdge = new Edge(overlappingVertex, intersectionPoint.Value);
                intersectionEdge.DebugDraw(Color.blue, 1);
                polygon.Add(backEdge);
                polygon.Add(intersectionEdge);
            }
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

    // if the player is stuck, this function is used to skip the level
    public void SkipLevel()
    {
        foreach (Transform child in transform)
        {
            challengeFinished = true;
            //if(child.gameObject.name != "Mouse Light")
            Destroy(child.gameObject);
        }
        lights.Clear();
        Destroy(challengePolygon.gameObject);
        visibilityPolygonList.Clear();
        visibilityPolygonLine.positionCount = 0;

        gameEventController.enabled = true;

        percentageText.text = "0%";
        livePercentageText.text = "0%";

        coverPercentage = 0f;

        skipButton.interactable = false;

        enabled = false;

    }

    public void SetValues(Polygon polygon)
    {
        challengePolygon = polygon;
    }

    // merges currently existing visibility polygons with the latest polygon
    // can be used both for updating the polygons to be drawn and 
    // placing a new light
    /// <param name="visibilityPolygonList">List of visibility polygons to merge and calculate.</param>
    /// <param name="outputElement">Text object to put cover percentage in.</param>
    /// <param name="endIfCovered">Pass true to end the game when the coverPercentage is reached. This should be false
    /// for live calculation, so the game does not end immediately when 100% is reached when moving your mouse.</param>
    public IEnumerator MergeVisibilityPolygons(List<Polygon> visibilityPolygonList, Text outputElement, bool endIfCovered = false)
    {

        ICollection<Polygon2D> pol2dCol = new List<Polygon2D>();
        foreach (var item in visibilityPolygonList)
        {
            pol2dCol.Add(ChangePolToPol2D(item));
        }

        contourPolygon = (ContourPolygon)unionSweepLine.Union(pol2dCol);

        float contourPolArea = 0f;

        foreach (var item in contourPolygon.Contours)
        {
            contourPolArea += ChangeContourToPol(item).GetArea();
        }

        //Debug.Log("vis area the same:" + (contourPolArea == contourPolygon.Area));

        coverPercentage = contourPolArea / challengePolygonArea;
        if (coverPercentage > 1f)
            coverPercentage = 1f;
        outputElement.text = string.Format("{0:P2}", (coverPercentage));


        // dumb way to wait for coroutine to finish
        yield return new WaitUntil(()=>!isDrawing);


        // tolerance for mistakes
        if (coverPercentage >=0.998f && endIfCovered)
        {
            EndGame();
            yield return null;
        }

        yield return null;
    }

    /// <summary>
    /// Ends the game and shows the current player score.
    /// </summary>
    void EndGame()
    {
        //Debug.Log("%:" + coverPercentage);
        challengeFinished = true;
        gameEventController.PlayAudio("win", 0.1f);
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
        lights.Clear();
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
        percentageText.text = "0%";
        livePercentageText.text = "0%";

        coverPercentage = 0f;

        enabled = false;
    }


    void DrawEdge(Vector2 start, Vector2 end, Color color, float duration = .2f, float width = 10f)
    {
        var line = new GameObject().AddComponent<LineRenderer>();
        line.material = mat;
        line.startWidth = width;
        line.endWidth = width;
        line.positionCount = 2;
        line.startColor = color;
        line.endColor = color;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        Destroy(line.gameObject, duration);
        

    }

    IEnumerator _GenerateVisiblityPolygon(Vector3 mPos)
    {
        isDrawing = true;


        var polygon = new List<Edge>();
        var queue = GenerateEventQueue(mPos);
        var state = new RedBlackTree<Event>();
        Edge previousEmittedEdge = null;
        Vector2? partialVisibleEdge = null;

        for (int i = 0; i<queue.Count; i += 2)
        {
            var event1 = queue[i];
            var event2 = queue[i + 1];

           

            if (event1.Type == EventType.Start && event2.Type == EventType.Start)
            {
                var overlappingVertex = event1.Edge.FindOverlappingVertex(event2.Edge).Value;
                var intersectionPoint = GetRayIntersectionPoint(mPos, overlappingVertex, state);

                if (intersectionPoint != null)
                {
                    DrawEdge(mPos, event1.Edge.start, Color.red, drawDuration);
                    yield return new WaitForSeconds(drawDuration);


                    partialVisibleEdge = intersectionPoint;
                    var previousEdge = polygon.Last();
                    polygon.Remove(previousEdge);

                    DrawEdge(previousEdge.start, previousEdge.end, Color.white, width: 20);
                    yield return new WaitForSeconds(drawDuration);


                    var backEdge = new Edge(previousEdge.start, intersectionPoint.Value);
                    var intersectionEdge = new Edge(overlappingVertex, intersectionPoint.Value);
                    polygon.Add(backEdge);
                    polygon.Add(intersectionEdge);
                    
                    DrawEdge(intersectionEdge.start, intersectionEdge.end, Color.gray, width: 20);
                    DrawEdge(backEdge.start, backEdge.end, Color.gray, width: 20);
                    yield return new WaitForSeconds(drawDuration);
                   
                }

                state.Add(event1);
                state.Add(event2);

            }
            else if (event1.Type == EventType.End && event2.Type == EventType.End)
            {
                DrawEdge(mPos, event1.Edge.start, Color.blue, drawDuration);
                yield return new WaitForSeconds(drawDuration);


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
                        previousEmittedEdge = minItem.Edge;
                        var intersectionEdge = new Edge(overlappingVertex, intersectionPoint.Value);
                        var backEdge = new Edge(intersectionPoint.Value, minItem.Edge.end);
                        intersectionEdge.DebugDraw(Color.green, 1);
                        polygon.Add(intersectionEdge);
                        polygon.Add(backEdge);

                        DrawEdge(intersectionEdge.start, intersectionEdge.end, Color.gray, width: 20);
                        //yield return new WaitForSeconds(drawDuration);
                        DrawEdge(backEdge.start, backEdge.end, Color.gray, width: 20);
                        yield return new WaitForSeconds(drawDuration);
                    }

                    //polygon.Add(new Edge(overlappingVertex, intersectionPoint.Value));
                    partialVisibleEdge = intersectionPoint;
                }

                continue;
            }
            else if (event1.Type == EventType.Start && event2.Type == EventType.End)
            {
                DrawEdge(mPos, event1.Edge.start, Color.green, drawDuration);
                yield return new WaitForSeconds(drawDuration);
                state.Add(event1);
                state.Delete(event2);
            }
            else if (event1.Type == EventType.End && event2.Type == EventType.Start)
            {
                DrawEdge(mPos, event1.Edge.start, Color.white, drawDuration);
                yield return new WaitForSeconds(drawDuration);
                state.Delete(event1);
                state.Add(event2);
            }

            var minEvent = state.FindMin();

            if (minEvent != null && minEvent.Edge != previousEmittedEdge)
            {
                polygon.Add(minEvent.Edge);
                previousEmittedEdge = minEvent.Edge;

                DrawEdge(previousEmittedEdge.start, previousEmittedEdge.end, Color.gray, width: 20);
                yield return new WaitForSeconds(drawDuration);
            }
        }


        isDrawing = false;

        yield return null;
        
    }


    IEnumerator Wait(float duration = .2f)
    {
        yield return new WaitForSeconds(duration);
    }
}
