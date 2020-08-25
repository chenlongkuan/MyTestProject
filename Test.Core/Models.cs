namespace MyTest.Core
{

    public class GpsPoint
    {
        public GpsPoint()
        {
            
        }

        public GpsPoint(double x, double y)
        {
            Lng = (decimal) x;
            Lat = (decimal) y;
        }

        public decimal Lng { get; set; }

        public decimal Lat { get; set; }
    }
}
