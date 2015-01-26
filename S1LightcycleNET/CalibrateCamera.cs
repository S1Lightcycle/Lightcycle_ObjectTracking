using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace S1lightcycle {
    public class CalibrateCamera {
        private VideoCapture _capture;
        private Mat _frame;
        private const int CaptureWidthProperty = 3;
        private const int CaptureHeightProperty = 4;
        public int camResolutionWidth = 1280;
        public int camResolutionHeight = 720;
        private CvPoint[] CalibrationPoints = new CvPoint[2];
        private int countClicks = 0;
        private CvWindow cvFrame;
        private IplImage srcImg;
        private static CalibrateCamera instance;

        public int GetROIWidth() {
            return S1LightcycleNET.Properties.Settings.Default.x2 - S1LightcycleNET.Properties.Settings.Default.x1;
        }

        public int GetROIHeight() {
            return S1LightcycleNET.Properties.Settings.Default.y2 - S1LightcycleNET.Properties.Settings.Default.y1;
        }

        public VideoCapture GetVideoCapture() {
            return _capture;
        }

        public CvPoint[] GetCalibrationPoints() {
            CvPoint point1 = new CvPoint(S1LightcycleNET.Properties.Settings.Default.x1, S1LightcycleNET.Properties.Settings.Default.y1);
            CvPoint point2 = new CvPoint(S1LightcycleNET.Properties.Settings.Default.x2, S1LightcycleNET.Properties.Settings.Default.y2);
            CalibrationPoints[0] = point1;
            CalibrationPoints[1] = point2;
            return this.CalibrationPoints;
        }


        public static CalibrateCamera GetInstance() {
            if (instance == null) instance = new CalibrateCamera();
            return instance;
        }


        private CalibrateCamera() {
            _capture = new VideoCapture(0);
            _capture.Set(CaptureWidthProperty, camResolutionWidth);
            _capture.Set(CaptureHeightProperty, camResolutionHeight);
        }

        public void ShowFrame() {
            
            _frame = new Mat();

            //get new _frame from camera
            _capture.Read(_frame);
            


            //_frame height == 0 => camera hasn't been initialized properly and provides garbage data
            while (_frame.Height == 0) {
                _capture.Read(_frame);
            }
            for (int i = 0; i < 5; i++) {
                _capture.Read(_frame);
                Thread.Sleep(500);
            }
            
            srcImg = _frame.ToIplImage();
            cvFrame = new CvWindow("edge calibration editor", WindowMode.Fullscreen, srcImg);
            cvFrame.OnMouseCallback += new CvMouseCallback(OnMouseDown);
            
        }

        public void OnMouseDown(MouseEvent me, int x, int y, MouseEvent me2) {
            if (me == MouseEvent.LButtonDown) {
                if (countClicks > 1) countClicks = 0;
                
                Console.WriteLine((countClicks + 1) + ". Coordinate:");
                Console.Write("x-coord: " + x + ", ");
                Console.WriteLine("y-coord: " + y);

                CvPoint point = new CvPoint(x, y);
                Cv.Circle(srcImg, point, 10, new CvColor(255, 0, 0), 5);
                if (countClicks == 0) {
                    S1LightcycleNET.Properties.Settings.Default.x1 = x;
                    S1LightcycleNET.Properties.Settings.Default.y1 = y;
                } else if (countClicks == 1) {
                    S1LightcycleNET.Properties.Settings.Default.x2 = x;
                    S1LightcycleNET.Properties.Settings.Default.y2 = y;
                }
                countClicks++;
                S1LightcycleNET.Properties.Settings.Default.Save();
                cvFrame.Image = srcImg;

                if (countClicks > 1) {
                    cvFrame.Close();
                    
                }
            }
            
        }
    }
}
