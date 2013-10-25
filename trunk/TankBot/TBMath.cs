using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankBot
{

    class TBMath
    {
        internal static double sqr(double x)
        {
            return x * x;
        }
        internal static double distance(Point point1, Point point2)
        {
            return Math.Sqrt(sqr(point1.x - point2.x) + sqr(point1.y - point2.y));
        }

        internal static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        internal static double cameraDegreeToEnemy(Vehicle et, Vehicle myTank)
        {
            double degree_diff = TBMath.RadianToDegree(Math.Atan2(et.pos.y - myTank.pos.y, et.pos.x - myTank.pos.x)) - myTank.cameraDirection + 90 + 360;
            while (degree_diff > 180)
                degree_diff -= 360;
            return degree_diff;
        }

        internal static double vehicleDegreeToPoint(Point np, Vehicle myTank)
        {
            double degree_diff = TBMath.RadianToDegree(Math.Atan2(np.y - myTank.pos.y, np.x - myTank.pos.x)) - myTank.direction + 90 + 720;
            while (degree_diff > 180)
                degree_diff -= 360;
            return degree_diff;
        }


        internal static Point BigWorldPos2MinimapPos(Point p, string map_name)
        {
            double scale = MapDef.map_size[map_name] * 10;
            double boundX = MapDef.boundingBox[map_name].Item1.x;
            double boundY = MapDef.boundingBox[map_name].Item2.y;
            return new Point((p.x - boundX) / scale * 10 + 1, (-p.y + boundY) / scale * 10 + 1);
        }
    }
}
