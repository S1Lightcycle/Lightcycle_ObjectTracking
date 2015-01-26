namespace S1LightcycleNET
{
    public class Coordinate
    {

        public static readonly Coordinate Invalid = null;

        public int XCoord { get; set; }
        public int YCoord { get; set; }

        public Coordinate(int x, int y)
        {
            XCoord = x;
            YCoord = y;
        }

        public Coordinate(OpenCvSharp.CvPoint point)
        {
            XCoord = point.X;
            YCoord = point.Y;
        }
    }
}
