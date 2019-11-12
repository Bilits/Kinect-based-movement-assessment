using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Media;
using Microsoft.Kinect;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //////////ALI///////////////////////////

        public int Point = 0;
        public string Status = "";
        public string Message3 = "null";
        public int counter = 0;
        public bool norepeat = true;
        public Random rand = new Random();

        SoundPlayer wrong = new SoundPlayer(@"C:\Users\pegah\Desktop\SmarLab\kinect\Rehab\BendingExercise\Bendingexcersie - userinterface\WpfApp1\WpfApp1\attachments\wrong.wav");
        SoundPlayer goodjob = new SoundPlayer(@"C:\Users\pegah\Desktop\SmarLab\kinect\Rehab\BendingExercise\Bendingexcersie - userinterface\WpfApp1\WpfApp1\attachments\goodjob.wav");
        SoundPlayer welldone = new SoundPlayer(@"C:\Users\pegah\Desktop\SmarLab\kinect\Rehab\BendingExercise\Bendingexcersie - userinterface\WpfApp1\WpfApp1\attachments\welldone.wav");
        SoundPlayer tooquick = new SoundPlayer(@"C:\Users\pegah\Desktop\SmarLab\kinect\Rehab\BendingExercise\Bendingexcersie - userinterface\WpfApp1\WpfApp1\attachments\tooquick.wav");

        //////////ALI///////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;
        private KinectSensor kinectSensor = null;
        private BodyFrameReader bodyFrameReader = null;
        private CoordinateMapper coordinateMapper = null;
        private Body[] bodies = null;
        private List<Tuple<JointType, JointType>> bones;



        public MainWindow()
        {
            this.kinectSensor = KinectSensor.GetDefault();
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            this.bones = new List<Tuple<JointType, JointType>>();
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            this.kinectSensor.Open();
            this.DataContext = this;
            this.InitializeComponent();

        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text

        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {            
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }
        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                //int a = bodyFrame.RelativeTime.Seconds;
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                {
                    foreach (Body body in this.bodies)
                    {
                        if (body.IsTracked)
                        {
                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                            // convert the joint points to depth (display) space
                            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                            ///////////////////////////ALI/////////////////////////////////
                            if (body.Joints[JointType.HandTipRight].Position.Y < body.Joints[JointType.KneeRight].Position.Y || body.Joints[JointType.HandTipLeft].Position.Y < body.Joints[JointType.KneeLeft].Position.Y)
                            {
                                //Message2 = "Bending";
                                if (Math.Abs(body.Joints[JointType.Head].Position.X - (body.Joints[JointType.FootLeft].Position.X + body.Joints[JointType.FootRight].Position.X) / 2) > 0.2 && norepeat)
                                {
                                    SystemSounds.Beep.Play();
                                    counter = 0;
                                    this.norepeat = false;
                                    this.Status = "  Wrong \n Try Again";
                                    wrong.Play();
                                }
                                else
                                {
                                    if (norepeat)
                                    {
                                        counter += 1;
                                        if (counter == 40)
                                        {
                                            SystemSounds.Hand.Play();
                                            this.Point += 1;
                                            this.Status = "Correct!";
                                            if (this.Point%2 == 0)
                                            {
                                                goodjob.Play();
                                            }
                                            else
                                            {
                                                welldone.Play();
                                            }
                                            this.norepeat = false;
                                        }
                                    }
                                }

                            }
                            else
                            {
                                //Message2 = "Standing";
                                if(counter > 5 && counter < 40)
                                {
                                    this.Status = "Too Quick!";
                                    tooquick.Play();
                                }
                                this.counter = 0;
                                this.norepeat = true;
                            }

                        }
                        PointScore.Text = this.Point.ToString();
                        StatusText.Text= this.Status;
                        ///////////////////////////ALI/////////////////////////////////
                    }
                }
            }
        }
    }
}
