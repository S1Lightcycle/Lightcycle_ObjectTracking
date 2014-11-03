﻿using System;
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

        public ObjectTracker()
        {
            //webcam
            capture = new VideoCapture(0);
            blobWindow = new CvWindow("blobs");
            subWindow = new CvWindow("subtracted");

            //setting capture resolution
            capture.Set(CAPTURE_WIDTH_PROPERTY, 320);
            capture.Set(CAPTURE_HEIGHT_PROPERTY, 240);

            //Background subtractor, alternatives: MOG, GMG
            subtractor = new BackgroundSubtractorMOG2();
        }

        public Tuple<Coordinate, Coordinate> track()
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
            return new Tuple<Coordinate, Coordinate>(calculateCenter(largest), calculateCenter(secondLargest));
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
    }
}
