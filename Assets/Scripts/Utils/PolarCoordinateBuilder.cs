using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class PolarCoordinateBuilder
    {
        public static Vector2 Build(Vector2 vertex, Vector2 point)
        {
            // Set coordinates relative to the current mouse position.
            var vPos = point - vertex;

            // https://www.mathsisfun.com/polar-cartesian-coordinates.html
            float hypothenuse = Mathf.Sqrt(Mathf.Pow(vPos.x, 2) + Mathf.Pow(vPos.y, 2));

            float tangent = Mathf.Atan(vPos.y / vPos.x);

            // Quadrant correction
            if (vPos.x < 0)
            {
                // Quadrant 2 or 3.
                tangent += Mathf.PI;
            }
            else if (vPos.x > 0 && vPos.y < 0)
            {
                // Quadrant 4.
                tangent += 2 * Mathf.PI;
            }

            // Assign polar coordinates to the vertex object.
            return new Vector3(hypothenuse, tangent);
        }
    }
}
