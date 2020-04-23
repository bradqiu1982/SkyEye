using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyEye.Models
{
    public class SnCoord
    {
        public SnCoord() {
            lotnum = "";
            sn = "";
            x = "";
            y = "";
        }

        public string lotnum { set; get; }
        public string sn { set; get; }
        public string x { set; get; }
        public string y { set; get; }
    }
}