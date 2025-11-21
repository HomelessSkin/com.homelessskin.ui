
using UnityEngine;

namespace UI
{
    public static class Extensions
    {
        public static Color ToColor(this Vector4 vector) => new Color(vector.x, vector.y, vector.z, vector.w);
    }
}