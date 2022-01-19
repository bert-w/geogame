using Assets.Scripts.Utils;
using System;
using UnityEngine;

public class Event : IComparable
{
    private readonly Edge rayCast1;
    private readonly Edge rayCast2;

    private readonly Edge largeRayCast1;
    private readonly Edge largeRayCast2;

    public int Id { get; set; }

    /// <summary>
    /// The distance from the light to the start vertex
    /// </summary>
    public float StartDistance { get; private set; }

    /// <summary>
    /// The distance from the light to the end vertex
    /// </summary>
    public float EndDistance { get; private set; }

    /// <summary>
    /// The degrees around the light
    /// </summary>
    public float Degrees { get; set; }

    /// <summary>
    /// The type of event
    /// </summary>
    public EventType Type { get; set; }

    /// <summary>
    /// The edge corresponding to the events
    /// </summary>
    public Edge Edge { get; private set; }

    public Event(Vector2 light, float startDistance, float endDistance, float degrees, Edge edge, EventType type)
    {
        EndDistance = endDistance;
        StartDistance = startDistance;
        Degrees = degrees;
        Edge = edge;
        rayCast1 = new Edge(light, edge.start);
        largeRayCast1 = BuildRayCast(light, edge.start);
        rayCast2 = new Edge(light, edge.end);
        largeRayCast2 = BuildRayCast(light, edge.end);
        Type = type;
    }

    public void SwapDistances()
    {
        var oldStartDistance = StartDistance;
        StartDistance = EndDistance;
        EndDistance = oldStartDistance;
    }

    public bool RayCastsIntersectWithEdge(Edge edge)
    {
        var crossing = rayCast1.Crosses(edge);
        if (crossing != null && !(crossing == Edge.start || crossing == Edge.end))
        {
            return true;
        }

        crossing = rayCast2.Crosses(edge);
        if (crossing != null && !(crossing == Edge.start || crossing == Edge.end))
        {
            return true;
        }

        return false;
    }

    public bool LargeRayCastsDoesNotIntersectWithEdge(Edge edge)
    {
        var crossing = largeRayCast1.Crosses(edge);
        if (crossing != null && !(crossing == Edge.start || crossing == Edge.end))
        {
            return false;
        }

        crossing = largeRayCast2.Crosses(edge);
        if (crossing != null && !(crossing == Edge.start || crossing == Edge.end))
        {
            return true;
        }

        return true;
    }

    public int CompareTo(object obj)
    {
        // Misschien toch ray gebruiken met crossing
        if (obj is Event otherEvent)
        {
            if (RayCastsIntersectWithEdge(otherEvent.Edge) && LargeRayCastsDoesNotIntersectWithEdge(otherEvent.Edge))
            {
                return 1;
            }

            if (otherEvent.RayCastsIntersectWithEdge(Edge) && otherEvent.LargeRayCastsDoesNotIntersectWithEdge(Edge))
            {
                return -1;
            }

            Debug.Log("No intersection found");
            if (StartDistance == otherEvent.StartDistance)
            {
                return EndDistance.CompareTo(otherEvent.EndDistance);
            }

            return StartDistance.CompareTo(otherEvent.StartDistance);
        }
        throw new NotImplementedException();
    }

    public override bool Equals(object obj)
    {
        if (obj is Event otherEvent)
        {
            return Edge.Equals(otherEvent.Edge);
        }
        throw new NotImplementedException();
    }

    private Edge BuildRayCast(Vector2 lightPoint, Vector2 endPoint)
    {
        var direction = endPoint - lightPoint;
        var ray = new Ray(lightPoint, direction);

        // TODO don't use large point but vector directly
        var largePoint = ray.GetPoint(10000);
        return new Edge(endPoint, largePoint);
    }
}

public enum EventType
{
    Start,
    End
}
