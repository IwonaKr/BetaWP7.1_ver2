using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BetaWP7._1_ver2
{
    [DataContract]
    public class ForecastDay
    {
        public ForecastDay()
        {

        }
        [DataMember]
        public string period { get; set; }
        [DataMember]
        public string icon { get; set; }
        [DataMember]
        public string iconUrl { get; set; }
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string fcttext { get; set; }
        [DataMember]
        public string fcttextMetric { get; set; }
        [DataMember]
        public string pop { get; set; } //Probability of Precipitation -> prawdopodobieństwo opadów atmosferycznych
        [DataMember]
        public Date data { get; set; }

        //dla forecast z SIMPLE FORECAST
        [DataMember]
        public string highTempC { get; set; } //a może zamienic na int?
        [DataMember]
        public string lowTempC { get; set; } //temp w C
        [DataMember]
        public string conditions { get; set; } //warunki pogodowe
        [DataMember]
        public string skyicon { get; set; } //różne od icon, nie wiem  czemu
        [DataMember]
        public string qpfAllDay { get; set; } //quantitative precipitation forecast. - ilościowa prognoza opadów w przeciągu 3 następnych godzin
        //może double, wynik albo w in(cal) albo mm
        [DataMember]
        public string snowAllDay { get; set; } //śnieg w dzień, w cm albo calach
        [DataMember]
        public int avehumidity { get; set; } //średnia wilgotność
        [DataMember]
        public int maxhumidity { get; set; }
        [DataMember]
        public int minhumidity { get; set; }

        //public class MaxWind //maksymalny wiatr. Nie wiem co zrobić ze średnim wiatrem, bo pola są takie same...
        [DataMember]
        public int maxwind_mph { get; set; } //metry na godzinę
        [DataMember]
        public int maxwind_kph { get; set; } //km/h
        [DataMember]
        public string maxwind_dir { get; set; } //kierunek wiatru
        [DataMember]
        public string maxwind_degrees { get; set; } //stopnie w kierunku wiatru

        //śrdni wiatr. Nie wiem co zrobić ze średnim wiatrem, bo pola są takie same...
        [DataMember]
        public int avewind_mph { get; set; } //metry na godzinę
        [DataMember]
        public int avewind_kph { get; set; } //km/h
        [DataMember]
        public string avewind_dir { get; set; } //kierunek wiatru
        [DataMember]
        public string avewind_degrees { get; set; } //stopnie w kierunku wiatru

    }
}
