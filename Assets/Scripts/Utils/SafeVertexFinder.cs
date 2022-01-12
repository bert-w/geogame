using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    /// <remarks>
    /// It works is the following steps:
    /// 1. Find the start vertex of a random edge
    /// 2. Draw a ray cast to that vertex
    /// 3. Check if the ray cast intersects any other edges
    /// 4. If so return the start vertex of the nearest intersecting vertex
    /// 5. If not return the original start vertex
    /// </remarks>
    public static class SafeVertexFinder
    {
        public static Tuple<Edge, Vector2> Find(List<Edge> edges, Vector2 point)
        {
            var intersectionEdge = edges[0];
            Vector2 intersectionVertex;
            if (IsStartVertex(intersectionEdge, point))
            {
                intersectionVertex = intersectionEdge.start;
            }
            else
            {
                intersectionVertex = intersectionEdge.end;
            }
            var rayCast = new Edge(point, intersectionVertex);

            Edge nearestIntersectionEdge = intersectionEdge;
            Vector2 nearestIntersection = intersectionVertex;
            float minimumDistance = float.MaxValue;

            for (int i = 1; i < edges.Count; i++)
            {
                var edge = edges[i];
                var crossingPoint = edge.Crosses(rayCast);

                if (crossingPoint != null)
                {
                    var distance = (new Edge(point, crossingPoint.Value)).Length;
                    if (distance < minimumDistance)
                    {
                        minimumDistance = distance;
                        nearestIntersectionEdge = edge;
                        if (IsStartVertex(edge, point))
                        {
                            nearestIntersection = edge.start;
                        }
                        else
                        {
                            nearestIntersection = edge.end;
                        }
                    }
                }
            }

           return new Tuple<Edge, Vector2>(nearestIntersectionEdge, nearestIntersection);
        }

        public static bool IsStartVertex(Edge edge, Vector2 point)
        {
            var startDegrees = PolarCoordinateBuilder.Build(edge.start, point).y;
            var endDegrees = PolarCoordinateBuilder.Build(edge.end, point).y;

            var degreeDistance = endDegrees - startDegrees;
            if (degreeDistance < 0)
            {
                degreeDistance += Mathf.PI * 2;
            }

            return degreeDistance < Mathf.PI;
        }
    }
}
