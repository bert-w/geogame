using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlaceLightsController : MonoBehaviour
{
    List<GameObject> vertexList;
    List<Edge> edgeList;

    public Camera mainCam;

    public GameObject mouseLight;

    public List<PolygonVertex> lights;

    public List<GameObject> eventQueue;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    void OnEnable()
    {
        mouseLight = Instantiate(mouseLight);
        mouseLight.name = "Mouse Light";
        mouseLight.GetComponent<SpriteRenderer>().color = Color.yellow;
    }

    // Update is called once per frame
    void Update()
    {
        mouseLight.transform.position = GetMousePosition();

        if (Input.GetButtonDown("Fire1")){
            GenerateVisibilityPolygon();
        }
    }

    void GenerateVisibilityPolygon()
    {
        Debug.Log("Computing Visibility Polygon");

        Vector3 mPos = GetMousePosition();

        eventQueue = GenerateEventQueue(mPos);

        for(var i = 0; i < eventQueue.Count; i++) {
            PolygonVertex vertex = eventQueue[i].GetComponent<PolygonVertex>();

        }
    }

    // Generate an event queue (radial sweep) for the visibility polygon from point mPos.
    List<GameObject> GenerateEventQueue(Vector3 mPos)
    {
        // Convert the list of vertices to polar coordinates so we can create an event queue for the radial sweep.
        for(var i = 0; i < vertexList.Count; i++) {
            PolygonVertex vertex = vertexList[i].GetComponent<PolygonVertex>();

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
        return vertexList.OrderBy(v => v.GetComponent<PolygonVertex>().polarCoordinates.y).ToList();
    }

    Vector3 GetMousePosition()
    {
        Vector3 mPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mPos.z = 10;
        return mPos;
    }

    public void SetValues(List<GameObject> vertices, List<Edge> edges)
    {
        vertexList = vertices;
        edgeList = edges;
    }
}
