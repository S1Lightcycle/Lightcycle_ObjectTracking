using System.Collections.Generic;

namespace S1LightcycleNET
{
    public class Robot
    {
        public Queue<Coordinate> Coord { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }


        public Robot(int width, int height)
        {
            Coord = new Queue<Coordinate>();
            Width = width;
            Height = height;
        }
    }
}
