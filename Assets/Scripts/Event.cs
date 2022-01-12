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

    public int CompareTo(object obj)
    {
        // Misschien toch ray gebruiken met crossing
        if (obj is Event otherEvent)
        {
            var crossing = rayCast1.Crosses(otherEvent.Edge);
            if (crossing != null)
            {
                return 1;
            }

            crossing = rayCast2.Crosses(otherEvent.Edge);
            if (crossing != null)
            {
                return 1;
            }

            return -1;
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
