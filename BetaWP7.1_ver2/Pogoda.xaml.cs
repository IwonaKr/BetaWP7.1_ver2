﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Phone.Controls;

namespace BetaWP7._1_ver2
{
    public partial class Pogoda : PhoneApplicationPage,INotifyPropertyChanged
    {
        public string miasto=null;
        public bool czyToGPS; //true GPS, false - miasto
        public static string mess; //potrzebne do linka
        public static List<ForecastDay> dni2= new List<ForecastDay>(); //txt_forecast
        public static List<ForecastDay> SFDay = new List<ForecastDay>(); //SimpleForecast
        public static ObservableCollection<HourlyForecast> HourlyForecast = new ObservableCollection<HourlyForecast>();
        public static Astronomy astronomy;


        public XDocument doc=null;

        public Pogoda()
        {
            InitializeComponent();
            godzinowaLB.DataContext=HourlyForecast;

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string msg;
            if (NavigationContext.QueryString.TryGetValue("msg", out msg))
            {

                if (msg.Contains(","))
                {
                    mess=msg;
                    var cos = mess.Split(',');
                    if (cos.Length==4)
                    {
                        mess=cos[0]+"."+cos[1]+","+cos[2]+"."+cos[3];
                    }
                    this.miasto="GPS: "+msg;
                    Debug.WriteLine("GPS");
                    czyToGPS=true;
                }
                else
                {
                    this.miasto=msg;
                    mess="Poland/"+msg;
                    Debug.WriteLine("MIASTO");
                    czyToGPS=false;
                }
                this.miastoTB.Text=miasto;
                Thread t = new Thread(NewThread);
                t.Start();
            }
        }

        void NewThread()
        {
            string url = "http://api.wunderground.com/api/c9d15b10ff3ed303/alerts/conditions/forecast/forecast10day/astronomy/hourly/lang:PL/q/"+mess+".xml";

            WebClient wc = new WebClient();
            wc.DownloadStringCompleted +=wc_DownloadStringCompleted; //dodane, bez tej funkcji też działa!!
            wc.DownloadStringAsync(new Uri(url));

        }

        private void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            string weather="";
            //try
            //{
                weather = e.Result;
                XmlReader reader = XmlReader.Create(new StringReader(weather));
                doc = XDocument.Load(reader);
                obrabianieConditions(doc);


                /* ASTRONOMIA */
                astronomy = new Astronomy();
                int hTmp=0, mTmp=0, ho=0, m=0;

                var moon_phase = (from d in doc.Descendants()
                                  where (d.Name.LocalName=="moon_phase")
                                  select d).FirstOrDefault();
                astronomy.ageOfMoon=moon_phase.Element("ageOfMoon").Value;
                astronomy.percentIlluminated=moon_phase.Element("percentIlluminated").Value;
                var tmp = (from d in moon_phase.Descendants()
                           where (d.Name.LocalName=="sunset")
                           select d).FirstOrDefault();
                if ((int.TryParse(tmp.Element("hour").Value, out hTmp))&&(int.TryParse(tmp.Element("minute").Value, out mTmp)))
                {
                    ho=hTmp;
                    m=mTmp;
                    DateTime cos=new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, ho, m, 0);
                    astronomy.moonset=cos;
                }
                tmp = (from d in moon_phase.Descendants()
                       where (d.Name.LocalName=="sunrise")
                       select d).FirstOrDefault();
                if ((int.TryParse(tmp.Element("hour").Value, out hTmp))&&(int.TryParse(tmp.Element("minute").Value, out mTmp)))
                {
                    ho=hTmp;
                    m=mTmp;
                    DateTime cos=new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, ho, m, 0);
                    astronomy.moonrise=cos;
                }
                moon_phase = (from d in doc.Descendants()
                              where (d.Name.LocalName=="sun_phase")
                              select d).FirstOrDefault();
                tmp = (from d in moon_phase.Descendants()
                       where (d.Name.LocalName=="sunrise")
                       select d).FirstOrDefault();
                if ((int.TryParse(tmp.Element("hour").Value, out hTmp))&&(int.TryParse(tmp.Element("minute").Value, out mTmp)))
                {
                    ho=hTmp;
                    m=mTmp;
                    DateTime cos=new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, ho, m, 0);
                    astronomy.sunrise=cos;
                }
                tmp = (from d in moon_phase.Descendants()
                       where (d.Name.LocalName=="sunset")
                       select d).FirstOrDefault();
                if ((int.TryParse(tmp.Element("hour").Value, out hTmp))&&(int.TryParse(tmp.Element("minute").Value, out mTmp)))
                {
                    ho=hTmp;
                    m=mTmp;
                    DateTime cos=new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, ho, m, 0);
                    astronomy.sunset=cos;
                }

                Debug.WriteLine("Moonrise: "+astronomy.moonrise.ToShortTimeString()+",Moonset: "+astronomy.moonset.ToShortTimeString());
                Debug.WriteLine("Sunrise: "+astronomy.sunrise.ToShortTimeString()+", Sunset: "+astronomy.sunset.ToShortTimeString());

                var zwrocone = Serialize(astronomy);
                Debug.WriteLine(zwrocone.GetType().ToString());

                var obdzekt = Deserialize<Astronomy>(zwrocone, astronomy.GetType());
                Debug.WriteLine(obdzekt.GetType().ToString()+" "+obdzekt.phaseOfMoon);

                



                

                var hourly_forecast = (from d in doc.Descendants()
                                       where (d.Name.LocalName=="hourly_forecast")
                                       select d).ToList();
                Debug.WriteLine("HF!! "+hourly_forecast.ToString());

                var hourly = (from d in hourly_forecast.Descendants()
                              where (d.Name.LocalName=="forecast")
                              select d).ToList();
                foreach (var item in hourly)
                {
                    int iTmp=0;
                    HourlyForecast hf = new HourlyForecast();
                    var FCTTIME = (from d in item.Descendants()
                                   where (d.Name.LocalName=="FCTTIME")
                                   select d).FirstOrDefault();
                    int h=0, min=0, s=0, y=0, mon=0, mday=0;
                    if (int.TryParse(FCTTIME.Element("hour").Value, out iTmp))
                        h=iTmp;
                    if (int.TryParse(FCTTIME.Element("min").Value, out iTmp))
                        s=iTmp;
                    if (int.TryParse(FCTTIME.Element("year").Value, out iTmp))
                        y=iTmp;
                    if (int.TryParse(FCTTIME.Element("mon").Value, out iTmp))
                        mon=iTmp;
                    if (int.TryParse(FCTTIME.Element("mday").Value, out iTmp))
                        mday=iTmp;

                    DateTime dt = new DateTime(y, mon, mday, h, min, s);
                    hf.czas=dt;
                    hf.monAbbrev = FCTTIME.Element("mon_abbrev").Value;
                    hf.monthAbbrev=FCTTIME.Element("month_name_abbrev").Value;
                    hf.pretty=FCTTIME.Element("pretty").Value;
                    hf.weekdayNameAbbrev=FCTTIME.Element("weekday_name_abbrev").Value;
                    hf.weekdayNameNight=FCTTIME.Element("weekday_name_night").Value;
                    Debug.WriteLine(hf.czas.ToLongDateString()+" "+hf.czas.ToLocalTime()+" "+hf.weekdayNameAbbrev+" "+hf.monthAbbrev);
                    hf.condition=item.Element("condition").Value;
                    hf.icon=item.Element("icon").Value;
                    ///Dodane żeby było dobrze wyświetlane
                    hf.icon+=".png";
                    hf.iconUrl=item.Element("icon_url").Value;
                    hf.sky=item.Element("sky").Value;
                    hf.humidity=item.Element("humidity").Value;
                    hf.pop=item.Element("pop").Value;
                    hf.fctcode=item.Element("fctcode").Value;
                    hf.tempC=((from d in item.Descendants()
                               where d.Name.LocalName=="temp"
                               select d).FirstOrDefault()).Element("metric").Value;
                    hf.dewpointC=((from d in item.Descendants()
                                   where d.Name.LocalName=="dewpoint"
                                   select d).FirstOrDefault()).Element("metric").Value;
                    hf.windKph=((from d in item.Descendants()
                                 where d.Name.LocalName=="wspd"
                                 select d).FirstOrDefault()).Element("metric").Value;
                    hf.windDir=(((from d in item.Descendants() where d.Name.LocalName=="wdir" select d).FirstOrDefault()).Element("dir").Value);
                    hf.windDegrees=(((from d in item.Descendants() where d.Name.LocalName=="wdir" select d).FirstOrDefault()).Element("degrees").Value);

                    if (int.TryParse((((from d in item.Descendants()
                                        where d.Name.LocalName=="windchill"
                                        select d).FirstOrDefault()).Element("metric").Value), out iTmp))
                    {
                        if (iTmp<-100)
                            hf.windchill="";
                        else
                            hf.windchill=iTmp.ToString();
                    }

                    if (int.TryParse((((from d in item.Descendants()
                                        where d.Name.LocalName=="heatindex"
                                        select d).FirstOrDefault()).Element("metric").Value), out iTmp))
                    {
                        if (iTmp<-100)
                            hf.heatindex="";
                        else
                            hf.heatindex=iTmp.ToString();
                    }
                    hf.feelslike=(((from d in item.Descendants()
                                    where d.Name.LocalName=="feelslike"
                                    select d).FirstOrDefault()).Element("metric").Value);
                    hf.qpf=(((from d in item.Descendants()
                              where d.Name.LocalName=="qpf"
                              select d).FirstOrDefault()).Element("metric").Value);
                    hf.snow=(((from d in item.Descendants()
                               where d.Name.LocalName=="snow"
                               select d).FirstOrDefault()).Element("metric").Value);
                    hf.pressure=(((from d in item.Descendants()
                                   where d.Name.LocalName=="mslp"
                                   select d).FirstOrDefault()).Element("metric").Value);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        HourlyForecast.Add(hf);
                    });
                    /*Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("pl-PL");
                        System.Threading.Thread.CurrentThread.CurrentCulture = ci;
                        Bloczek b = new Bloczek();
                        b.godzina.Text=hf.czas.ToString("HH:mm");
                        b.dzien.Text=hf.czas.ToString("d MMM yyyy");
                        b.warunki.Text=hf.condition;
                        b.opady.Text="Opady: "+hf.qpf;
                        b.temperatura.Text="Temp: "+hf.tempC+"C";
                        ImageSource imgSrc;
                        if (hf.czas.Hour<=astronomy.sunrise.Hour || hf.czas.Hour>=astronomy.sunset.Hour)
                        {
                            imgSrc = new BitmapImage(new Uri("Icons/nt_"+hf.icon+".png", UriKind.Relative));
                        }
                        else
                        {
                            imgSrc = new BitmapImage(new Uri("Icons/"+hf.icon+".png", UriKind.Relative));
                        }

                        b.ikonka.Source=imgSrc;
                        hStackPanel.Children.Add(b);
                    });*/
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                    });
                }


            //}
            //catch (Exception ex)
            //{
            //    Deployment.Current.Dispatcher.BeginInvoke(() =>
            //    {
            //        MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK);
            //        this.textBox1.Text=ex.Message;
            //        this.ikonka.Source=null;
            //    });
            //}
            //costam(); //PAMIĘĆ 
        }

        public static void costam()
        {

            var poooooo = Serialize(dni2);
            Debug.WriteLine(poooooo.ToString());

            var toCoZwrocil = Deserialize<List<ForecastDay>>(poooooo, typeof(List<ForecastDay>));
            foreach (var item in toCoZwrocil)
            {
                Debug.WriteLine(item.icon);
            }

            IsolatedStorageSettings localStorage = IsolatedStorageSettings.ApplicationSettings;
            localStorage.Add("dni1", dni2);
            localStorage.Save();


            var ccc = localStorage["dni1"] as List<ForecastDay>;
            foreach (var item in ccc)
            {
                Debug.WriteLine(item.iconUrl);
            }
        }

        private void obrabianieConditions(XDocument doc)
        {
            if (czyToGPS)
            {
                var current_obs = (from d in doc.Descendants()
                                   where (d.Name.LocalName=="current_observation")
                                   select d).FirstOrDefault();
                Debug.WriteLine(current_obs.ToString());
                var disLoc = (from d in current_obs.Descendants()
                              where (d.Name.LocalName=="display_location")
                              select d).FirstOrDefault();
                //var place = (from d in disLoc.Descendants()
                //             where (d.Name.LocalName=="full")
                //             select d).FirstOrDefault();
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    this.miasto=disLoc.Element("full").Value;
                    this.miastoTB.Text=disLoc.Element("full").Value;
                    Debug.WriteLine(disLoc.ToString());
                });


            }

            var txt_forecast = (from d in doc.Descendants()
                                where (d.Name.LocalName == "txt_forecast")
                                select d).ToList();

            var forecast = (from d in txt_forecast.Descendants()
                            where (d.Name.LocalName=="forecastday")
                            select d).ToList();
            foreach (Object o in forecast)
            {
                Debug.WriteLine("TUTAJ!!"+o.ToString());
            }

            foreach (var item in forecast)
            {

                Console.WriteLine(item);
                ForecastDay d = new ForecastDay();
                d.period = item.Element("period").Value;
                d.icon=item.Element("icon").Value;
                d.iconUrl=item.Element("icon_url").Value;
                d.fcttext=item.Element("fcttext").Value;
                d.fcttextMetric=item.Element("fcttext_metric").Value;
                d.title=item.Element("title").Value;
                d.pop=item.Element("pop").Value;
                dni2.Add(d);
            }
            var simpleForecast = (from d in doc.Descendants()
                                  where (d.Name.LocalName=="simpleforecast")
                                  select d).ToList();

            var smplFrcstDay = (from d in simpleForecast.Descendants()
                                where (d.Name.LocalName=="forecastday")
                                select d).ToList();
            foreach (var item in smplFrcstDay)
            {
                string sTmp="";
                int iTmp=0;
                Console.WriteLine("****"+item);
                ForecastDay fd = new ForecastDay();

                fd.period = item.Element("period").Value;
                fd.icon=item.Element("icon").Value;
                fd.iconUrl=item.Element("icon_url").Value;
                fd.conditions=item.Element("conditions").Value;
                fd.pop=item.Element("pop").Value;

                //DATA
                Date d = new Date();
                var data = (from x in item.Descendants()
                            where x.Name.LocalName=="date"
                            select x).FirstOrDefault();

                Console.WriteLine("XXXXXXX: "+data.Element("day").Value);
                d.day=data.Element("day").Value;
                d.epoch=data.Element("epoch").Value;
                d.hour=data.Element("hour").Value;
                d.min=data.Element("min").Value;
                d.month=data.Element("month").Value;
                d.monthName=data.Element("monthname").Value;
                d.weekDay=data.Element("weekday").Value; //albo weekday_short , czyli skrót nazwy dnia tygodnia
                d.pretty=data.Element("pretty").Value;
                d.yday=data.Element("yday").Value;
                d.year=data.Element("year").Value;
                d.prettyShort=data.Element("pretty_short").Value;

                fd.data=d;


                //WIND NIE DZIAŁA, DOKOŃCZYĆ

                //MAX WIND
                var wnd = (from x in item.Descendants()
                           where x.Name.LocalName=="maxwind"
                           select x).FirstOrDefault();
                sTmp=wnd.Element("mph").Value;
                if (int.TryParse(sTmp, out iTmp))
                    fd.maxwind_mph=iTmp;
                sTmp=wnd.Element("kph").Value;
                if (int.TryParse(sTmp, out iTmp))
                    fd.maxwind_kph=iTmp;
                fd.maxwind_dir=wnd.Element("dir").Value;
                fd.maxwind_degrees=wnd.Element("degrees").Value;

                //AVERAGE WIND
                wnd = (from x in item.Descendants()
                       where x.Name.LocalName=="avewind"
                       select x).FirstOrDefault();
                sTmp=wnd.Element("mph").Value;
                if (int.TryParse(sTmp, out iTmp))
                    fd.avewind_mph=iTmp;
                sTmp=wnd.Element("kph").Value;
                if (int.TryParse(sTmp, out iTmp))
                    fd.avewind_kph=iTmp;
                fd.avewind_dir=wnd.Element("dir").Value;
                fd.avewind_degrees=wnd.Element("degrees").Value;

                Console.WriteLine(sTmp);
                // HUMIDITY
                sTmp=item.Element("avehumidity").Value;
                if (int.TryParse(sTmp, out iTmp))
                    fd.avehumidity=iTmp;
                sTmp=item.Element("maxhumidity").Value;
                if (int.TryParse(sTmp, out iTmp))
                    fd.maxhumidity=iTmp;
                sTmp=item.Element("minhumidity").Value;
                if (int.TryParse(sTmp, out iTmp))
                    fd.minhumidity=iTmp;

                //TEMPERATURE
                var temp2 = from m in item.Descendants()
                            where (m.Name.LocalName=="high")
                            select m;
                var temeperatura = from m in temp2.Descendants()
                                   where m.Name.LocalName=="celsius"
                                   select m.Value;

                sTmp = temeperatura.First();
                fd.highTempC=sTmp;
                temp2 = from m in item.Descendants()
                        where (m.Name.LocalName=="low")
                        select m;
                temeperatura = from m in temp2.Descendants()
                               where m.Name.LocalName=="celsius"
                               select m.Value;
                sTmp = temeperatura.First();
                fd.lowTempC=sTmp;
                SFDay.Add(fd);
            }

            var dzien = (from d in SFDay where d.period=="1" select d).FirstOrDefault();
            var dzien2 = (from d in dni2 where d.period=="0" select d).FirstOrDefault();
            if (!(dzien==null))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    this.textBox1.Text = "Period:             " + dzien.period+
                            "\nIconUri: " + dzien.iconUrl+
                            "\nPogoda:          " + dzien2.fcttext+
                            "\nfcttextMetric:     " + dzien2.fcttextMetric+
                            "\ntitle:           " + dzien2.title;
                    Uri uri = new Uri("Icons/"+dzien.icon+".png", UriKind.Relative);
                    ImageSource imgSource = new BitmapImage(uri);
                    this.ikonka.Source = imgSource;
                    TextBlock tb = new TextBlock();
                    tb.TextWrapping=TextWrapping.Wrap; //zawijanie tekstu
                    tb.Text = "Temp: "+dzien.lowTempC+"C-"+dzien.highTempC+"C\nWarunki: "+dzien.conditions+"\nWilgotność (min,max,śr): "+dzien.minhumidity+", "+dzien.maxhumidity.ToString()+","+dzien.avehumidity.ToString()+"\nWiatr (mile/h, km/h,kierunek): "+dzien.maxwind_mph.ToString()+","+dzien.maxwind_kph.ToString()+","+dzien.maxwind_dir;
                    TextBlock oDniu = new TextBlock();
                    oDniu.TextWrapping=TextWrapping.Wrap;
                    oDniu.Text="Dzisiaj jest "+dzien.data.day+" "+dzien.data.monthName+" "+dzien.data.year+", "+dzien.data.weekDay+". To "+dzien.data.yday+" dzień roku.";

                    this.glownyStackPanel.Children.Add(oDniu);
                    this.glownyStackPanel.Children.Add(tb);
                });
            }
        }

        public static string Serialize(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Position=0;
                return reader.ReadToEnd();
            }
        }

        public static T Deserialize<T>(string xml, Type toType)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position=0;
                DataContractSerializer deserializer = new DataContractSerializer(toType);
                return (T)deserializer.ReadObject(stream);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}