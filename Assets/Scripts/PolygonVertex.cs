using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonVertex : MonoBehaviour
{
    public enum _Type { Start, End, Split, Merge, Regular }

    private readonly IDictionary<_Type, Color> typeColors = new Dictionary<_Type, Color>(){
        {_Type.Start, UnityEngine.Color.cyan},
        {_Type.End, UnityEngine.Color.blue},
        {_Type.Split, UnityEngine.Color.red},
        {_Type.Merge, UnityEngine.Color.yellow},
        {_Type.Regular, UnityEngine.Color.magenta}
    };
    
    private IDictionary<string, object> defaults = new Dictionary<string, object>(){};

    public Edge prevEdge;
    public Edge nextEdge;
    public float x;
    public float y;

    public Vector3 polarCoordinates;

    public bool showTypeColor;

    [field: SerializeField]
    public _Type Type { get; set; }

    [field: SerializeField]
    public Edge LeftHelperEdge { get; set; }

    public Color Color = new Color(0f, 0f, 0f);

    public float Scale = 30f;

    void Awake()
    {
        x = transform.position.x;
        y = transform.position.y;
    }

    void Start()
    {
        // Set defaults.
        defaults["Color"] = Color = GetComponent<SpriteRenderer>().color;
        defaults["Scale"] = Scale = transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<SpriteRenderer>().color = showTypeColor ? typeColors[Type] : Color;
        transform.localScale = new Vector3(Scale, Scale, 1);
    }

    public void ResetShape()
    {
        Scale = (float) defaults["Scale"];
        Color = (Color) defaults["Color"];
    }

    public Vector2 ToVector()
    {
        return new Vector2(x, y);
    }

    public float Distance(Vector3 to)
    {
        return Vector3.Distance(transform.position, to);
    }

    // @TODO check if this is correct
    public bool IsEqual(PolygonVertex other)
    {
        return ((x == other.x) && (y == other.y));
    }
}
