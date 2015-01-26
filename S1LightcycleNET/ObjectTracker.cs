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
        private CalibrateCamera _calibration;
        private readonly VideoCapture _capture;
        private readonly CvWindow _blobWindow;
        private readonly BackgroundSubtractor _subtractor;
        private Mat _frame;
        private CvBlobs _blobs;
        private CvPoint _oldFirstCar;
        private CvPoint _oldSecondCar;
        private bool _isTracking;
        private const int CaptureWidthProperty = 3;
        private const int CaptureHeightProperty = 4;
        private readonly static object Lock = new object();

        private Thread _trackingThread;

        public Robot FirstCar { get; set; }
        public Robot SecondCar { get; set; }
        public int BlobMinSize { get; set; }
        public int BlobMaxSize { get; set; }

        //determines how fast stationary objects are incorporated into the background mask ( higher = faster)
        public double LearningRate { get; set; }

        public ObjectTracker() {
            //webcam
            _calibration = CalibrateCamera.GetInstance();
            _capture = _calibration.GetVideoCapture();

            //setting _capture resolution
            _capture.Set(CaptureWidthProperty, _calibration.camResolutionWidth);
            _capture.Set(CaptureHeightProperty, _calibration.camResolutionHeight);

            _blobWindow = new CvWindow("_blobs");

            //Background _subtractor, alternatives: MOG, GMG
            _subtractor = new BackgroundSubtractorMOG2();

            _oldFirstCar = CvPoint.Empty;
            FirstCar = new Robot(-1, -1);
            SecondCar = new Robot(-1, -1);

            /*BlobMinSize = 2500;
            BlobMaxSize = 50000;
            LearningRate = 0.001;*/
        }

        public void StartTracking() {
            _oldFirstCar = CvPoint.Empty;
            _oldSecondCar = CvPoint.Empty;
            _trackingThread = new Thread(this.Track);
            _isTracking = true;
            _trackingThread.Priority = ThreadPriority.Highest;
            _trackingThread.Start();
        }

        public void StopTracking() {
            _isTracking = false;
            _trackingThread.Abort();
        }

        public void Track()
        {
            CvWindow asdf = new CvWindow("roi");
            while (_isTracking) { 
                _frame = new Mat();

                //get new _frame from camera
                _capture.Read(_frame);

                //_frame height == 0 => camera hasn't been initialized properly and provides garbage data
                while (_frame.Height == 0)
                {
                    _capture.Read(_frame);
                }
                
                Mat sub = new Mat();

                
                //camera calibration - ROI
                CvPoint[] roiPoints = _calibration.GetCalibrationPoints();
                CvSize size = new CvSize(_calibration.GetROIWidth(), _calibration.GetROIHeight());
                //CvSize size = new CvSize(_calibration.GetROIHeight(), _calibration.GetROIWidth());
                CvRect roiRect = new CvRect(roiPoints[0], size);
                Mat srcRoi = _frame.Clone(roiRect);

                IplImage tmpImg = srcRoi.ToIplImage().Clone();
                asdf.ShowImage(tmpImg);

                //perform background subtraction with selected _subtractor.
                _subtractor.Run(srcRoi, sub, LearningRate);
                IplImage src = (IplImage)sub;

                //binarize image
                Cv.Threshold(src, src, 250, 255, ThresholdType.Binary);

                IplConvKernel element = Cv.CreateStructuringElementEx(4, 4, 0, 0, ElementShape.Rect, null);
                Cv.Erode(src, src, element, 1);
                Cv.Dilate(src, src, element, 1);
                _blobs = new CvBlobs();
                _blobs.Label(src);

                _blobs.FilterByArea(BlobMinSize, BlobMaxSize);

                var blobList = SortBlobsBySize(_blobs);

                CvBlob largest = null;
                CvBlob secondLargest = null;

                CvBlobs blobs = _blobs.Clone();

                if (blobList.Count >= 1)
                {
                    largest = blobList[0];
                    blobs.FilterByLabel(largest.Label);
                }

                if (blobList.Count >= 2)
                {
                    secondLargest = blobList[1];
                    _blobs.FilterByLabel(secondLargest.Label);
                }

                IplImage render = new IplImage(src.Size, BitDepth.U8, 3);
                
                _blobs.RenderBlobs(src, render);
                blobs.RenderBlobs(render, render);
                _blobWindow.ShowImage(render);

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

                if ((_oldFirstCar == CvPoint.Empty) || 
                    ((_oldFirstCar.DistanceTo(largestCenter) < _oldFirstCar.DistanceTo(secondCenter)) && 
                    _oldSecondCar.DistanceTo(largestCenter) > _oldSecondCar.DistanceTo(secondCenter)))
                {
                    _oldFirstCar = largestCenter;
                    _oldSecondCar = secondCenter;
                    
                    FirstCar.Width = CalculateDiameter(largest.MaxX, largest.MinX);
                    FirstCar.Height = CalculateDiameter(largest.MaxY, largest.MinY);

                    
                    SecondCar.Width = CalculateDiameter(secondLargest.MaxX, secondLargest.MinX);
                    SecondCar.Height = CalculateDiameter(secondLargest.MaxY, secondLargest.MinY);

                    EnqueuePlayers(new Coordinate(largestCenter), new Coordinate(secondCenter));
                }
                else
                {
                    _oldFirstCar = secondCenter;
                    _oldSecondCar = largestCenter;

                    SecondCar.Width = CalculateDiameter(largest.MaxX, largest.MinX);
                    SecondCar.Height = CalculateDiameter(largest.MaxY, largest.MinY);

                    FirstCar.Width = CalculateDiameter(secondLargest.MaxX, secondLargest.MinX);
                    FirstCar.Height = CalculateDiameter(secondLargest.MaxY, secondLargest.MinY);

                    EnqueuePlayers(new Coordinate(secondCenter), new Coordinate(largestCenter));
                }
            }
        }

        private void LinearPrediction(CvBlob blob)
        {
            CvPoint center = blob.CalcCentroid();

            if (_oldFirstCar.DistanceTo(center) < _oldSecondCar.DistanceTo(center))
            {
                EnqueuePlayers(new Coordinate(center), null);
            }
            else
            {
                EnqueuePlayers(null, new Coordinate(center));
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
