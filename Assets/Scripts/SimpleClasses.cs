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

public class Light
{
    public float x;
    public float y;
    public List<Triangle> triangleList;
}
