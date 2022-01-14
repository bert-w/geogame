using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameEdge : MonoBehaviour
{
    public Edge edge;

    public bool show;

    public Color color = Color.black;

    private LineRenderer lineRenderer;

    // Create a GameEdge as a child of the given gameObject.
    public static GameEdge Create(GameObject gameObject, Vector2 start, Vector2 end)
    {
        GameObject go = new GameObject("Edge " + start + " -> " + end, typeof(GameEdge));
        GameEdge ge = go.GetComponent<GameEdge>();
        go.transform.parent = gameObject.transform;
        ge.edge = new Edge(start, end);
        return ge;
    }

    void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.transform.parent = transform;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 5f;
        lineRenderer.endWidth = 5f;
    }

    void Update()
    {
        if(show) {
            lineRenderer.enabled = true;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.SetPositions(new[] { (Vector3) edge.start, (Vector3) edge.end });
        } else {
            lineRenderer.enabled = false;
        }
    }
}

