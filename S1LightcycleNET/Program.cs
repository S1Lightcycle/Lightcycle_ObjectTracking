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

            //webcam
            VideoCapture capture = new VideoCapture(0);
            Mat frame = new Mat();

            //output windows
            CvWindow cvwindow = new CvWindow("blobs");
            CvWindow subwindow = new CvWindow("subtracted");
            Window window = new Window("original");

            if(!capture.IsOpened()){
                return;
            }


            //setting capture resolution
            capture.Set(CAPTURE_WIDTH_PROPERTY, 640);
            capture.Set(CAPTURE_HEIGHT_PROPERTY, 480);


            //Background subtractor, alternatives: MOG, GMG
            BackgroundSubtractor subtractor = new BackgroundSubtractorMOG2();
            int key = -1;
            while (key == -1) {


                //get new frame from camera
                capture.Read(frame);
                Mat sub = new Mat();


                //determines how fast stationary objects are incorporated into the background mask ( higher = faster)
                double learningRate = 0.001;

                //perform background subtraction with selected subtractor.
                subtractor.Run(frame, sub, learningRate);


                //show the unaltered camera output
                window.ShowImage(frame);


                IplImage src = (IplImage)sub;

                //binarize image
                Cv.Threshold(src, src, 250, 255, ThresholdType.Binary);
                

                IplConvKernel element = Cv.CreateStructuringElementEx(2,2, 0, 0, ElementShape.Rect, null);
                Cv.Erode(src, src, element, 1);
                Cv.Dilate(src, src, element, 1);
                CvBlobs blobs = new CvBlobs();
                blobs.Label(src);
                
                
                IplImage render = new IplImage(src.Size, BitDepth.U8, 3);



                blobs.RenderBlobs(src, render);
                
                
                
                cvwindow.ShowImage(render);
                subwindow.ShowImage(src);
                
                key = Cv2.WaitKey(1);
            }

            
            
        }

    }
}
