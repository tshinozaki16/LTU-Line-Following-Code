using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using System.IO.Ports;

namespace emgu
{
    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private Thread _captureThread;
        SerialPort _serialPort;
        int thresh;
        int leftWhiteCount = 0;
        delegate void SetTextCallback(string text1);
        const byte STOP = 0x7F;
        const byte FLOAT = 0x0F;
        const byte FORWARD = 0x6F;
        const byte BACKWARD = 0x5F;
        byte[] buffer = { 0x01, 0, 0};

        public Form1()
        {
            InitializeComponent();
            numericUpDown1.Value = 150;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _capture = new VideoCapture();
            _captureThread = new Thread(DisplayWebcam);
            _captureThread.Start();
            _serialPort = new SerialPort("COM4", 2400);
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.Two;
            //_serialPort.Open();

        }
        private void SetText(string text1)
        {
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text1});
            }
            else
            {
                this.textBox1.Text = text1;
            }
        }
        private void DisplayWebcam()
        {
            while (_capture.IsOpened)
            {
                Mat frame = _capture.QueryFrame();
                CvInvoke.Resize(frame, frame, pictureBox1.Size);
                Image<Gray, Byte> img = frame.ToImage<Gray, Byte>();
                img = img.ThresholdBinary(new Gray(thresh), new Gray(255));
                img._ThresholdBinary(new Gray(thresh), new Gray(255));
                pictureBox1.Image = img.Bitmap;
                leftWhiteCount = 0;

                for (int x = 0; x < img.Width / 3; x++)
                {
                    for (int y = 0; y < img.Height; y++)
                    {
                        if (img.Data[y, x, 0] == 255)
                            leftWhiteCount++;
                    }
                }

                if (leftWhiteCount < 1000)
                {
                    this.SetText(leftWhiteCount.ToString());
                    buffer[1] = FLOAT;
                    buffer[2] = FORWARD;
                }
                else if (leftWhiteCount > 10000)
                {
                    this.SetText(leftWhiteCount.ToString());
                    buffer[1] = FORWARD;
                    buffer[2] = FLOAT;
                }
                else
                {
                    this.SetText(leftWhiteCount.ToString());
                    buffer[1] = FORWARD;
                    buffer[2] = FORWARD;
                }
                //_serialPort.Write(buffer, 0 ,3);                
            }
          
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _captureThread.Abort();
        }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            thresh = (byte)numericUpDown1.Value;
        }
    }
}
