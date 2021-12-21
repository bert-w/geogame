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

    [field: SerializeField]
    public _Type Type { get; set; }

    [field: SerializeField]
    public Edge LeftHelperEdge { get; set; }

    private Color _color;

    private float _scale;

    private void Awake()
    {
        // Set defaults.
        Color = UnityEngine.Color.black;
        Scale = 30;

        x = transform.position.x;
        y = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector2 ToVector()
    {
        return new Vector2(x, y);
    }

    public float Distance(Vector3 to)
    {
        return Vector3.Distance(transform.position, to);
    }

    public float? Scale {
        get {
            return _scale;
        }
        set {
            transform.localScale = new Vector3(value.GetValueOrDefault(30), value.GetValueOrDefault(30), 1);
            _scale = value.GetValueOrDefault(30);
        }
    }

    public Color? Color {
        get {
            return _color;
        }
        set {
            GetComponent<SpriteRenderer>().color = value.GetValueOrDefault(UnityEngine.Color.black);
            _color = value.GetValueOrDefault(UnityEngine.Color.black);
        }
    }
}
