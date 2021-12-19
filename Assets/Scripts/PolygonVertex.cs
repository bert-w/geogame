using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonVertex : MonoBehaviour
{

    public Edge prevEdge;
    public Edge nextEdge;
    public float x;
    public float y;

    private void Awake()
    {
        x = transform.position.x;
        y = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
