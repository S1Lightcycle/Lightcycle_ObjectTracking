using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Drawing;
using System.Windows.Forms.VisualStyles;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Utilities;
using OpenCvSharp;
using OpenCvSharp.Blob;

namespace S1LightcycleNET
{
    class ObjectTracker
    {
        public Tuple<int, int> FirstCarCoordinate { get; private set; }
        public Tuple<int, int> SecondCarCoordinate { get; private set; }

        private const int CAPTURE_WIDTH_PROPERTY = 3;
        private const int CAPTURE_HEIGHT_PROPERTY = 4;

        private readonly VideoCapture capture;
        private readonly Mat frame;

        private readonly CvWindow blobWindow;
        private readonly CvWindow subWindow;
        private readonly Window originalWindow;

        private readonly BackgroundSubtractor subtractor;

        private bool stop;

        public ObjectTracker()
        {
            capture = new VideoCapture(0);
            
            frame = new Mat();

            //output windows
            blobWindow = new CvWindow("blobs");
            subWindow = new CvWindow("subtracted");
            originalWindow = new Window("original");

            capture.Set(CAPTURE_WIDTH_PROPERTY, 360);
            capture.Set(CAPTURE_HEIGHT_PROPERTY, 240);

            subtractor = new BackgroundSubtractorMOG2();
        }

        public void StartTracking()
        {
            //delete me
            FirstCarCoordinate = new Tuple<int, int>(1, 2);
            stop = false;
            Task trackingTask = new Task(() => track());
            trackingTask.Start();
        }

        public void StopTracking()
        {
            stop = true;
        }

        private void track()
        {
            
            Console.WriteLine("Entering track");
            while (stop == false)
            {

                //get new frame from camera
                capture.Read(frame);

                //frame height == 0 => camera hasn't been initialized properly and provides garbage data
                while (frame.Height == 0)
                {
                    capture.Read(frame);
                }
                Mat sub = new Mat();


                //determines how fast stationary objects are incorporated into the background mask ( higher = faster)
                double learningRate = 0.001;

                //perform background subtraction with selected subtractor.
                subtractor.Run(frame, sub, learningRate);


                //show the unaltered camera output
                //originalWindow.ShowImage(frame);


                IplImage src = (IplImage)sub;

                //binarize image
                Cv.Threshold(src, src, 250, 255, ThresholdType.Binary);


                IplConvKernel element = Cv.CreateStructuringElementEx(4, 4, 0, 0, ElementShape.Rect, null);
                Cv.Erode(src, src, element, 1);
                Cv.Dilate(src, src, element, 1);
                CvBlobs blobs = new CvBlobs();
                blobs.Label(src);


                int blobMINsize = 2500;
                int blobMAXsize = 50000;
                blobs.FilterByArea(blobMINsize, blobMAXsize);

                IplImage render = new IplImage(src.Size, BitDepth.U8, 3);
                CvBlob largest = blobs.LargestBlob();
                if (largest != null)
                {
                    blobs.FilterByArea(largest.Area - 1500, largest.Area);
                    Console.Out.WriteLine(blobs.Count);
                    Console.Out.WriteLine(largest.Area);
                }

                blobs.RenderBlobs(src, render);
                try
                {
                    blobWindow.ShowImage(render);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                subWindow.ShowImage(src);
            }
        }

    }
}
