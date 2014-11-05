using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Utilities;
using OpenCvSharp;
using OpenCvSharp.Blob;

namespace S1LightcycleNET
{
    class ObjectTracker
    {
        public Coordinate FirstCarCoordinate { get; set; }
        public Coordinate SecondCarCoordinate { get; set; }

        private const int CAPTURE_WIDTH_PROPERTY = 3;
        private const int CAPTURE_HEIGHT_PROPERTY = 4;

        private const int BLOB_MIN_SIZE = 2500;
        private const int BLOB_MAX_SIZE = 50000;

        private readonly VideoCapture capture;
        private readonly CvWindow blobWindow;
        private readonly CvWindow subWindow;

        private readonly BackgroundSubtractor subtractor;

        private Mat frame;

        private CvBlobs blobs;

        private CvPoint firstCar;

        public ObjectTracker()
        {
            //webcam
            capture = new VideoCapture(0);
            blobWindow = new CvWindow("blobs");
            subWindow = new CvWindow("subtracted");

            //setting capture resolution
#if DEBUG
            capture.Set(CAPTURE_WIDTH_PROPERTY, 320);
            capture.Set(CAPTURE_HEIGHT_PROPERTY, 240);
#else
            capture.Set(CAPTURE_WIDTH_PROPERTY, SystemInformation.VirtualScreen.Width);
            capture.Set(CAPTURE_HEIGHT_PROPERTY, SystemInformation.VirtualScreen.Height);
#endif

            //Background subtractor, alternatives: MOG, GMG
            subtractor = new BackgroundSubtractorMOG2();

            firstCar = CvPoint.Empty;
        }

        public void track()
        {

            frame = new Mat();

            //get new frame from camera
            capture.Read(frame);

            //frame height == 0 => camera hasn't been initialized properly and provides garbage data
            while (frame.Height == 0)
            {
                capture.Read(frame);
            }

            //determines how fast stationary objects are incorporated into the background mask ( higher = faster)
            double learningRate = 0.001;

            Mat sub = new Mat();

            //perform background subtraction with selected subtractor.
            subtractor.Run(frame, sub, learningRate);

            IplImage src = (IplImage)sub;

            //binarize image
            Cv.Threshold(src, src, 250, 255, ThresholdType.Binary);


            IplConvKernel element = Cv.CreateStructuringElementEx(4, 4, 0, 0, ElementShape.Rect, null);
            Cv.Erode(src, src, element, 1);
            Cv.Dilate(src, src, element, 1);
            blobs = new CvBlobs();
            blobs.Label(src);

            IplImage render = new IplImage(src.Size, BitDepth.U8, 3);

            CvBlob largest = getLargestBlob(BLOB_MIN_SIZE, BLOB_MAX_SIZE);
            CvBlob secondLargest = null;

            if (largest != null)
            {
                secondLargest = getLargestBlob(largest.Area - 1500, largest.Area);
            }

            blobs.RenderBlobs(src, render);

            blobWindow.ShowImage(render);
            subWindow.ShowImage(src);

            

            Cv2.WaitKey(1);

            //compare distance between last largest blob and current largest and second largest blob
            //if distance between the last largest and current largest is shorter than between the last largest and second largest 
            //return the current largest as first element
            //else return the second largest as second element
            linearPrediction(largest, secondLargest);
        }

        private void linearPrediction(CvBlob largest, CvBlob secondLargest)
        {
            if (largest != null)
            {
                CvPoint largestCenter = largest.CalcCentroid();
                CvPoint secondCenter = secondLargest.CalcCentroid();

                if ((firstCar == CvPoint.Empty) || (firstCar.DistanceTo(largestCenter) < firstCar.DistanceTo(secondCenter)))
                {
                    firstCar = largestCenter;
                    FirstCarCoordinate = cvPointToCoordinate(largestCenter);
                    SecondCarCoordinate = cvPointToCoordinate(secondCenter);
                }
                else
                {
                    firstCar = secondCenter;
                    FirstCarCoordinate = cvPointToCoordinate(secondCenter);
                    SecondCarCoordinate = cvPointToCoordinate(largestCenter);
                }
            }
            else
            {
                FirstCarCoordinate = calculateCenter(largest);
                SecondCarCoordinate = calculateCenter(secondLargest);
            }
        }

        private Coordinate calculateCenter(CvBlob blob)
        {
            if (blob == null)
            {
                return new Coordinate(-1, -1);
            }
            CvPoint center = blob.CalcCentroid();
            return new Coordinate(center.X, center.Y);
        }

        private CvBlob getLargestBlob(int minBlobSize, int maxBlobSize)
        {
            blobs.FilterByArea(minBlobSize, maxBlobSize);
            return blobs.LargestBlob();
        }

        private Coordinate cvPointToCoordinate(CvPoint point)
        {
            return new Coordinate(point.X, point.Y);
        }
    }
}
