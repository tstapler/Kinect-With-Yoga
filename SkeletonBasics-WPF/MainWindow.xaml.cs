﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// Authors: Sambhav P. Srirama, Quinn Murphy, Michael Parker, and Tyler Stapler
// Iowa State University February 8th 2014
//------------------------------------------------------------------------------


namespace Microsoft.Samples.Kinect.SkeletonBasics {
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using System.Windows.Media.Imaging;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;

        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private Pen modelPenBad = new Pen(Brushes.Red, 6);
        private Pen modelPenGood = new Pen(Brushes.Green, 6);
        private Pen personPen = new Pen(Brushes.Yellow, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        // Skeleton Model that person is compared to
        private Skeleton staticSkeleton;

        // Capture current skeleton for future model
        private bool buttonPressed;

        // Determine whether or not certain modes are active
        private bool dogeMode = false;
        private bool centerMode = false;

        // Flag to determine which info picture to display
        private int infoStatus = 0;

        // For measuring how long someone is in a pose
        private int startTime;
        private int endTime;
        private bool inPose;

        // Total distance for all of the joints
        private double totalRadius;

        // How long a pose must be held
        private const int HOLDTIME = 10;

        // How close someone must be (0 = perfect, 1 = OMG SO BAD)
        private const double THRESHOLD = 0.04;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow() {
            InitializeComponent();
            buttonPressed = false;
            inPose = false;
            staticSkeleton = null;
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext) {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom)) {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top)) {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left)) {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right)) {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e) {
            if (null != this.sensor)
                this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Add an event handler to be called whenever there is new color frame data
            //this.sensor.ColorFrameReady += this.AllFramesReady();

            // Display the drawing using our image control
            Image.Source = this.imageSource;

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors) {
                if (potentialSensor.Status == KinectStatus.Connected) {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor) {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try {
                    this.sensor.Start();
                } catch (IOException) {
                    this.sensor = null;
                }
            }

            if (null == this.sensor) {
                //this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (null != this.sensor) {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e) {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame()) {
                if (colorFrame != null) {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) {
            Skeleton[] skeletons = new Skeleton[0];
            DrawingContext info = this.drawingGroup.Open();
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) {
                if (skeletonFrame != null) {
                    Skeleton person = new Skeleton();
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    for (int i = 0; i < 6; i++) {
                        if (skeletons[i].Position.X != 0 || skeletons[i].Position.Y != 0) {
                            person = skeletons[i];
                            if (staticSkeleton != null) {
                                totalRadius = calcTotRadius(person, staticSkeleton);
                            }
                        }
                    }
                    if (person.Position.X == 0 && person.Position.Y == 0)
                        totalRadius = 1;
                    if (person != null && buttonPressed) {
                        staticSkeleton = person;
                        buttonPressed = false;
                        BinaryFormatter bf = new BinaryFormatter();
                        FileStream fs = File.Create("C:\\Users\\Michael\\Desktop\\output.txt");
                        bf.Serialize(fs, staticSkeleton);
                        fs.Close();
                    }
                    if (skeletons.Length != 0) {
                        try {
                            if (person != null && staticSkeleton != null) {
                                if (staticSkeleton != null && totalRadius < THRESHOLD && !inPose) {
                                    startTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                                    inPose = true;
                                } else if (staticSkeleton != null && totalRadius < THRESHOLD && inPose) {
                                    endTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                                    if (endTime - startTime > HOLDTIME) {
                                        infoStatus = 2;
                                        infoImage.Source = new BitmapImage(new System.Uri(@"C:\\done.png", UriKind.RelativeOrAbsolute));
                                    } else {
                                        infoStatus = 1;
                                        infoImage.Source = new BitmapImage(new System.Uri(@"C:\\hold.png", UriKind.RelativeOrAbsolute));
                                        Console.WriteLine(totalRadius);
                                    }
                                } else {
                                    inPose = false;
                                    if (infoStatus != 2)
                                        infoImage.Source = null;
                                }
                            }
                        } catch (Exception x) { 
                            Console.WriteLine(x.StackTrace);
                        }
                    }
                }
            }
            info.Close();
            using (DrawingContext dc = this.drawingGroup.Open()) {
                // Draw a transparent background to set the render size
                //dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                dc.DrawImage(this.colorBitmap, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                if (skeletons.Length != 0) {
                    foreach (Skeleton skel in skeletons) {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked) {
                            this.DrawBonesAndJoints(skel, dc);
                        } else if (skel.TrackingState == SkeletonTrackingState.PositionOnly) {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                if (staticSkeleton != null) { // Draw the static Skeleton
                    if (staticSkeleton.TrackingState == SkeletonTrackingState.Tracked) {
                        this.DrawBonesAndJoints(staticSkeleton, dc);
                    } else if (staticSkeleton.TrackingState == SkeletonTrackingState.PositionOnly) {
                        dc.DrawEllipse(
                        this.centerPointBrush,
                        null,
                        this.SkeletonPointToScreen(staticSkeleton.Position),
                        BodyCenterThickness,
                        BodyCenterThickness);
                    }

                    // prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                }
            }
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext) {

            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints) {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked) {
                    drawBrush = this.trackedJointBrush;
                } else if (joint.TrackingState == JointTrackingState.Inferred) {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null) {
                    if (centerMode && skeleton != staticSkeleton) {
                        drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.Cyan), 1), new Point(centerOfBalanceX(skeleton), RenderHeight), new Point(centerOfBalanceX(skeleton), 0));
                        drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.Cyan), 1), new Point(RenderWidth, centerOfBalanceY(skeleton)), new Point(0, centerOfBalanceY(skeleton)));
                    }

                    if (joint.JointType == JointType.Head && dogeMode && skeleton != staticSkeleton) {

                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        if (this.SkeletonPointToScreen(joint.Position).X - 50 > RenderWidth / 2) {
                            bi.UriSource = new System.Uri(@"C:\\dogehead.png", UriKind.RelativeOrAbsolute);
                        } else {
                            bi.UriSource = new System.Uri(@"C:\\dogehead2.png", UriKind.RelativeOrAbsolute);
                        }
                        bi.EndInit();
                        drawingContext.DrawImage(bi, new Rect(this.SkeletonPointToScreen(joint.Position).X - 50, this.SkeletonPointToScreen(joint.Position).Y - 53, 100, 106), null);

                    }
                    //drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint) {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y + 40);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1) {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked) {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred) {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked) {
                if (totalRadius < THRESHOLD && skeleton == staticSkeleton) {
                    modelPenGood.Thickness = 10;
                    drawPen = this.modelPenGood;
                } else if (skeleton == staticSkeleton) {
                    modelPenBad.Thickness = 10;
                    drawPen = this.modelPenBad;
                } else {
                    personPen.Thickness = 10;
                    drawPen = this.personPen;
                }
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        /// 


        private void button_Click(object sender, RoutedEventArgs e) {
            if (null != this.sensor) {
                this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                buttonPressed = true;
            }
        }

        private void setMountain(object sender, RoutedEventArgs e) {
            if (null != this.sensor) {
                try {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream fs = File.OpenRead("C:\\Mountain.txt");
                    fs.Position = 0;
                    staticSkeleton = (Skeleton)bf.Deserialize(fs);
                    fs.Close();
                } catch (FileNotFoundException f) {
                    staticSkeleton = null;
                }
            }
            infoImage.Source = null;
        }

        private void setTree(object sender, RoutedEventArgs e) {
            if (null != this.sensor) {
                try {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream fs = File.OpenRead("C:\\Tree.txt");
                    fs.Position = 0;
                    staticSkeleton = (Skeleton)bf.Deserialize(fs);
                    fs.Close();
                } catch (FileNotFoundException f) {
                    staticSkeleton = null;
                }
                infoImage.Source = null;
            }
        }

        private void setSalute(object sender, RoutedEventArgs e) {
            if (null != this.sensor) {
                try {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream fs = File.OpenRead("C:\\UpwardsSalute.txt");
                    fs.Position = 0;
                    staticSkeleton = (Skeleton)bf.Deserialize(fs);
                    fs.Close();
                } catch (FileNotFoundException f) {
                    staticSkeleton = null;
                }
            }
            infoImage.Source = null;
        }

        private void setWarrior(object sender, RoutedEventArgs e) {
            if (null != this.sensor) {
                try {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream fs = File.OpenRead("C:\\WarriorTwo.txt");
                    fs.Position = 0;
                    staticSkeleton = (Skeleton)bf.Deserialize(fs);
                    fs.Close();
                } catch (FileNotFoundException f) {
                    staticSkeleton = null;
                }
            }
            infoImage.Source = null;
        }

        private void setReverse(object sender, RoutedEventArgs e) {
            if (null != this.sensor) {
                try {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream fs = File.OpenRead("C:\\ReverseWarrior.txt");
                    fs.Position = 0;
                    staticSkeleton = (Skeleton)bf.Deserialize(fs);
                    fs.Close();
                } catch (FileNotFoundException f) {
                    staticSkeleton = null;
                }
            }
            infoImage.Source = null;
        }

        private void setFlower(object sender, RoutedEventArgs e) {
            if (null != this.sensor) {
                try {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream fs = File.OpenRead("C:\\MoonFlower.txt");
                    fs.Position = 0;
                    staticSkeleton = (Skeleton)bf.Deserialize(fs);
                    fs.Close();
                } catch (FileNotFoundException f) {
                    staticSkeleton = null;
                }
            }
            infoImage.Source = null;
        }

        private void setNone(object sender, RoutedEventArgs e) {
            staticSkeleton = null;
            infoImage.Source = null;
        }

        private double calcTotRadius(Skeleton person, Skeleton model) {
            double total = 0;
            if (model == null || person == null)
                return 1;
            System.Collections.IEnumerator personJoints = person.Joints.GetEnumerator();
            System.Collections.IEnumerator modelJoints = model.Joints.GetEnumerator();
            personJoints.MoveNext();
            modelJoints.MoveNext();
            bool flag = false;
            for (Joint jointP = (Joint)personJoints.Current; !flag; flag = personJoints.MoveNext(), jointP = (Joint)personJoints.Current) {
                for (Joint jointM = (Joint)modelJoints.Current; !flag; flag = modelJoints.MoveNext(), jointM = (Joint)modelJoints.Current) {
                    if (jointM.GetType().Equals(jointP.GetType())) {
                        total += System.Math.Sqrt(System.Math.Pow(jointM.Position.X - jointP.Position.X, 2) + System.Math.Pow(jointM.Position.Y - jointP.Position.Y, 2));
                    }
                }
                modelJoints.Reset();
                modelJoints.MoveNext();
            }
            return total;
        }

        private void dogebutton_Click(object sender, RoutedEventArgs e) {
            dogeMode = !dogeMode;
        }

        private void centerbutton_Click(object sender, RoutedEventArgs e) {
            centerMode = !centerMode;
        }

        private double centerOfBalanceX(Skeleton s) {
            double sum = 0;
            double num = 0;
            foreach (Joint j in s.Joints) {
                sum += this.SkeletonPointToScreen(j.Position).X;
                num++;
            }
            return sum / num;
        }

        private double centerOfBalanceY(Skeleton s) {
            double sum = 0;
            double num = 0;
            foreach (Joint j in s.Joints) {
                if (isTorso(j)) {
                    sum += (this.SkeletonPointToScreen(j.Position).Y) / 2.2;
                } else
                    sum += this.SkeletonPointToScreen(j.Position).Y;
                num++;
            }
            return sum / num;
        }

        private bool isTorso(Joint j) {
            return (j.JointType == JointType.Head || j.JointType == JointType.ShoulderCenter || j.JointType == JointType.ShoulderLeft || j.JointType == JointType.ShoulderRight ||
                                    j.JointType == JointType.Spine || j.JointType == JointType.HipCenter || j.JointType == JointType.HipLeft || j.JointType == JointType.HipRight);
        }
    }
}