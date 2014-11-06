using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Drawing;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Utilities;
using OpenCvSharp;
using OpenCvSharp.Blob;
using System.Collections;

namespace S1LightcycleNET
{
    class Program
    {
        static void Main(string[] args)
        {
            ObjectTracker tracker = new ObjectTracker();
            Dictionary<Coordinate, bool> asdf = new Dictionary<Coordinate, bool>();
            asdf.Add(new Coordinate(1, 2), true);
            if (asdf.ContainsKey(new Coordinate(1, 2)))
            {
                Console.WriteLine("jippii");
            }
            Console.ReadKey();
            for (;;)
            {
                //var Coordinates = tracker.track();
                //Console.WriteLine("LBlob: " + Coordinates.Item1.XCoord + "\t" + Coordinates.Item1.YCoord + "\tSBlob: " + Coordinates.Item2.XCoord + "\t" + Coordinates.Item2.YCoord);
                //Console.WriteLine();
            }
        }

    }
}
