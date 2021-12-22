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

    // List with "confirmed" visibility polygons
    public List<Polygon> visibilityPolygonList; 
    // List with polygons to visualize, I.e. while still moving the light
    public List<Polygon> drawVisibilityPolygonList; 

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

        eventQueue = GenerateEventQueue(mPos);

        List<Edge> edges = challengePolygon.edges;

        visibilityPolygon.empty();

        for(var i = 0; i < eventQueue.Count; i++) {
            PolygonVertex vertex = eventQueue[i].GetComponent<PolygonVertex>();
            Edge rayCast = new Edge(mPos, vertex.transform.position);
            Debug.DrawLine(mPos, vertex.transform.position, Color.blue, .1f);
            float length = rayCast.Length;
            bool intersected = false;
            for(var j = 0; j < edges.Count; j++) {
                Debug.DrawLine(edges[j].start, edges[j].end, Color.red, .1f);
                Vector3? intersection = rayCast.Crosses(edges[j]);
                if(intersection.HasValue) {
                    
                    intersected = true;
                    visibilityPolygon.Add(intersection.Value);
                    break;
                }
            }
            if(!intersected) {
                visibilityPolygon.Add(vertex.ToVector());
            }
        }

        visibilityPolygonLine.SetPositions(visibilityPolygon.vertices.Select(v => {
            return new Vector3(v.x, v.y, 0);
        }).ToArray());

        visibilityPolygonLine.positionCount = visibilityPolygon.vertices.Count;

        visibilityPolygon.empty();
    }

    // Generate an event queue (radial sweep) for the visibility polygon from point mPos.
    List<PolygonVertex> GenerateEventQueue(Vector3 mPos)
    {
        // Convert the list of vertices to polar coordinates so we can create an event queue for the radial sweep.
        for(var i = 0; i < challengePolygon.vertices.Count; i++) {
            PolygonVertex vertex = challengePolygon.vertices[i];

            // Set coordinates relative to the current mouse position.
            Vector3 vPos = mPos - vertex.transform.position;

            // https://www.mathsisfun.com/polar-cartesian-coordinates.html
            float hypothenuse = Mathf.Sqrt(Mathf.Pow(vPos.x, 2) + Mathf.Pow(vPos.y, 2));

            float tangent = Mathf.Atan(vPos.y / vPos.x);

            // Quadrant correction
            if(vPos.x < 0) {
                // Quadrant 2 or 3.
                tangent += Mathf.PI;
            } else if(vPos.x > 0 && vPos.y < 0) {
                // Quadrant 4.
                tangent += 2 * Mathf.PI;
            }

            // Assign polar coordinates to the vertex object.
            vertex.polarCoordinates = new Vector3(hypothenuse, tangent);

        }

        // Sort by y (polar) coordinate.
        return challengePolygon.vertices.OrderBy(v => v.polarCoordinates.y).ToList();
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

    }


    // calculates the intersections between 2 visibility polygons
    // places new vertices on segments with intersections
    // we naively check intersections between each segment
    // since segments might overlap, we need special cases for overlapiing segments
    // returns a list with vertices and which edges from different polygons they are linked to
    // @TODO implement as sweepline?
    // @TODO move part of the code to edge class?
    public List<(PolygonVertex, Edge)> CalculateIntersections(Polygon polygon1, Polygon polygon2)
    {
        List <(PolygonVertex, Edge)> newLinks = new List<(PolygonVertex, Edge)>();

        for (int i = 0; i < polygon1.edges.Count; i++)
        {
            Edge edge1 = polygon1.edges[i];
            for (int j = 0; j < polygon2.edges.Count; j++)
            {
                Edge edge2 = polygon2.edges[j];

                // Case 1: both edges are the same edge
                if (edge1.IsEqual(edge2))
                {
                    // if the edges are "reversed"
                    if (edge1.IsReversed(edge2)){
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

            }
        }

        return newLinks;

    }
}
