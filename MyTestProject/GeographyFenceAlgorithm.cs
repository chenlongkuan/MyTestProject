using MyTest.Core;
using System;

namespace MyTestProject
{
    public class GeographyFenceAlgorithm
    {

        public static string testAlgorithm()
        {
            var fencePoints = new GpsPoint[]
            {
                new GpsPoint() {Lng = 106.539178M, Lat = 29.596276M},
                new GpsPoint() {Lng = 106.518836M, Lat = 29.593216M},
                new GpsPoint() {Lng = 106.529951M, Lat = 29.588962M},
                new GpsPoint() {Lng = 106.532269M, Lat = 29.59344M},
                new GpsPoint() {Lng = 106.536346M, Lat = 29.593515M}
            };

            while (true)
            {
                var point = Console.ReadLine();
                var gpsPoint = point.Split(',');
                Console.WriteLine(GpsFenceContains(fencePoints,
                    new GpsPoint() { Lng = decimal.Parse(gpsPoint[0]), Lat = decimal.Parse(gpsPoint[1]) }));
            }
        }

        private static string GpsFenceContains(GpsPoint[] poly, GpsPoint p)
        {

            var px = p.Lng;
            var py = p.Lat;
            var flag = false;

            var j = 0;
            for (var i = 0; i < poly.Length; j = i, i++)
            {
                j = i - 1;
                if (i == 0)
                {
                    j = poly.Length - 1;
                }
                var sx = poly[i].Lng;
                var sy = poly[i].Lat;
                var tx = poly[j].Lng;
                var ty = poly[j].Lat;

                // 点与多边形顶点重合  
                if ((sx == px && sy == py) || (tx == px && ty == py))
                {
                    return "on";
                }

                // 判断线段两端点是否在射线两侧  
                if ((sy < py && ty >= py) || (sy >= py && ty < py))
                {
                    // 线段上与射线 Y 坐标相同的点的 X 坐标  
                    var x = sx + (py - sy) * (tx - sx) / (ty - sy);

                    // 点在多边形的边上  
                    if (x == px)
                    {
                        return "on";
                    }

                    // 射线穿过多边形的边界  
                    if (x > px)
                    {
                        flag = !flag;
                    }
                }
            }

            // 射线穿过多边形边界的次数为奇数时点在多边形内  
            return flag ? "in" : "out";
        }
    }

}