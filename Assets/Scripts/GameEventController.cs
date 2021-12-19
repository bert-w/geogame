using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEventController : MonoBehaviour
{

   

    public List<GameObject> vertexList;
    public List<Edge> edgeList = new List<Edge>();
    public GameObject vertex;
    public Camera mainCam;

    public float width;
    public Color color = Color.red;


    private bool polygonStarted = false;
    private LineRenderer polygonLine;
    private GameObject currVertex;
    private GameObject firstVertex;

    public void OnClick()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = 10; // select distance = 10 units from the camera
        var instPos = mainCam.ScreenToWorldPoint(mousePos);

        if (!polygonStarted){
            polygonStarted = true;
            firstVertex = Instantiate(vertex, instPos, Quaternion.identity); // saving the first vertex as a GameObject
            vertexList.Add(firstVertex);
            polygonLine.enabled = true;
            polygonLine.positionCount = 2;
            polygonLine.SetPosition(0, instPos);
            polygonLine.SetPosition(1, instPos);

        }
        else
        {
            if (CheckNotIntersect(instPos)){

                polygonLine.SetPosition(polygonLine.positionCount - 1, instPos); // fix location of previous line
                polygonLine.positionCount++;
                polygonLine.SetPosition(polygonLine.positionCount - 1, instPos); // start next line segment
                // create new vertex
                currVertex = Instantiate(vertex, instPos, Quaternion.identity);
                vertexList.Add(currVertex);
                // create new edge
                Edge createdEdge = new Edge(vertexList[vertexList.Count -2], vertexList[vertexList.Count - 1]);
                edgeList.Add(createdEdge);
                // create references in vertices to edge
                vertexList[vertexList.Count - 2].GetComponent<PolygonVertex>().nextEdge = createdEdge;
                vertexList[vertexList.Count - 1].GetComponent<PolygonVertex>().prevEdge = createdEdge;

            }
            else
            {
                Debug.Log("Not Possible!");
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        polygonLine = gameObject.AddComponent<LineRenderer>();
        polygonLine.material.color = color;
        polygonLine.widthMultiplier = width;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1")){
            OnClick();
        }
        else if (polygonStarted)
        {
            var mousePos = Input.mousePosition;
            mousePos.z = 10; // select distance = 10 units from the camera
            var instPos = mainCam.ScreenToWorldPoint(mousePos);
            polygonLine.SetPosition(polygonLine.positionCount - 1, instPos);
        }
    }


    // checks if the new added edge is legal and does not intersect previous edges
    public bool CheckNotIntersect(Vector3 currPos)
    {
        if (edgeList.Count == 0)
            return true;


        float ax = currPos.x;
        float ay = currPos.y;
        float bx = vertexList[vertexList.Count - 1].GetComponent<PolygonVertex>().x;
        float by = vertexList[vertexList.Count - 1].GetComponent<PolygonVertex>().y;
        for (int i = 0; i < edgeList.Count; i++)
        {
            var currEdge = edgeList[i];
            float cx = currEdge.startPointX;
            float cy = currEdge.startPointY;
            float dx = currEdge.startPointX;
            float dy = currEdge.startPointY;

            if (Intersect(ax, ay, bx, by, cx, cy, dx, dy))
            {
                return false;
            }

        }
        return true;
    }

    public bool CounterClockWise(float ax, float ay, float bx, float by, float cx, float cy)
    {
        return (cy - ay) * (bx - ax) > (by - ay) * (cx-ax);
    }

    public bool Intersect(float ax, float ay, float bx, float by, float cx, float cy, float dx, float dy)
    {
        bool bool1 = CounterClockWise(ax, ay, cx, cy, dx, dy) != CounterClockWise(bx, by, cx, cy, dx, dy);
        bool bool2 = CounterClockWise(ax, ay, bx, by, cx, cy) != CounterClockWise(ax, ay, bx, by, dx, dy);
        Debug.Log((bool1, bool2));
        return (bool1 & bool2);
    }
}
