using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Event : IComparable
{
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

    public Event(float startDistance, float endDistance, float degrees, Edge edge, EventType type)
    {
        EndDistance = endDistance;
        StartDistance = startDistance;
        Degrees = degrees;
        Edge = edge;
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
