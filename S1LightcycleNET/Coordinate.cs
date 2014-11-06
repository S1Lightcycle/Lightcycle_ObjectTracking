using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S1LightcycleNET
{
    public class Coordinate
    {
        public int XCoord { get; set; }
        public int YCoord { get; set; }

        public Coordinate(int x, int y)
        {
            XCoord = x;
            YCoord = y;
        }
    }
}
