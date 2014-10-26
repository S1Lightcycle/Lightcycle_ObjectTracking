using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Utilities;
using OpenCvSharp;
using OpenCvSharp.Blob;

namespace S1LightcycleNET
{
    class Program
    {



        private static int CAPTURE_WIDTH_PROPERTY = 3;
        private static int CAPTURE_HEIGHT_PROPERTY = 4;


        static void Main(string[] args)
        {
            VideoCapture capture = new VideoCapture(0);
            Mat frame = new Mat();
            CvWindow cvwindow = new CvWindow("blobs");
            Window subwindow = new Window("subtracted");
            Window window = new Window("original");

            if(!capture.IsOpened()){
                return;
            }


            //setting capture resolution
            capture.Set(CAPTURE_WIDTH_PROPERTY, 640);
            capture.Set(CAPTURE_HEIGHT_PROPERTY, 480);

            BackgroundSubtractor subtractor = new BackgroundSubtractorMOG2();
            int key = -1;
            while (key == -1) {

                capture.Read(frame);
                Mat sub = new Mat();
                subtractor.Run(frame, sub, 0.001);

                window.ShowImage(frame);

                IplImage src = (IplImage)sub;
                IplImage binary = new IplImage(src.Size, BitDepth.U8, 1);

                CvBlobs blobs = new CvBlobs();
                blobs.Label(binary);

                IplImage render = new IplImage(src.Size, BitDepth.U8, 3);
                Cv.Threshold(src, src, 100, 255, ThresholdType.Binary);
                blobs.RenderBlobs(src, render);
                cvwindow.ShowImage(render);
                subwindow.ShowImage(sub);
                key = Cv2.WaitKey(1);
            }

            
            
        }

    }
}
