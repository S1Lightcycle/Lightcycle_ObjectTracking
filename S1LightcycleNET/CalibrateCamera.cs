using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S1lightcycle {
    public class CalibrateCamera {
        private const int ImageNum = 3;
        private readonly VideoCapture _capture;
        private Mat _frame;
        private const int CaptureWidthProperty = 3;
        private const int CaptureHeightProperty = 4;
        private CvPoint[] CalibrationPoints = new CvPoint[4];
        private int countClicks = 0;
        private CvWindow cvFrame;
        IplImage srcImg;

        public void detectEdges() {
            /*
            int cornerCount = 150;
            IplImage srcImg = getPicture();

            using (srcImg = new IplImage(@"02.JPG", LoadMode.AnyColor | LoadMode.AnyDepth))
            using (IplImage srcImgGray = new IplImage(@"02.JPG", LoadMode.GrayScale))
            using (IplImage eigImg = new IplImage(srcImgGray.GetSize(), BitDepth.F32, 1))
            using (IplImage tempImg = new IplImage(srcImgGray.GetSize(), BitDepth.F32, 1)) {
                CvPoint2D32f[] corners;

                Cv.GoodFeaturesToTrack(srcImgGray, eigImg, tempImg, out corners, ref cornerCount, 0.1, 15);
                Cv.FindCornerSubPix(srcImgGray, corners, cornerCount, new CvSize(3, 3), new CvSize(-1, -1), new CvTermCriteria(20, 0.03));

                for (int i = 0; i < cornerCount; i++)
                    Cv.Circle(dstImg, corners[i], 3, new CvColor(0, 0, 255), 2);

                using (new CvWindow("Corners", WindowMode.AutoSize, dstImg)) {
                    Cv.WaitKey(0);
                }
            }*/
        }

    

        public CalibrateCamera() {
            _capture = new VideoCapture(0);
            //setting _capture resolution
            _capture.Set(CaptureWidthProperty, 1280);
            _capture.Set(CaptureHeightProperty, 720);
        }

        public void ShowFrame() {
            _frame = new Mat();

            //get new _frame from camera
            _capture.Read(_frame);
            srcImg = _frame.ToIplImage();


            //_frame height == 0 => camera hasn't been initialized properly and provides garbage data
            while (_frame.Height == 0) {
                _capture.Read(_frame);

            }
            cvFrame = new CvWindow("edge calibration editor", WindowMode.Fullscreen, srcImg);
            cvFrame.OnMouseCallback += new CvMouseCallback(OnMouseDown);
            
        }

        public void OnMouseDown(MouseEvent me, int x, int y, MouseEvent me2) {
            if (me == MouseEvent.LButtonDown) {
                Console.WriteLine("x-coord: " + x);
                Console.WriteLine("y-coord: " + y);
                CvPoint point = new CvPoint(x, y);
                CalibrationPoints[countClicks] = point;
                countClicks++;
                Cv.Circle(srcImg, point, 10, new CvColor(255, 0, 0), 5);
                cvFrame.Image = srcImg;


                if (countClicks > 3) {
                    cvFrame.Close();
                    
                }
            }
            
        }
    }
}
