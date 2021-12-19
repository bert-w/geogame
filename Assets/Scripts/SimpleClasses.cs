using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle
{

}

public class Node
{
    public Node leftChild;
    public Node rightChild;
    public Node parent;
    public Edge edge;
}

public class Edge
{

    public float startPointX;
    public float startPointY;
    public float endPointX;
    public float endPointY;

    public Edge(GameObject startVertex, GameObject endVertex)
    {
        startPointX = startVertex.GetComponent<PolygonVertex>().x;
        startPointY = startVertex.GetComponent<PolygonVertex>().y;
        endPointX = endVertex.GetComponent<PolygonVertex>().x;
        endPointY = endVertex.GetComponent<PolygonVertex>().y;
    }
}



public class Light
{
    public float x;
    public float y;
    public List<Triangle> triangleList;
}
