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
            tracker.StartTracking();
            for (; ; )
            {
                Console.WriteLine("X: " + tracker.FirstCarCoordinate.XCoord + "\t" + tracker.FirstCarCoordinate.YCoord);
            }
            Console.ReadKey();
        }

    }
}
