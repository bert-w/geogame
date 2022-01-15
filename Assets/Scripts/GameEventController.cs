using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEventController : MonoBehaviour
{
    public List<Edge> edgeList = new List<Edge>();
    public Camera mainCam;
    public float LineWidth;

    public Color LineColor = Color.black;

    private bool polygonStarted = false;
    public LineRenderer polygonLine;

    public GameObject challengePolygonPrefab;

    Polygon challengePolygon;

    public AudioClip audioClipClick;
    public AudioClip audioClipError;

    public PlaceLightsController placeLightsController;

    public void OnClick(PolygonVertex snapToVertex)
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 10; // select distance = 10 units from the camera

        Polygon p = challengePolygon;

        if (!polygonStarted){
            GetComponent<AudioSource>().PlayOneShot(audioClipClick, 0.1f);
            polygonStarted = true;
            challengePolygon.Add(mousePos);
            polygonLine.enabled = true;
            polygonLine.positionCount = 2;
            polygonLine.SetPosition(0, mousePos);
            polygonLine.SetPosition(1, mousePos);

            return;
        }

        Edge intersectedEdge = IntersectsPolygon(mousePos);
        // Check if no edges are intersected and if there is, then we check if this is the snapvertex. This
        // allows us to snap to the snapvertex even if we cross an edge while near it.
        if (intersectedEdge == null || (snapToVertex && intersectedEdge.start.Equals(snapToVertex.ToVector()))) {
            GetComponent<AudioSource>().PlayOneShot(audioClipClick, 0.1f);
            Edge newEdge;
            if(snapToVertex) {
                // Snap to the given vertex.
                polygonLine.SetPosition(polygonLine.positionCount - 1, snapToVertex.transform.position); // fix location of previous line
                polygonLine.positionCount++;
                polygonLine.SetPosition(polygonLine.positionCount - 1, snapToVertex.transform.position); 
                newEdge = new Edge(p.Vertices[p.Vertices.Count - 1], snapToVertex);
                edgeList.Add(newEdge);
                // create references in vertices to edge
                p.Vertices[p.Vertices.Count - 1].GetComponent<PolygonVertex>().nextEdge = newEdge;
                snapToVertex.prevEdge = newEdge;

                snapToVertex.ResetShape();

                challengePolygon.Edges = edgeList;
                challengePolygon.Completed = true;
                
                // Pass current vertexlist to the placeLightsController and activate it.
                placeLightsController.SetValues(challengePolygon);
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
            newEdge = new Edge(p.Vertices[p.Vertices.Count -2], p.Vertices[p.Vertices.Count - 1]);
            edgeList.Add(newEdge);
            // create references in vertices to edge
            p.Vertices[p.Vertices.Count - 2].nextEdge = newEdge;
            p.Vertices[p.Vertices.Count - 1].prevEdge = newEdge;
            return;
        }
        else
        {
            GetComponent<AudioSource>().PlayOneShot(audioClipError, 0.3f);
           // Debug.Log("Not Possible!");
        }
    }

    // to call after each "turn"
    private void OnEnable()
    {
        polygonStarted = false;
        challengePolygon = Instantiate(challengePolygonPrefab, transform).GetComponent<Polygon>();
        challengePolygon.name = "Challenge Polygon";
        edgeList.Clear();
        polygonLine = challengePolygon.GetComponentInParent<LineRenderer>();
        polygonLine.positionCount = 0;
        polygonLine.material.color = LineColor;
        polygonLine.widthMultiplier = LineWidth;
    }

    void Start()
    {
        //
    }

    // Update is called once per frame
    void Update()
    {
        PolygonVertex snapToVertex = challengePolygon.Vertices.Count > 0 ? isCloseToVertex(challengePolygon.Vertices[0], 200f) : null;
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
            vertex.Color = Color.green;
            return vertex;
        }

        vertex.ResetShape();

        return null;
    }

    // checks if the new added edge is legal and does not intersect previous edges.
    public Edge IntersectsPolygon(Vector3 currPos)
    {
        if (edgeList.Count == 0)
            return null;

        Vector2 currPos2 = currPos;
        Vector2 prevPos = edgeList[edgeList.Count - 1].end;

        Edge tempEdge = new Edge(prevPos, currPos2);
        tempEdge.DebugDraw();
        foreach(Edge edge in edgeList)
        {
            edge.DebugDraw();
            if (edge.Crosses(tempEdge).HasValue)
            {
                return edge;
            } 
        }

        return null;
    }


    public bool CheckNotIntersectOld(Vector3 currPos)
    {
        Polygon p = challengePolygon;
        if (edgeList.Count == 0)
            return true;


        float ax = currPos.x;
        float ay = currPos.y;
        float bx = p.Vertices[p.Vertices.Count - 1].GetComponent<PolygonVertex>().x;
        float by = p.Vertices[p.Vertices.Count - 1].GetComponent<PolygonVertex>().y;
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
        //Debug.Log((bool1, bool2));
        return (bool1 & bool2);
    }
}
