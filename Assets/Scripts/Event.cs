using System;
using UnityEngine;

public class Event : IComparable
{
    private readonly Edge rayCast1;
    private readonly Edge rayCast2;

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
        rayCast2 = new Edge(light, edge.end);
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
        if (crossing != null)
        {
            return true;
        }

        crossing = rayCast2.Crosses(edge);
        if (crossing != null)
        {
            return true;
        }

        return false;
    }

    public int CompareTo(object obj)
    {
        // Misschien toch ray gebruiken met crossing
        if (obj is Event otherEvent)
        {
            if (RayCastsIntersectWithEdge(otherEvent.Edge))
            {
                return 1;
            }

            if (otherEvent.RayCastsIntersectWithEdge(Edge))
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
}

public enum EventType
{
    Start,
    End
}
