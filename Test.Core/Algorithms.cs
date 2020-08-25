using System;
using System.Collections.Generic;
using System.Linq;
using MyTest.Core;

namespace ConsoleApp1
{
    public class Algorithms
    {
        
        /// <summary>
        /// 检查Gps坐标是否在多边形范围内
        /// </summary>
        /// <param name="poly">多边形点集合</param>
        /// <param name="p">检测点</param>
        /// <returns></returns>
        public static bool IsGpsFenceContains(GpsPoint[] poly, GpsPoint p)
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
                    return true;
                }

                // 判断线段两端点是否在射线两侧  
                if ((sy < py && ty >= py) || (sy >= py && ty < py))
                {
                    // 线段上与射线 Y 坐标相同的点的 X 坐标  
                    var x = sx + (py - sy) * (tx - sx) / (ty - sy);

                    // 点在多边形的边上  
                    if (x == px)
                    {
                        return true;
                    }

                    // 射线穿过多边形的边界  
                    if (x > px)
                    {
                        flag = !flag;
                    }
                }
            }

            // 射线穿过多边形边界的次数为奇数时点在多边形内  
            return flag;
        }


        /// <summary>
        /// 时间交叉校验
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsDateOverlap(DateTime a, DateTime b, DateTime x, DateTime y)
        {
            //讨论取a[B,E]与b1[B,E]不重叠部分：
            //2.a)当 (a.E<=b1.B)||(b1.E<=a.B) 时，无重叠，保留原先的a
            //时间无重叠                 
            if ((b < x) || (y < a))
            {
                return false;
            }
            //2.b)否则有重叠，去掉原先的a；
            //当 (a.B<b1.B)&&(b1.B<=a.E) 时，留下左边非重复段 a1[a.B,b1.B]
            //当 (a.B<=b1.E)&&(b1.E<a.E) 时，留下右边非重复段 a2[b1.E,a.E]
            //重叠
            if ((a < x) && (x <= b))
            {
                return true;
            }
            if ((a <= y) && (y < b))
            {
                return true;
            }
            if ((a < y) && (b == x))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断点是否在直线上
        /// </summary>
        /// <param name="pf">比较点</param>
        /// <param name="lineArr">线段点集合</param>
        /// <param name="tolerance">误差（米）</param>
        /// <returns></returns>
        public static double IsPointOnLine (GpsPoint pf, List<GpsPoint> lineArr, double tolerance) {

            var distances =new List<double>(); // 点到折线每相邻两点的最短距离

            for (var i = 0; i < lineArr.Count; i++)
            {
                if (i+1>=lineArr.Count)break;
                var distance = GetPoToLineDis(pf, lineArr[i], lineArr[i + 1]);
                distances.Add(distance);
                //if (distance <= tolerance)
                //{
                //    break;
                //}
            }
            var minDistance = distances.Min();
            //if (minDistance <= tolerance) {
            //    // 当最短距离小于误差值时，判断鼠标点在折线上
            //    return true;
            //}
            //return false;

            return minDistance;
        }

        /// <summary>
        /// 点到折线上相邻两点组成的线段的最短距离
        /// </summary>
        /// <param name="point">点坐标</param>
        /// <param name="curPt">折线点坐标</param>
        /// <param name="nextPt">与 curPt 相邻的折线点坐标</param>
        /// <returns></returns>
        private static double GetPoToLineDis(GpsPoint point,GpsPoint curPt,GpsPoint nextPt) {
            //https://blog.csdn.net/ufoxiong21/article/details/46487001
            var a = GetPoToPoDis(curPt.Lng, curPt.Lat, nextPt.Lng, nextPt.Lat); // P1 到 P2 的长度，记作 a 线段
            var b = GetPoToPoDis(curPt.Lng, curPt.Lat, point.Lng, point.Lat); // P1 到 P 的长度，记作 b 线段
            var c = GetPoToPoDis(nextPt.Lng, nextPt.Lat, point.Lng, point.Lat); // P2 到 P 的长度，记作 c 线段

            double distance = 0;
            if (b+c==a) {
                // 当 b + c = a 时，P 在 P1 和 P2 组成的线段上
                distance = 0;
            } else if (c * c >=a * a + b * b) {
                // 当 c * c >= a * a + b * b 时组成直角三角形或钝角三角形，投影在 P1 延长线上
                distance = b;
            } else if (b * b >=c * c + a * a) {
                // 当 b * b > c * c + a * a 时组成直角三角形或钝角三角形，投影在 p2 延长线上
                distance = c;
            } else {
                // 其他情况组成锐角三角形，则求三角形的高
                var p = (a + b + c) / 2; // 半周长
                var s = Math.Sqrt(p * (p - a) * (p - b) * (p - c)); // 海伦公式求面积
                distance = 2 * s / a; // 点到线的距离（利用三角形面积公式求高)
            }

            return distance;
        }

        /// <summary>
        /// 两点之间平面的距离
        /// </summary>
        /// <param name="x1">第一个点的经度</param>
        /// <param name="y1">第一个点的纬度</param>
        /// <param name="x2">第二个点的经度</param>
        /// <param name="y2">第二个点的纬度</param>
        /// <returns></returns>
        private static double GetPoToPoDis(decimal x1, decimal y1,decimal x2,decimal y2) {
            //运用勾股定理来计算距离。
            //(x1，y1)到（x2，y2）距离计算步骤，
            //x2-x1=纵向长度=勾边，
            //y2-y1=横向长度=股边，
            //勾平方+股平方=弦平方，
            //弦平方开根=弦边=长度。
            return Math.Sqrt((double) ((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2))) * 100000;
        }


        //地球半径，单位米
        private const double EARTH_RADIUS = 6378137;
        /// <summary>
        /// 计算两点位置的球面距离，返回两点的距离，单位 米
        /// 该公式为GOOGLE提供，误差小于0.2米
        /// </summary>
        /// <param name="lat1">第一点纬度</param>
        /// <param name="lng1">第一点经度</param>
        /// <param name="lat2">第二点纬度</param>
        /// <param name="lng2">第二点经度</param>
        /// <returns></returns>
        public static double GetDistance(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
        {
            var radLat1 = Rad(lat1);
            var radLng1 = Rad(lng1);
            var radLat2 = Rad(lat2);
            var radLng2 = Rad(lng2);
            var a = radLat1 - radLat2;
            var b = radLng1 - radLng2;
            var result = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2))) * EARTH_RADIUS;
            return result;
        }
 
        /// <summary>
        /// 经纬度转化成弧度
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private static double Rad(decimal d)
        {
            return (double)d *  Math.PI /  180.0;
        }
    }
}