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
    public partial class KinectServer : Form
    {
        public KinectServer()
        {
            InitializeComponent();
        }
        
        KinectSensor k = KinectSensor.GetDefault();

        private void Form1_Load(object sender, EventArgs e)
        {
            k.IsAvailableChanged += K_IsAvailableChanged;
            if (k.IsAvailable)
            {
                k.Open();
            }
            UpdateStatus();
        }

        private void K_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            lblStatus.Text = $"Available: {k.IsAvailable} Connected: {k.IsOpen}";
        }
    }
}
