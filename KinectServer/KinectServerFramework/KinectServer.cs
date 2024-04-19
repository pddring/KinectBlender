using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Kinect;

namespace KinectServerFramework
{
    public partial class KinectServer : Form, ILogger, IRequestFrame
    {
        public KinectServer()
        {
            InitializeComponent();
        }
        bool[] trackedBodies = null;
        int trackedBodyCount = 0;
        
        KinectSensor k = KinectSensor.GetDefault();
        WebServer web;
        CoordinateMapper coordinateMapper;
        BodyFrameReader bodyFrameReader;
        Body[] bodies = null;
        List<Tuple<JointType, JointType>> bones;
        private void Form1_Load(object sender, EventArgs e)
        {

            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty
            | BindingFlags.Instance | BindingFlags.NonPublic, null,
            tab3D, new object[] { true });

            web = new WebServer(this, this);
            k.IsAvailableChanged += K_IsAvailableChanged;
            
            coordinateMapper = k.CoordinateMapper;
            FrameDescription fd = k.DepthFrameSource.FrameDescription;
            Log($"Width: {fd.Width} x Height: {fd.Height}");
            bodyFrameReader = k.BodyFrameSource.OpenReader();
            bones = new List<Tuple<JointType, JointType>>();

            // Torso
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

            bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;
            k.Open();
            web.Start();
            UpdateStatus();
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using(BodyFrame f = e.FrameReference.AcquireFrame())
            {
                if(f != null)
                {
                    if(bodies == null)
                    {
                        bodies = new Body[f.BodyCount];
                        Log($"Allocating memory for {f.BodyCount} bodies");
                        trackedBodies = new bool[f.BodyCount];
                    }
                    f.GetAndRefreshBodyData(bodies);
                    int trackedBodyCount = 0;
                    string json = "";
                    for(int i = 0; i < bodies.Length; i++)
                    {
                        Body b = bodies[i];
                        trackedBodies[i] = b.IsTracked;
                        if(b.IsTracked)
                        {
                            trackedBodyCount++;
                            Armature a = new Armature(b, i, coordinateMapper);

                            json += txtPreview.Text = a.GetJSON();
                            Draw(a);
                        }
                    }
                    txtPreview.Text = json;
                    this.trackedBodyCount = trackedBodyCount;
                    UpdateStatus();
                }
                
            }
            
        }

        Bitmap imagePreview = null;

        public void Draw(Armature a)
        {
            if(imagePreview == null)
            {
                imagePreview = new Bitmap(tab3D.Width, tab3D.Height);
            }

            if(imagePreview.Width != tab3D.Width || imagePreview.Height != tab3D.Height)
            {
                imagePreview = new Bitmap(tab3D.Width, tab3D.Height);
            }

            using (Graphics g = Graphics.FromImage(imagePreview))
            {
                g.Clear(Color.Black);
                int jointWidth = 10;
                int jointHeight = 10;
                float scaleX = 200;
                float scaleY = 200;
                int offsetX = 50;
                int offsetY = 50;
                SolidBrush jointBrush = new SolidBrush(Color.Yellow);

                // draw all joints
                foreach(JointType j in a.b.Joints.Keys)
                {
                    g.FillEllipse(jointBrush,
                        (imagePreview.Width/2) - (a.b.Joints[j].Position.X * scaleX) + offsetX,
                        (imagePreview.Height/2) - (a.b.Joints[j].Position.Y * scaleY) + offsetY,
                        jointWidth, jointHeight);
                }
                
            }
            tab3D.Invalidate();
        }

        private void K_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            StringWriter s = new StringWriter();
            s.Write($"Kinect Available: {k.IsAvailable} ");
            s.Write($"Connected: {k.IsOpen} ");
            s.Write($"Web server running: {web.IsRunning()} ");
            s.Write($"Tracked bodies: {trackedBodyCount}");
            if(trackedBodyCount > 0)
            {
                s.Write(" (");
                for(int i =  0; i < trackedBodies.Length; i++)
                {
                    s.Write(trackedBodies[i]?i.ToString():"_");
                }
                s.Write(" )");
            }
            lblStatus.Text = s.ToString();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(web.IsRunning())
            {
                web.Stop();
            } else
            {
                web.Start();
            }
            UpdateStatus();
        }

        public void Log(string message)
        {
            if (message == null)
                return;
            if(lstLog.InvokeRequired)
            {
                lstLog.Invoke(new Action(() =>
                {
                    lstLog.Items.Add(message);
                    lstLog.SelectedIndex = lstLog.Items.Count - 1;
                }));
            }
            else
            {
                lstLog.Items.Add(message);
                lstLog.SelectedIndex = lstLog.Items.Count - 1;

            }
        }

        public string GetArmature(int bodyID)
        {
            if(bodyID >= 0 && bodyID <= 5 && bodies != null) {
                Armature a = new Armature(bodies[bodyID], bodyID, coordinateMapper);
                return a.ToString();
            }
            return "{\"valid\":\"false\"}";
        }

        private void tab3D_Paint(object sender, PaintEventArgs e)
        {
            if (imagePreview != null)
            {
                e.Graphics.DrawImage(imagePreview, 0, 0);
            }
        }
    }
}
