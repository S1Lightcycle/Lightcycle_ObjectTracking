using OpenCvSharp.CPlusPlus;
using OpenCvSharp;
using OpenCvSharp.Blob;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace S1LightcycleNET
{
    public class ObjectTracker
    {

        private readonly VideoCapture capture;
        private CvWindow blobWindow;
        private readonly BackgroundSubtractor subtractor;
        private Mat frame;
        private CvBlobs blobs;
        private CvPoint oldFirstCar;
        private CvPoint oldSecondCar;
        private bool IsTracking;
        private const int CAPTURE_WIDTH_PROPERTY = 3;
        private const int CAPTURE_HEIGHT_PROPERTY = 4;
        private static object Lock = new object();

        public Robot FirstCar { get; set; }
        public Robot SecondCar { get; set; }
        public int BLOB_MIN_SIZE { get; set; }
        public int BLOB_MAX_SIZE { get; set; }

        //determines how fast stationary objects are incorporated into the background mask ( higher = faster)
        public double LEARNING_RATE { get; set; }

        public ObjectTracker(int width = 640, int height = 480) {
            //webcam
            capture = new VideoCapture(0);

            //setting capture resolution
            capture.Set(CAPTURE_WIDTH_PROPERTY, width);
            capture.Set(CAPTURE_HEIGHT_PROPERTY, height);

            //Background subtractor, alternatives: MOG, GMG
            subtractor = new BackgroundSubtractorMOG2();

            oldFirstCar = CvPoint.Empty;
            FirstCar = new Robot(-1, -1);
            SecondCar = new Robot(-1, -1);

            BLOB_MIN_SIZE = 2500;
            BLOB_MAX_SIZE = 50000;
            LEARNING_RATE = 0.001;
        }

        public void StartTracking() {
            blobWindow = new CvWindow("blobs");
            oldFirstCar = CvPoint.Empty;
            oldSecondCar = CvPoint.Empty;
            Thread trackingThread = new Thread(this.Track);
            IsTracking = true;
            trackingThread.Priority = ThreadPriority.Highest;
            trackingThread.Start();

        }

        public void StopTracking() {
            IsTracking = false;
        }

        public void Track()
        {
            while (IsTracking) { 
                frame = new Mat();

                //get new frame from camera
                capture.Read(frame);

                //frame height == 0 => camera hasn't been initialized properly and provides garbage data
                while (frame.Height == 0)
                {
                    capture.Read(frame);
                }

                Mat sub = new Mat();

                //perform background subtraction with selected subtractor.
                subtractor.Run(frame, sub, LEARNING_RATE);

                IplImage src = (IplImage)sub;

                //binarize image
                Cv.Threshold(src, src, 250, 255, ThresholdType.Binary);

                IplConvKernel element = Cv.CreateStructuringElementEx(4, 4, 0, 0, ElementShape.Rect, null);
                Cv.Erode(src, src, element, 1);
                Cv.Dilate(src, src, element, 1);
                blobs = new CvBlobs();
                blobs.Label(src);

                blobs.FilterByArea(BLOB_MIN_SIZE, BLOB_MAX_SIZE);

                var blobList = SortBlobsBySize(blobs);

                CvBlob largest = null;
                CvBlob secondLargest = null;

                if (blobList.Count >= 1)
                {
                    largest = blobList[0];
                }

                if (blobList.Count >= 2)
                {
                    secondLargest = blobList[1];
                }

                IplImage render = new IplImage(src.Size, BitDepth.U8, 3);

                blobs.RenderBlobs(src, render);

                blobWindow.ShowImage(render);

                Cv2.WaitKey(1);
                if ((largest != null) && (secondLargest != null))
                {
                    LinearPrediction(largest, secondLargest);
                } else if ((largest != null) && (secondLargest == null))
                {
                    LinearPrediction(largest);
                }
            }
        }

        /// <summary>
        /// Compares the distance between the largest blob of the last cycle and the current largest and second largest blob.
        /// If the distance between the last largest and current largest is shorter than between the last largest and second largest 
        /// it returns the current largest as first element, otherwise it returns the second largest as second element
        /// </summary>
        /// <param name="largest">Largest detected blob</param>
        /// <param name="secondLargest">Second largest detected blob</param>
        private void LinearPrediction(CvBlob largest, CvBlob secondLargest)
        {
            if (largest != null)
            {
                CvPoint largestCenter = largest.CalcCentroid();
                CvPoint secondCenter = secondLargest.CalcCentroid();

                if ((oldFirstCar == CvPoint.Empty) || 
                    ((oldFirstCar.DistanceTo(largestCenter) < oldFirstCar.DistanceTo(secondCenter)) && 
                    oldSecondCar.DistanceTo(largestCenter) > oldSecondCar.DistanceTo(secondCenter)))
                {
                    oldFirstCar = largestCenter;
                    oldSecondCar = secondCenter;
                    
                    FirstCar.Width = CalculateDiameter(largest.MaxX, largest.MinX);
                    FirstCar.Height = CalculateDiameter(largest.MaxY, largest.MinY);

                    
                    SecondCar.Width = CalculateDiameter(secondLargest.MaxX, secondLargest.MinX);
                    SecondCar.Height = CalculateDiameter(secondLargest.MaxY, secondLargest.MinY);

                    EnqueuePlayers(CvPointToCoordinate(largestCenter), CvPointToCoordinate(secondCenter));
                }
                else
                {
                    oldFirstCar = secondCenter;
                    oldSecondCar = largestCenter;

                    SecondCar.Width = CalculateDiameter(largest.MaxX, largest.MinX);
                    SecondCar.Height = CalculateDiameter(largest.MaxY, largest.MinY);

                    FirstCar.Width = CalculateDiameter(secondLargest.MaxX, secondLargest.MinX);
                    FirstCar.Height = CalculateDiameter(secondLargest.MaxY, secondLargest.MinY);

                    EnqueuePlayers(CvPointToCoordinate(secondCenter), CvPointToCoordinate(largestCenter));
                }
            }
        }

        private void LinearPrediction(CvBlob blob)
        {
            CvPoint center = blob.CalcCentroid();

            if (oldFirstCar.DistanceTo(center) < oldSecondCar.DistanceTo(center))
            {
                EnqueuePlayers(CvPointToCoordinate(center), null);
            }
            else
            {
                EnqueuePlayers(null, CvPointToCoordinate(center));
            }
        }

        private void EnqueuePlayers(Coordinate firstPlayer, Coordinate secondPlayer)
        {
            lock(ObjectTracker.Lock)
            {
                if (firstPlayer != null)
                {
                    FirstCar.Coord.Enqueue(firstPlayer);
                }

                if (secondPlayer != null)
                {
                    SecondCar.Coord.Enqueue(secondPlayer);
                }
            }
        }

        private Coordinate CvPointToCoordinate(CvPoint point)
        {
            return new Coordinate(point.X, point.Y);
        }

        private int CalculateDiameter(int max, int min)
        {
            return max - min;
        }

        private List<CvBlob> SortBlobsBySize(CvBlobs blobs)
        {
            List<CvBlob> blobList = new List<CvBlob>();

            foreach(CvBlob blob in blobs.Values)
            {
                blobList.Add(blob);
            }

            return blobList.OrderByDescending(x => x.Area).ToList();
        }
    }
}
