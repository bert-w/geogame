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
}
