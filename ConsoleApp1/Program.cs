using MyTest.Core;
using System;
using System.Collections.Generic;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {

            do
            {
                Console.WriteLine("请输入比较坐标点：");
                var pt = Console.ReadLine();
                var lng = double.Parse(pt.Split(',')[0]);
                var lat = double.Parse(pt.Split(',')[1]);
                Console.WriteLine("请输入允许误差值：");
                var tolowrance = double.Parse(Console.ReadLine());
                var marker = new GpsPoint(lng, lat);
                var lineArr = new List<GpsPoint>()
            {
                new GpsPoint(106.408477,29.793385),new GpsPoint(106.407348,29.792843),new GpsPoint(106.403702,29.791298),new GpsPoint(106.400959,29.790143),new GpsPoint(106.399839,29.78974),new GpsPoint(106.398958,29.789492),new GpsPoint(106.39803,29.78931),new GpsPoint(106.397569,29.789253),new GpsPoint(106.397118,29.789219),new GpsPoint(106.396597,29.789197),new GpsPoint(106.39599,29.789158),new GpsPoint(106.39556,29.789149),new GpsPoint(106.395087,29.789119),new GpsPoint(106.3924,29.789219),new GpsPoint(106.391602,29.789253),new GpsPoint(106.390773,29.789297),new GpsPoint(106.388685,29.789362),new GpsPoint(106.387569,29.789353),new GpsPoint(106.38691,29.789323),new GpsPoint(106.386497,29.789293),new GpsPoint(106.385807,29.789188),new GpsPoint(106.384909,29.789002),new GpsPoint(106.384549,29.788906),new GpsPoint(106.383841,29.788676),new GpsPoint(106.383268,29.788446),new GpsPoint(106.382865,29.788277),new GpsPoint(106.382079,29.78786),new GpsPoint(106.381176,29.787313),new GpsPoint(106.379523,29.786159),new GpsPoint(106.379015,29.785799),new GpsPoint(106.378607,29.785516),new GpsPoint(106.376363,29.783932),new GpsPoint(106.376016,29.783685),new GpsPoint(106.375256,29.783168),new GpsPoint(106.371836,29.780781),new GpsPoint(106.369627,29.779214),new GpsPoint(106.368238,29.778216),new GpsPoint(106.367122,29.777326),new GpsPoint(106.366654,29.776927),new GpsPoint(106.365608,29.775946),new GpsPoint(106.364961,29.77526),new GpsPoint(106.364514,29.774761),new GpsPoint(106.364154,29.774332),new GpsPoint(106.363542,29.773533),new GpsPoint(106.363125,29.772947),new GpsPoint(106.363003,29.772769),new GpsPoint(106.362396,29.771862),new GpsPoint(106.359631,29.767552),new GpsPoint(106.358915,29.766476),new GpsPoint(106.357956,29.765104),new GpsPoint(106.357166,29.764058),new GpsPoint(106.356367,29.763038),new GpsPoint(106.355716,29.762244),new GpsPoint(106.355469,29.761962),new GpsPoint(106.354822,29.761194),new GpsPoint(106.352318,29.758368),new GpsPoint(106.350625,29.75645),new GpsPoint(106.349918,29.755603),new GpsPoint(106.349232,29.75474),new GpsPoint(106.348516,29.753811),new GpsPoint(106.347977,29.753038),new GpsPoint(106.347656,29.752582),new GpsPoint(106.347014,29.751641),new GpsPoint(106.346589,29.750977),new GpsPoint(106.34615,29.750278),new GpsPoint(106.345543,29.749249),new GpsPoint(106.345326,29.748872),new GpsPoint(106.344549,29.747461),new GpsPoint(106.34444,29.747279),new GpsPoint(106.34418,29.746875),new GpsPoint(106.343377,29.745347),new GpsPoint(106.343008,29.744674),new GpsPoint(106.342053,29.742999),new GpsPoint(106.34099,29.74128),new GpsPoint(106.340122,29.739987),new GpsPoint(106.339987,29.739792),new GpsPoint(106.339384,29.738932),new GpsPoint(106.338398,29.737617),new GpsPoint(106.336111,29.734735),new GpsPoint(106.335477,29.733937),new GpsPoint(106.333411,29.731324),new GpsPoint(106.333095,29.730907),new GpsPoint(106.332049,29.729475),new GpsPoint(106.331736,29.729002),new GpsPoint(106.331571,29.728746),new GpsPoint(106.331141,29.728038),new GpsPoint(106.330833,29.727483),new GpsPoint(106.330495,29.726819),new GpsPoint(106.330165,29.726128),new GpsPoint(106.329913,29.725538),new GpsPoint(106.329674,29.724931),new GpsPoint(106.329223,29.723707),new GpsPoint(106.328199,29.72053),new GpsPoint(106.327574,29.718685),new GpsPoint(106.327361,29.718016),new GpsPoint(106.327179,29.717452),new GpsPoint(106.326576,29.715495),new GpsPoint(106.326107,29.714041),new GpsPoint(106.325742,29.712773),new GpsPoint(106.325681,29.712556),new GpsPoint(106.325404,29.711523),new GpsPoint(106.325104,29.710247),new GpsPoint(106.32487,29.709106),new GpsPoint(106.324479,29.707031),new GpsPoint(106.324457,29.706923),new GpsPoint(106.324392,29.70651),new GpsPoint(106.324301,29.705855),new GpsPoint(106.32421,29.705069),new GpsPoint(106.324154,29.704453),new GpsPoint(106.324106,29.703759),new GpsPoint(106.324062,29.703125),new GpsPoint(106.323997,29.702196),new GpsPoint(106.323976,29.701897)
            };

                //var isPointOnLine = Algorithms.IsPointOnLine(marker, lineArr, tolowrance);
                var distance = Algorithms.IsPointOnLine(marker, lineArr, tolowrance);
                var isPointOnLine = distance <= tolowrance;
                var printOutStr = $"{(isPointOnLine ? "在线上" : "不在线上")}，最短距离{distance:F}米";
                Console.WriteLine(printOutStr);

            } while (true);
        }
    }
}
