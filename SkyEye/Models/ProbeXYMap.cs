using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyEye.Models
{
    public class ProbeXYMap
    {
        public static Dictionary<string, bool> GetProbeXYMap(string wafernum)
        {
            var ret = new Dictionary<string, bool>();
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferID", wafernum.ToUpper().Replace("E","").Replace("R", "").Replace("T", ""));

            var sql = "select Xcoord,Ycoord  from [EngrData].[dbo].[VR_Eval_Pts_Data_Basic] where WaferID = @WaferID";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,dict);
            foreach (var line in dbret)
            {
                var X = UT.O2S(UT.O2I(line[0]));
                var Y = UT.O2S(UT.O2I(line[1]));
                var key = X + ":::" + Y;
                if (!ret.ContainsKey(key))
                { ret.Add(key, true); }
            }
            return ret;
        }

    }
}