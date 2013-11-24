using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace BetaWP7._1_ver2
{
    [DataContract]
    public class Astronomy
    {
        public Astronomy()
        {

        }
        [DataMember]
        public String percentIlluminated { get; set; }
        [DataMember]
        public String ageOfMoon { get; set; }
        [DataMember]
        public String phaseOfMoon { get; set; }
        [DataMember]
        public DateTime moonrise { get; set; }
        [DataMember]
        public DateTime moonset { get; set; }
        [DataMember]
        public DateTime sunrise { get; set; }
        [DataMember]
        public DateTime sunset { get; set; }
    }

   
}
