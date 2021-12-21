using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEventController : MonoBehaviour
{
    public List<Edge> edgeList = new List<Edge>();
    public Camera mainCam;
    public float width;
    public Color color = Color.red;

    private bool polygonStarted = false;
    private LineRenderer polygonLine;

    public Polygon challengePolygon;

    public PlaceLightsController placeLightsController;

    public void OnClick(PolygonVertex snapToVertex)
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 10; // select distance = 10 units from the camera

        Polygon p = challengePolygon;

        if (!polygonStarted){
            polygonStarted = true;
            challengePolygon.Add(mousePos);
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
                newEdge = new Edge(p.vertices[p.vertices.Count - 1], snapToVertex);
                edgeList.Add(newEdge);
                // create references in vertices to edge
                p.vertices[p.vertices.Count - 1].GetComponent<PolygonVertex>().nextEdge = newEdge;
                snapToVertex.prevEdge = newEdge;

                // Pass current vertexlist to the placeLightsController and activate it.
                challengePolygon.edges = edgeList;
                
                placeLightsController.SetValues(challengePolygon);
                snapToVertex.Color = null;
                snapToVertex.Scale = null;
                placeLightsController.enabled = true;
                // Disable this controller.
                enabled = false;
                return;
            }
            polygonLine.SetPosition(polygonLine.positionCount - 1, mousePos); // fix location of previous line
            polygonLine.positionCount++;
            polygonLine.SetPosition(polygonLine.positionCount - 1, mousePos); // start next line segment
            // create new vertex
            challengePolygon.Add(mousePos);
            // create new edge
            newEdge = new Edge(p.vertices[p.vertices.Count -2], p.vertices[p.vertices.Count - 1]);
            edgeList.Add(newEdge);
            // create references in vertices to edge
            p.vertices[p.vertices.Count - 2].nextEdge = newEdge;
            p.vertices[p.vertices.Count - 1].prevEdge = newEdge;
            return;
        }
        else
        {
            Debug.Log("Not Possible!");
        }

  
    }

    // Start is called before the first frame update
    void Start()
    {
        challengePolygon = Instantiate(new GameObject().AddComponent<Polygon>());
        challengePolygon.name = "Challenge Polygon";
        challengePolygon.transform.SetParent(transform);

        polygonLine = gameObject.AddComponent<LineRenderer>();
        polygonLine.material.color = color;
        polygonLine.widthMultiplier = width;
        polygonLine.numCornerVertices = 1;
        polygonLine.numCapVertices = 1;

        placeLightsController = gameObject.GetComponent<PlaceLightsController>();
        placeLightsController.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        PolygonVertex snapToVertex = challengePolygon.vertices.Count > 0 ? isCloseToVertex(challengePolygon.vertices[0], 200f) : null;
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
    PolygonVertex isCloseToVertex(PolygonVertex vertex, float range)
    {
        var pos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        if(vertex.Distance(pos) < range) {
            vertex.Scale = 60;
            vertex.Color = Color.blue;
            return vertex;
        }

        vertex.Scale = null;
        vertex.Color = null;

        return null;
    }

    // checks if the new added edge is legal and does not intersect previous edges
    public bool CheckNotIntersect(Vector3 currPos)
    {
        if (edgeList.Count == 0)
            return true;

        Vector2 currPos2 = currPos;
        Vector2 prevPos = edgeList[edgeList.Count - 1].end;

        Edge tempEdge = new Edge(prevPos, currPos2);
        tempEdge.DebugDraw();
        for (int i = 0; i < edgeList.Count; i++)
        {
            var currEdge = edgeList[i];
            currEdge.DebugDraw();
            if (null != currEdge.Crosses(tempEdge))
            {
                return false;
            } 
        }

        return true;
    }


    public bool CheckNotIntersectOld(Vector3 currPos)
    {
        Polygon p = challengePolygon;
        if (edgeList.Count == 0)
            return true;


        float ax = currPos.x;
        float ay = currPos.y;
        float bx = p.vertices[p.vertices.Count - 1].GetComponent<PolygonVertex>().x;
        float by = p.vertices[p.vertices.Count - 1].GetComponent<PolygonVertex>().y;
        for (int i = 0; i < edgeList.Count; i++)
        {
            var currEdge = edgeList[i];
            float cx = currEdge.start.x;
            float cy = currEdge.start.y;
            float dx = currEdge.start.x;
            float dy = currEdge.start.y;

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
