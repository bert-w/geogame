using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonVertex : MonoBehaviour
{
    public enum _Type { Start, End, Split, Merge, Regular }

    public Edge prevEdge;
    public Edge nextEdge;
    public float x;
    public float y;

    public Vector3 polarCoordinates;

    public _Type Type { get; set; }

    private void Awake()
    {
        x = transform.position.x;
        y = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float Distance(Vector3 to)
    {
        return Vector3.Distance(transform.position, to);
    }

    public void SetScale(float? size)
    {
        transform.localScale = new Vector3(size.GetValueOrDefault(30), size.GetValueOrDefault(30), 1);
    }

    public void SetColor(Color? color)
    {
        GetComponent<SpriteRenderer>().color = color.GetValueOrDefault(Color.black);
    }
}
