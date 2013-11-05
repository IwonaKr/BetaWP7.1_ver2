using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.GamerServices;
using System.Device.Location;

namespace BetaWP7._1_ver2
{
    public partial class MainPage : PhoneApplicationPage
    {
        GeoCoordinateWatcher geoWatcher = null;
        bool GPSorWybor=true; //true  to wpisywanie, false to gps
        public String miasto=null;
        GeoCoordinate currentLocation=null;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            geoWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            geoWatcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(geoWatcher_PositionChanged);
            geoWatcher.Start();

        }

        private void geoWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            //throw new NotImplementedException();
            currentLocation = e.Position.Location;
            double currentAltitude = e.Position.Location.Altitude;
            double currentLongitude = e.Position.Location.Longitude;
            double currnetLatitude = e.Position.Location.Latitude;
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            this.OKbtn.Visibility=Visibility.Visible;
            ApplicationBarIconButton temp = (sender as ApplicationBarIconButton);
            //ApplicationBarIconButton btn = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
            if (temp.Text=="gps")
            {
                Debug.WriteLine("GiePeEs z appbaru guziczkowego");
                this.wlaczGPS.Visibility=Visibility.Visible;
                this.GPSbtn.Visibility=Visibility.Visible;
                this.GPSTB.Visibility=Visibility.Visible;
                this.podajMiasto.Visibility=Visibility.Collapsed;
                this.miastoTB.Visibility=Visibility.Collapsed;
                if (this.GPSorWybor==false)
                    this.GPSorWybor=true;

            }
            else if (temp.Text=="miasto")
            {
                Debug.WriteLine("Miasto z appbaru guziczkowego");
                this.podajMiasto.Visibility=Visibility.Visible;
                this.miastoTB.Visibility=Visibility.Visible;
                this.wlaczGPS.Visibility=Visibility.Collapsed;
                this.GPSbtn.Visibility=Visibility.Collapsed;
                this.GPSTB.Visibility=Visibility.Collapsed;
                if (this.GPSorWybor)
                    this.GPSorWybor=false;
            }
        }

        private void GPSbtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentLocation==null)
            {
                Debug.WriteLine("current location jest nullem :C");
            }
            else
            {
                Debug.WriteLine(currentLocation.Latitude.ToString()+" "+currentLocation.Longitude.ToString());
                this.miasto=currentLocation.Latitude.ToString("0.0000")+","+currentLocation.Longitude.ToString("0.0000");
            }
            this.GPSTB.Text=currentLocation.Latitude.ToString("0.0000")+" "+currentLocation.Longitude.ToString("0.0000");
        }

        private void OKbtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.GPSorWybor) //czyli wybrany GPS
            {
                NavigationService.Navigate(new Uri("/Pogoda.xaml?msg="+this.miasto, UriKind.RelativeOrAbsolute));
            }
            else
            {
                this.miasto=this.miastoTB.Text;
                NavigationService.Navigate(new Uri("/Pogoda.xaml?msg="+this.miasto, UriKind.RelativeOrAbsolute));
            }
        }


    }
}