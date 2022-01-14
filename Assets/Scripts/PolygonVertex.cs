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
    
    private readonly IDictionary<string, object> defaults = new Dictionary<string, object>(){
        {"Color", UnityEngine.Color.black},
        {"Scale", 30f},
    };

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
        Color = (Color) defaults["Color"];
        Scale = (float) defaults["Scale"];

        x = transform.position.x;
        y = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowColors(bool value)
    {
        Color = (Color) (value ? typeColors[Type] : defaults["Color"]);
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
            var _value = value.GetValueOrDefault((float) defaults["Scale"]);
            transform.localScale = new Vector3(_value, _value, 1);
            _scale = _value;
        }
    }

    public Color? Color {
        get {
            return _color;
        }
        set {
            var _value = value.GetValueOrDefault((Color) defaults["Color"]);
            GetComponent<SpriteRenderer>().color = _value;
            _color = _value;
        }
    }

    // @TODO check if this is correct
    public bool IsEqual(PolygonVertex other)
    {
        return ((x == other.x) && (y == other.y));
    }
}
