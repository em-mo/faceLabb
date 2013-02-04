// -----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace FaceTrackingBasics
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.FaceTracking;
    using System.Timers;
    using System.Collections.Generic;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        private WriteableBitmap colorImageWritableBitmap;
        private byte[] colorImageData;
        private ColorImageFormat currentColorImageFormat = ColorImageFormat.Undefined;
       
        // added
        private DateTime time = new DateTime();
        private List<Plupp> pluppar = new List<Plupp>();

        private Muncher muncher;
        private Random randomGenerator;
        //

        public MainWindow()
        {
            InitializeComponent();

            //added
            time = DateTime.Now;
            //

            var faceTrackingViewerBinding = new Binding("Kinect") { Source = sensorChooser };
            faceTrackingViewer.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding);

            sensorChooser.KinectChanged += SensorChooserOnKinectChanged;

            sensorChooser.Start();

            muncher = new Muncher();
            randomGenerator = new Random();
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs kinectChangedEventArgs)
        {
            KinectSensor oldSensor = kinectChangedEventArgs.OldSensor;
            KinectSensor newSensor = kinectChangedEventArgs.NewSensor;

            if (oldSensor != null)
            {
                oldSensor.AllFramesReady -= KinectSensorOnAllFramesReady;
                oldSensor.ColorStream.Disable();
                oldSensor.DepthStream.Disable();
                oldSensor.DepthStream.Range = DepthRange.Default;
                oldSensor.SkeletonStream.Disable();
                oldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                oldSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            }

            if (newSensor != null)
            {
                try
                {
                    newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    newSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                    try
                    {
                        // This will throw on non Kinect For Windows devices.
                        newSensor.DepthStream.Range = DepthRange.Near;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        newSensor.DepthStream.Range = DepthRange.Default;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    newSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    newSensor.SkeletonStream.Enable();
                    newSensor.AllFramesReady += KinectSensorOnAllFramesReady;
                }
                catch (InvalidOperationException)
                {
                    // This exception can be thrown when we are trying to
                    // enable streams on a device that has gone away.  This
                    // can occur, say, in app shutdown scenarios when the sensor
                    // goes away between the time it changed status and the
                    // time we get the sensor changed notification.
                    //
                    // Behavior here is to just eat the exception and assume
                    // another notification will come along if a sensor
                    // comes back.
                }
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            sensorChooser.Stop();
            faceTrackingViewer.Dispose();
        }

        public void TimerEnd()
        {
            Plupp plupp = new Plupp(true, 5, (float)this.ActualWidth/2, (float)this.ActualHeight/2);
            MainGrid.Children.Add(plupp.ellipse);
            System.Windows.Rect rect = plupp.returnRectangle();
            pluppar.Add(plupp);
        }

        private void KinectSensorOnAllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {

            using (var colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame())
            {
                if (colorImageFrame == null)
                {
                    return;
                }

                // Make a copy of the color frame for displaying.
                var haveNewFormat = this.currentColorImageFormat != colorImageFrame.Format;
                if (haveNewFormat)
                {
                    this.currentColorImageFormat = colorImageFrame.Format;
                    this.colorImageData = new byte[colorImageFrame.PixelDataLength];
                    this.colorImageWritableBitmap = new WriteableBitmap(
                        colorImageFrame.Width, colorImageFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                    ColorImage.Source = this.colorImageWritableBitmap;
                }

                colorImageFrame.CopyPixelDataTo(this.colorImageData);
                this.colorImageWritableBitmap.WritePixels(
                    new Int32Rect(0, 0, colorImageFrame.Width, colorImageFrame.Height),
                    this.colorImageData,
                    colorImageFrame.Width * Bgr32BytesPerPixel,
                    0);

                if (DateTime.Now.Second - time.Second > 3) 
                {
                    TimerEnd();
                    time = DateTime.Now;
                }

                foreach (Plupp plupp in pluppar)
                {
                    plupp.Update();
                }

                float mouthState = faceTrackingViewer.ReturnMouthState();

                if (muncher.checkBite(mouthState))
                {
                    foreach (Plupp plupp in checkPluppToHeadCollisions())
                    {
                        MainGrid.Children.Remove(plupp.ellipse);
                        pluppar.Remove(plupp);
                    }
                }

                Vector3DF vector = faceTrackingViewer.ReturnRotationValues();
                float mouthValue = mouthState;
                textBox1.Text = vector.X.ToString();
                textBox2.Text = vector.Y.ToString();
                textBox3.Text = vector.Z.ToString();
                textBoxMun.Text = mouthValue.ToString();
            }
        }

        private List<Plupp> checkPluppToHeadCollisions()
        {
            System.Windows.Rect faceRect = faceTrackingViewer.ReturnFaceRect();
            List<Plupp> collisionList = new List<Plupp>();

            foreach (Plupp plupp in pluppar)
            {
                if (faceRect.IntersectsWith(plupp.returnRectangle()))
                    collisionList.Add(plupp);
            }
            return collisionList;
        }

        private bool checkPluppToHeadTilt(System.Windows.Rect head, System.Windows.Rect plupp)
        {
            double diffX = head.X - plupp.X;
            double diffY = head.Y - plupp.Y;

            double angle = Math.Atan(diffY / diffX)* 360 / Math.PI;

            double faceAngle = faceTrackingViewer.ReturnRotationValues().X;

            if (Math.Abs(angle - faceAngle) < 5)
                return true;

            return false;
        }

        private bool checkPluppToHeadDirection(System.Windows.Rect head, System.Windows.Rect plupp)
        {
            double diffX = head.X - plupp.X;
            double headDirection = faceTrackingViewer.ReturnRotationValues().Y;

            if (diffX < 0 && headDirection < 0 || diffX > 0 && headDirection > 0)
                return true;

            return false;
        }

        private void SpawnFoodStuff(System.Windows.Rect faceRect)
        {
            int spawnX = randomGenerator.Next(0, 1) * 640;
            int spawnY = randomGenerator.Next(0, 480);

            double diffX = faceRect.X - spawnX;
            double diffY = faceRect.Y - spawnY;

            double angle = Math.Atan(diffY / diffX);

            double velocityX = Math.Cos(angle) * PluppVelocity;
            double velocityY = Math.Sin(angle) * PluppVelocity;
        }

        private const double PluppVelocity = 5;

        private class Muncher
        {
            private const float OpenThreshold = 0.5F;
            private const float ClosedThreshold = 0.2F;
            System.Windows.Rect rect;
            private enum MouthState { Open, Closed };

            private MouthState state = MouthState.Closed;

            public bool checkBite(float mouthPosition)
            {
                bool result = false;
                if (state == MouthState.Closed && mouthPosition > OpenThreshold)
                {
                    state = MouthState.Open;
                }
                else if (state == MouthState.Open && mouthPosition < ClosedThreshold)
                {   
                    state = MouthState.Closed;
                    result = true;
                }
                return result;
            }
        }
    }
}
