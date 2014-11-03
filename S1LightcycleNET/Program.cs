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

namespace S1LightcycleNET
{
    class Program
    {
        static void Main(string[] args)
        {
            ObjectTracker tracker = new ObjectTracker();
            for (;;)
            {
                var Coordinates = tracker.track();
                Console.WriteLine("Largest Blob: " + Coordinates.Item1.XCoord + "\t" + Coordinates.Item1.YCoord);
                Console.WriteLine("Second Largest Blob: " + Coordinates.Item2.XCoord + "\t" + Coordinates.Item2.YCoord);
            }
        }

    }
}
