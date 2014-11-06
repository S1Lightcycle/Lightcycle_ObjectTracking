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
            for (;;)
            {
                Console.WriteLine("LBlob: " + tracker.FirstCar.Coord.XCoord + "\t" + tracker.FirstCar.Coord.YCoord + "\tSBlob: " + tracker.SecondCar.Coord.XCoord + "\t" + tracker.SecondCar.Coord.YCoord);
                Console.WriteLine();
            }
        }

    }
}
