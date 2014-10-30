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
            //Window window = new Window("original");

            if(!capture.IsOpened()){
                return;
            }


            //setting capture resolution
            capture.Set(CAPTURE_WIDTH_PROPERTY, 360);
            capture.Set(CAPTURE_HEIGHT_PROPERTY, 240);


            //Background subtractor, alternatives: MOG, GMG
            BackgroundSubtractor subtractor = new BackgroundSubtractorMOG2();
            int key = -1;
            while (key == -1) {

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
                //window.ShowImage(frame);


                IplImage src = (IplImage)sub;

                //binarize image
                Cv.Threshold(src, src, 250, 255, ThresholdType.Binary);
                

                IplConvKernel element = Cv.CreateStructuringElementEx(4,4, 0, 0, ElementShape.Rect, null);
                Cv.Erode(src, src, element, 1);
                Cv.Dilate(src, src, element, 1);
                CvBlobs blobs = new CvBlobs();
                blobs.Label(src);


                int blobMINsize = 2500;
                int blobMAXsize = 50000;
                blobs.FilterByArea(blobMINsize, blobMAXsize);
                
                IplImage render = new IplImage(src.Size, BitDepth.U8, 3);
                CvBlob largest = blobs.LargestBlob();
                if (largest != null) {
                    blobs.FilterByArea(largest.Area - 1500, largest.Area);
                    Console.Out.WriteLine(blobs.Count);
                    Console.Out.WriteLine(largest.Area);
                }
                
                blobs.RenderBlobs(src, render);
                
                
                
                cvwindow.ShowImage(render);
                subwindow.ShowImage(src);
                
                key = Cv2.WaitKey(1);
            }

            
            
        }

    }
}
