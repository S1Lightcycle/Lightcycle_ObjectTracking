using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S1LightcycleNET
{
    public class Coordinate
    {

        public static readonly Coordinate INVALID = null;

        public int XCoord { get; set; }
        public int YCoord { get; set; }
        
        public override bool Equals(object obj) {
            if (this.GetHashCode() != ((Coordinate)obj).GetHashCode()) return false;
            return true;
        }

        public override int GetHashCode() {
            return (XCoord * 0x1f1f1f1f) ^ YCoord;
        }

        public Coordinate(int x, int y)
        {
            XCoord = x;
            YCoord = y;
        }
    }
}
