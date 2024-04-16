using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Kinect;

namespace KinectServerFramework
{
    public partial class KinectServer : Form, Logger
    {
        public KinectServer()
        {
            InitializeComponent();
        }
        
        KinectSensor k = KinectSensor.GetDefault();
        WebServer web;

        private void Form1_Load(object sender, EventArgs e)
        {
            web = new WebServer(this);
            k.IsAvailableChanged += K_IsAvailableChanged;
            if (k.IsAvailable)
            {
                k.Open();
            }
            web.Start();
            UpdateStatus();
        }

        private void K_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            lblStatus.Text = $"Kinect Available: {k.IsAvailable} Connected: {k.IsOpen}. Web server running: {web.IsRunning()}";
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

        void Logger.Log(string message)
        {
            if (message == null)
                return;
            if(lstLog.InvokeRequired)
            {
                lstLog.Invoke(new Action(() =>
                {
                    lstLog.Items.Add(message);
                }));
            }
            else
            {
                lstLog.Items.Add(message);
            }
        }
    }
}
