using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyEye.Models
{
    public class ModuleSNWaferMap
    {
        public static void StoreData(string sn, string wf)
        {
            var sql = "insert into ModuleSNWaferMap(SN,WaferNum) values(@SN,@WaferNum)";
            var dict = new Dictionary<string, string>();
            dict.Add("@SN", sn);
            dict.Add("@WaferNum", wf);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static Dictionary<string,string> GetSN2WF()
        {
            var ret = new Dictionary<string, string>();

            var sql = "select SN,WaferNum from ModuleSNWaferMap";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = UT.O2S(line[0]);
                var wf = UT.O2S(line[1]);
                if (!ret.ContainsKey(sn))
                { ret.Add(sn, wf); }
            }

            return ret;
        }

        public ModuleSNWaferMap()
        {
            SN = "";
            WaferNum = "";
        }

        public string SN { set; get; }
        public string WaferNum { set; get; }
    }
}