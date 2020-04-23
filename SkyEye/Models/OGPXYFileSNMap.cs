using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyEye.Models
{
    public class OGPXYFileSNMap
    {
        public static Dictionary<string, string> GetSNMap(string wafernum)
        {
            var ret = new Dictionary<string, string>();
            //var sql = "select  [FileName],[SN]+':::'+[Index] from [AIProjects].[dbo].[CouponData] where SN like '<wafernum>%' and [FileName] is not null order by [timestamp] desc";
            //sql = sql.Replace("<wafernum>", wafernum);
            //var dbret = DBUtility.ExeOGPSqlWithRes(sql);
            //foreach (var line in dbret)
            //{
            //    var fs = UT.O2S(line[0]);
            //    var sn = UT.O2S(line[1]);
            //    if (!ret.ContainsKey(fs))
            //    { ret.Add(fs, sn); }
            //}
            return ret;
        }
    }
}