using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Emgu.CV;
using Emgu.CV.Structure;
using System.IO;
using System.Globalization;
using System.Net.Sockets;
using WinSCP;
using System.Reflection;


namespace tharsis_s
{
    public partial class Form1 : Form
    {

        #region TANIMLAMALAR

       
        bool isRecording = false;

        //int isVideoSend = -200;

        StringBuilder stringBuffer = new StringBuilder();
        SessionOptions sessionOptions;
        Session FTPsession;
        OpenFileDialog file;
        string videoPath;
        string videoName;
        byte[] bytes;

        TcpClient tcpClient = new TcpClient();
        NetworkStream networkStream = null;
        private StreamWriter streamWriter;
        public StreamWriter clientData;
        //Capture _capture;
        VideoWriter video_writer;
        #endregion

        VideoCapture capture;
        Image takenPhoto;
        public static SerialPort port;
        string textname;
        string output;
        string savePath = Environment.CurrentDirectory;


        GMapMarker marker;
        GMapMarker marker2;
        PointLatLng point1 = new PointLatLng();
        PointLatLng point2 = new PointLatLng();
        GMapOverlay markers = new GMapOverlay("markers");


        bool ayrilma=false;

        string[] splitVeri;

        public NetworkStream NetworkStream { get => networkStream; set => networkStream = value; }

        #region THREADS
        Thread cameraThread;
        Thread mapThread;
        Thread dataGridThread;
        Thread chartThread;
        Thread rpyInfoThread;
        Thread sendVideo;
        #endregion


        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;

            
            txtBuild();

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            #region SERIAL PORT SETUP

            comports.Items.Clear();
            String[] ports = SerialPort.GetPortNames();
            comports.Items.AddRange(ports);
            #endregion

            #region GMAP SETUP
            //GMap Initialize
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            gMap.DragButton = MouseButtons.Left;
            gMap.MapProvider = GMapProviders.GoogleMap;

            gMap.MinZoom = 10;
            gMap.MaxZoom = 200;
            gMap.Zoom = 17;
            
            gMap.Position = new PointLatLng(38.3991310, 33.7117840);

            //gMap.ShowCenter = true;
           
           // gMap.Refresh();
         
            #endregion

        }

        // FUNCTIONS
        


        private void drawMap(string gps1Latitude, string gps1Longitude, string gps2Latitude, string gps2Longitude)
        {
            double lat1 = double.Parse(gps1Latitude, CultureInfo.InvariantCulture);
            double lat2 = double.Parse(gps2Latitude, CultureInfo.InvariantCulture);
            double long1 = double.Parse(gps1Longitude, CultureInfo.InvariantCulture);
            double long2 = double.Parse(gps2Longitude, CultureInfo.InvariantCulture);

           // double lat2 = lat1 + 0.01;
           // double long2 = long1 + 0.01;

            gMap.Position = new PointLatLng(lat1, long1);
            markers.Clear();
            gMap.Overlays.Clear();

            try
            {
         
            //point1 = new PointLatLng(lat1, long1);
                point1.Lat = lat1;
                point1.Lng = long1;
            marker = new GMarkerGoogle(point1, GMarkerGoogleType.red_dot);

                // point2 = new PointLatLng(lat2, long2);
                point2.Lat = lat2;
                point2.Lng = long2;
            marker2 = new GMarkerGoogle(point2, GMarkerGoogleType.green_dot);

            //markers = new GMapOverlay("markers");
 

            gMap.Overlays.Add(markers);

             markers.Markers.Add(marker);   
            markers.Markers.Add(marker2);



              gMap.Refresh();
            }
            catch (Exception e)
            {
               MessageBox.Show(e.Message);
            }

            
        }

        private void RPYInfo(string[] splitVeri)
        {
            
            rollTextBox.Text = splitVeri[19].ToString();
            pitchTextBox.Text = splitVeri[18].ToString();
            yawTextBox.Text = splitVeri[20].ToString();

            try
            {

                Invoke(new Action(() =>
                {
                    rpySimulation.Rotate(double.Parse(splitVeri[19]), double.Parse(splitVeri[18]), double.Parse(splitVeri[20]));
                }));


            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void drawGraphs(string[] splitVeri)
        {
            try
            {
                if (!backgroundWorker1.IsBusy)
                {
                    backgroundWorker1.RunWorkerAsync();
                }
            }
            catch (Exception err)
            {

                MessageBox.Show(err.Message);
            }
            

        }

        private void addToGrid(string[] splitVeri)
        {
            DataGridViewRow row = (DataGridViewRow)veriGrid.Rows[0].Clone();
            for (int i = 0; i < 23; i++)
            {
               
                row.Cells[i].Value = splitVeri[i];

            }
            veriGrid.Rows.Add(row);

            if(veriGrid.Rows.Count > 6)
            {
                veriGrid.Rows.RemoveAt(0);
            } 
        }

        private void setCamera()
        {
            try
            {
                if (capture == null)
                {
                    capture = new VideoCapture(1);
                    capture.ImageGrabbed += Capture_ImageGrabbed;
                    capture.Start();
                    string path = savePath + @"\Saves\" + DateTime.Now.ToString().Replace(' ', '-').Replace(':', ';') + @" output.avi";
                    video_writer = new VideoWriter(path, VideoWriter.Fourcc('M', 'P', '4', 'V'), 30, new Size(640, 480), true);
                    isRecording = true;
                }

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }            
           

        }

        void saveText(String text)
        {
            try
            {
                StreamWriter writer = new StreamWriter(output, true);

                writer.Write(text);
                writer.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

        }

        void txtBuild()
        {
            try
            {
                textname = DateTime.Now.ToString().Replace(' ', '-');
                textname = textname.Replace(':', ';');
                textname = string.Concat(textname, ".csv");


                output = string.Concat(textname);

                output = savePath + @"\Saves\" + output;

                saveText("Takım No,Paket No, Görev Yükü Basınç,Taşıyıcı Basınç,Görev Yükü Yükseklik,Taşıyıcı Yükseklik,İrtifa Farkı, İniş Hızı,Sıcaklık,Pil Gerilimi,Latitude1,Longitude1,Altitude1,Latitude2,Longitude2,Altitude2,Statü,Pitch,Roll,Yaw,Dönüş Sayısı,Video Aktarımı\n");

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

        }

        void komutGonder(String komut)
        {
            DialogResult dialogResult = MessageBox.Show("Emin misiniz?", "Komut Onaylama", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                //do something
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(komut+"~\n\r");
                    MessageBox.Show("Komut Gönderildi.");
                    /*if (komut.Equals("A"))
                    {
                        btnAyrilma.Text = "BİRLEŞMEYİ GERÇEKLEŞTİR";
                    }
                    if (komut.Equals("B"))
                    {
                        btnAyrilma.Text = "AYRILMAYI GERÇEKLEŞTİR";
                    }*/
                }
                else
                {
                    MessageBox.Show("Port bağlı değil.");
                }
            }
            else if (dialogResult == DialogResult.No)
            {
                //do something else
                MessageBox.Show("Komut Gönderilemedi.");
            }
        }

        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    string newData = serialPort1.ReadExisting();

                    if (newData != null)
                    {
                        string[] telemetri = newData.Split(',');
                        
                       
                        /*isVideoSend += 1;
                        
                        if(isVideoSend==150)
                        {
                            labelTick.Visible = true;
                            labelCross.Visible = false;
                            komutGonder("V");
                        }*/
                        //textBox2.Text = newData;
                        if (telemetri.Length == 23)
                        {
                            if (!ayrilma && telemetri[17] == "AYRILMA")
                            {
                                ayrilma = true;
                                label8.Visible = true;
                                label7.Visible = false;
                            }

                            saveText(newData);
                            //telemetri[8] = GetRandomNumber(0.0, 1.0).ToString();
                            splitVeri = telemetri;
                            
                            chartThread = new Thread(() => drawGraphs(telemetri));
                            chartThread.Start();
                            Thread.Sleep(50);

                            dataGridThread = new Thread(() => addToGrid(telemetri));
                            dataGridThread.Start();
                            Thread.Sleep(50);
                            
                            rpyInfoThread = new Thread(() => RPYInfo(telemetri));
                            rpyInfoThread.Start();
                            Thread.Sleep(50);

                            mapThread = new Thread(() => drawMap(telemetri[11], telemetri[12], telemetri[14], telemetri[15]));
                            mapThread.Start();
                            Thread.Sleep(50);

                        }
                    }
                });

            }
            catch (Exception err)
            {

                MessageBox.Show(err.Message);
            }


            
        }

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                Mat m = new Mat();
                capture.Retrieve(m);

                Image<Bgr, byte> image = m.ToImage<Bgr, byte>();
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox1.Image = image.ToBitmap();

                if (isRecording && video_writer != null)
                {
                    video_writer.Write(m);
                }

            }
            catch (Exception)
            {

            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                irtifaLabel.Text = splitVeri[7].ToString();

                basıncChart.Series["Taşıyıcı"].Points.AddXY(splitVeri[1], double.Parse(splitVeri[4], CultureInfo.InvariantCulture));
                basıncChart.Series["Görev Yükü"].Points.AddXY(splitVeri[1], double.Parse(splitVeri[3], CultureInfo.InvariantCulture));

                yukseklikChart.Series["Taşıyıcı"].Points.AddXY(splitVeri[1], double.Parse(splitVeri[6], CultureInfo.InvariantCulture));
                yukseklikChart.Series["Görev Yükü"].Points.AddXY(splitVeri[1], double.Parse(splitVeri[5], CultureInfo.InvariantCulture));

                sıcaklıkChart.Series["Sıcaklık"].Points.AddXY(splitVeri[1], double.Parse(splitVeri[9], CultureInfo.InvariantCulture));

                hızChart.Series["İniş Hızı"].Points.AddXY(splitVeri[1], double.Parse(splitVeri[8], CultureInfo.InvariantCulture));

                pilChart.Series["Pil"].Points.AddXY(splitVeri[1], double.Parse(splitVeri[10], CultureInfo.InvariantCulture));
            }
            catch (Exception err)
            {

                MessageBox.Show(err.Message);
            }
        }

        //BUTTONS
        private void startButton_Click(object sender, EventArgs e)
        {
            //string File_path = "C:\\Users\\zubey\\Desktop\\İHA\\PDR\\turksat2022zub.csv";

            //Veri = File.ReadAllLines(File_path);

            //timer1.Start();

            cameraThread = new Thread(new ThreadStart(setCamera));
            cameraThread.Start();
            Thread.Sleep(50);

            serialPort1.DataReceived += SerialPort1_DataReceived;


        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = comports.Text;
                serialPort1.BaudRate = 115200;
                serialPort1.Parity = Parity.None;
                serialPort1.StopBits = StopBits.One;

                serialPort1.Open();
                port = serialPort1;
                if (serialPort1.IsOpen)
                {
                    progressBar1.Value = 100;
                    baglanLabel.Visible = true;

                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }


        private void takePhotoButton_Click(object sender, EventArgs e)
        {
            takenPhoto = pictureBox1.Image;
            /*SaveFileDialog s = new SaveFileDialog();
            s.FileName = "Resim_" + DateTime.Now.ToString("dd.MM.yyyy");
            s.DefaultExt = ".jpg";
            s.Filter = "Image (.jpg)|*.jpg";*/

            takenPhoto.Save(savePath + @"\Saves\" + "camera_goruntusu_" + DateTime.Now.ToString().Replace(' ', '-').Replace(':', ';') + ".png", System.Drawing.Imaging.ImageFormat.Png);
            /*if (s.ShowDialog() == DialogResult.OK)
            {
                //string DosyaAdi = s.FileName;
                //FileStream fstream = new FileStream(DosyaAdi, FileMode.Create);
                image.Save(s.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                //fstream.Close();
            }*/
        }

        private void savePhotoButton_Click(object sender, EventArgs e)
        {
            int xOffSet, yOffSet;
            var pikselSize = 5;
            if (takenPhoto == null)
            {
                MessageBox.Show("Önce Fotoğraf Çek...");
            }
            else
            {
                var originalImage = new Bitmap(takenPhoto);
                var imageWidth = originalImage.Width;
                var imageHeight = originalImage.Height;

                var pikselizedImage = new Bitmap(imageWidth, imageHeight);

                for (var i = 0; i < imageWidth; i += pikselSize)
                {
                    for (var j = 0; j < imageHeight; j += pikselSize)
                    {
                        xOffSet = yOffSet = pikselSize / 2;
                        if (i + xOffSet >= imageWidth)
                            xOffSet = imageWidth - i - 1;
                        if (j + yOffSet >= imageHeight)
                            yOffSet = imageHeight - j - 1;

                        var piksel = originalImage.GetPixel(i + xOffSet, j + yOffSet);

                        for (var x = i; x < i + pikselSize && x < imageWidth; x++)
                        {
                            for (var y = j; y < j + pikselSize && y < imageHeight; y++)
                            {
                                pikselizedImage.SetPixel(x, y, piksel);
                            }
                        }
                    }
                }
                pikselizedImage.Save(savePath + @"\Saves\" + "piksellestirilmis_goruntu_" + DateTime.Now.ToString().Replace(' ', '-').Replace(':', ';') + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }

       
        private void disconnectButton_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            port = serialPort1;
            if (!serialPort1.IsOpen)
            {
                progressBar1.Value = 0;
                baglanLabel.Visible = false;

            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            double lat1 = double.Parse(splitVeri[11], CultureInfo.InvariantCulture);
            double lat2 = double.Parse(splitVeri[14], CultureInfo.InvariantCulture);
            double long1 = double.Parse(splitVeri[12], CultureInfo.InvariantCulture);
            double long2 = double.Parse(splitVeri[15], CultureInfo.InvariantCulture);

            disconnectButton_Click(sender, e);
            capture.Stop();
            Form2 form2 = new Form2(lat1,long1,lat2,long2);
            form2.ShowDialog();
            
        }

        private void btnAyrilma_Click(object sender, EventArgs e)
        {
            /*if(btnAyrilma.Text.Equals("AYRILMAYI GERÇEKLEŞTİR"))
            {
                komutGonder("A");
                label8.Visible = true;
                label7.Visible = false;
            }
            else
            {*/
            ayrilma = true;
                komutGonder("A");
                label8.Visible = true;
                label7.Visible = false;
           // }
            
        }

        private void motorButton_Click(object sender, EventArgs e)
        {
            komutGonder("T");
        }

        private void sensorButton_Click(object sender, EventArgs e)
        {
            komutGonder("K");
        }

        private void sıfırlaButton_Click(object sender, EventArgs e)
        {
            komutGonder("Z");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(button1.Text == "Kes")
            {
                labelTick.Visible = true;
                labelCross.Visible = false;
                tcpClient.Dispose();
                tcpClient.Close();
                NetworkStream.Close();
                if (!tcpClient.Connected)
                {
                    button1.Text = "Bağlan";
                }
            }
            else
            {
                tcpClient.Connect("192.168.4.1", 80);
                NetworkStream = tcpClient.GetStream();
                streamWriter = new StreamWriter(NetworkStream);
                if (tcpClient.Connected)
                {
                    button1.Text = "Kes";
                }
            }
            

        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            file = ofd;
            if (file.ShowDialog() == DialogResult.OK)
            {
                videoPath = file.FileName;
                videoName = file.SafeFileName;
                MessageBox.Show("Gönderilmek istenen dosya:" + videoPath, "", MessageBoxButtons.OKCancel);
                bytes = File.ReadAllBytes(videoPath);



            }
            try
            {
                if (!backgroundWorker2.IsBusy)
                {
                    backgroundWorker2.RunWorkerAsync();
                }
            }
            catch (Exception err)
            {

                MessageBox.Show(err.Message);
            }


        }
        void SendVideo()
        {
            if (NetworkStream.CanWrite)
            {
                NetworkStream.Write(bytes, 0, bytes.Length);
                //textBox1.Text = "GÖNDERİLİYOR";
                NetworkStream.Flush();
                

                if (serialPort1.IsOpen)
                {
                    serialPort1.Write("V~\n\r");
                }

            }
            else if (!NetworkStream.CanWrite)
            {
                MessageBox.Show("Video Gönderilemiyor.");

            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            sendVideo = new Thread(() => SendVideo());
            sendVideo.Start();
            Thread.Sleep(50);
            
        }


        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        private void birlesBtn_Click(object sender, EventArgs e)
        {
            komutGonder("B");
            label8.Visible = false;
            label7.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            komutGonder("P");
        }
    }

    


}
