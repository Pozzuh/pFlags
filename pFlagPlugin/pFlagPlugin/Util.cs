using System;
using System.Collections.Generic;
using System.Text;
using Addon;


namespace pFlagPlugin
{
    class Util
    {
        public static double Distance(Vector vec1, Vector vec2)
        {
            return Math.Sqrt(Math.Pow(vec1.X - vec2.X, 2) + Math.Pow(vec1.Y - vec2.Y, 2) + Math.Pow(vec1.Z - vec2.Z, 2));
        }

        public static Vector clientToVector(ServerClient c)
        {
            return new Vector(c.OriginX, c.OriginY, c.OriginZ);
        }

        public static Vector coordsToVector(float x, float y, float z)
        {
            return new Vector(x, y, z);
        }

        public static void moveTo(ServerClient c, Vector v)
        {
            c.OriginX = v.X;
            c.OriginY = v.Y;
            c.OriginZ = v.Z;
        }
    }
}
