using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;
using GMap.NET.MapProviders;
using System.Windows.Forms;

namespace tharsis_s
{
    public partial class Form2 : Form
    {
        double latitude1, latitude2,longitude1,longitude2;

        public Form2(double lat1, double long1, double lat2,double long2)
        {
            latitude1=lat1;
            longitude1=long1; 
            longitude2=long2;
            latitude2= lat2;
            InitializeComponent();
            gMap1.DragButton = MouseButtons.Left;
            gMap1.MapProvider = GMapProviders.GoogleMap;

            gMap1.MinZoom = 10;
            gMap1.MaxZoom = 50;
            gMap1.Zoom = 8;

            gMap2.DragButton = MouseButtons.Left;
            gMap2.MapProvider = GMapProviders.GoogleMap;

            gMap2.MinZoom = 10;
            gMap2.MaxZoom = 50;
            gMap2.Zoom = 8;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            gMap1.Position = new PointLatLng(latitude1, longitude1);
            gMap2.Position = new PointLatLng(latitude2, longitude2);
        }
    }
}
