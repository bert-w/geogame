using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RadialSweep
{
    /// <summary>
    /// returns the portion of the edge that may have been cut off by  a  different edge
    /// the final implementation should return 2 points + edge so we can calculate the
    /// triangle.
    /// </summary>
    /// <returns></returns>
    public static Triangle VisibleEdges()
    {
        return null;
    }
}

public class VisibilityCalculator
{
    public List<Light> lightList;
    public List<Edge> polygonEdgeList;

    // gives area covered by light by calculating unions
    // of all triangles from each lighting point
    public float LightCoverage()
    {
        return 10.0f;
    }



}
