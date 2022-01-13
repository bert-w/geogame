using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class EdgeHelpers
    {
        public static Vector2? FindOverlappingVertex(this Edge edge, Edge compareEdge)
        {
            if (edge.start == compareEdge.end)
            {
                return edge.start;
            }

            if (edge.end == compareEdge.start)
            {
                return edge.end;
            }

            if (edge.start == compareEdge.start)
            {
                return edge.start;
            }

            if (edge.end == compareEdge.end)
            {
                return edge.end;
            }

            return null;
        }
    }
}
