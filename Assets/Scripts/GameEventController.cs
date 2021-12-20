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

    public void OnClick(PolygonVertex? snapToVertex)
    {
        var mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 10; // select distance = 10 units from the camera

        if (!polygonStarted){
            polygonStarted = true;
            firstVertex = Instantiate(vertex, mousePos, Quaternion.identity); // saving the first vertex as a GameObject

            vertexList.Add(firstVertex);
            polygonLine.enabled = true;
            polygonLine.positionCount = 2;
            polygonLine.SetPosition(0, mousePos);
            polygonLine.SetPosition(1, mousePos);

            return;
        }

        if (CheckNotIntersect(mousePos)){
            Edge newEdge;
            if(snapToVertex) {
                // Snap to the given vertex.
                polygonLine.SetPosition(polygonLine.positionCount - 1, snapToVertex.transform.position); // fix location of previous line
                polygonLine.positionCount++;
                polygonLine.SetPosition(polygonLine.positionCount - 1, snapToVertex.transform.position); 
                newEdge = new Edge(vertexList[vertexList.Count - 1], snapToVertex.gameObject);
                edgeList.Add(newEdge);
                // create references in vertices to edge
                vertexList[vertexList.Count - 1].GetComponent<PolygonVertex>().nextEdge = newEdge;
                snapToVertex.prevEdge = newEdge;

                // @TODO manage some sort of game state, like polygonDrawn = true
                return;
            }
            polygonLine.SetPosition(polygonLine.positionCount - 1, mousePos); // fix location of previous line
            polygonLine.positionCount++;
            polygonLine.SetPosition(polygonLine.positionCount - 1, mousePos); // start next line segment
            // create new vertex
            currVertex = Instantiate(vertex, mousePos, Quaternion.identity);
            vertexList.Add(currVertex);
            // create new edge
            newEdge = new Edge(vertexList[vertexList.Count -2], vertexList[vertexList.Count - 1]);
            edgeList.Add(newEdge);
            // create references in vertices to edge
            vertexList[vertexList.Count - 2].GetComponent<PolygonVertex>().nextEdge = newEdge;
            vertexList[vertexList.Count - 1].GetComponent<PolygonVertex>().prevEdge = newEdge;
            return;
        }

        Debug.Log("Not Possible!");
    }


    // Start is called before the first frame update
    void Start()
    {
        polygonLine = gameObject.AddComponent<LineRenderer>();
        polygonLine.material.color = color;
        polygonLine.widthMultiplier = width;
        polygonLine.numCornerVertices = 1;

    }

    // Update is called once per frame
    void Update()
    {
        PolygonVertex snapToVertex = vertexList.Count > 0 ? isCloseToVertex(vertexList[0], 200f) : null;
        if (Input.GetButtonDown("Fire1")){
            OnClick(snapToVertex);
        }
        else if (polygonStarted)
        {
            var mousePos = Input.mousePosition;
            mousePos.z = 10; // select distance = 10 units from the camera
            var instPos = mainCam.ScreenToWorldPoint(mousePos);
            polygonLine.SetPosition(polygonLine.positionCount - 1, instPos);
        }
    }

    // Check if the pointer is close to the first vertex in the vertexList. Returns the vertex if true.
    PolygonVertex isCloseToVertex(GameObject vertex, float range)
    {
        var pos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        if(firstVertex) {
            PolygonVertex _vertex = vertex.GetComponent<PolygonVertex>();
            if(_vertex.Distance(pos) < range) {
                _vertex.SetScale(60);
                _vertex.SetColor(Color.red);
                _vertex.GetComponent<SpriteRenderer>().color = Color.red;
                return _vertex;
            } else {
                _vertex.SetScale(null);
                _vertex.SetColor(null);
                _vertex.GetComponent<SpriteRenderer>().color = Color.black;
            }
        }

        return null;
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
