using OpenCvSharp;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace SkyEye.Models
{
    public class UT
    {
        public static string O2S(object obj)
        {
            if (obj != null)
            {
                try
                {
                    return Convert.ToString(obj);
                }
                catch (Exception ex) { return string.Empty; }
            }
            return string.Empty;
        }

        public static DateTime O2T(object obj)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                try
                {
                    return Convert.ToDateTime(obj);
                }
                catch (Exception ex) { return DateTime.Parse("1982-05-06 10:00:00"); }
            }
            return DateTime.Parse("1982-05-06 10:00:00");
        }

        public static string T2S(object obj)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                try
                {
                    return Convert.ToDateTime(obj).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch (Exception ex) { return string.Empty; }
            }
            return string.Empty;
        }

        public static string Db2S(object obj)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                try
                {
                    return Convert.ToDouble(obj).ToString();
                }
                catch (Exception ex) { return string.Empty; }
            }
            return string.Empty;
        }

        public static int O2I(object obj)
        {
            if (obj != null)
            {
                try
                {
                    return Convert.ToInt32(obj);
                }
                catch (Exception ex) { return 0; }
            }
            return 0;
        }

        public static double O2D(object obj)
        {
            if (obj != null)
            {
                try
                {
                    return Convert.ToDouble(obj);
                }
                catch (Exception ex) { return 0.0; }
            }
            return 0.0;
        }

        public static Dictionary<string, string> GetWaferFromSN(List<string> snlist)
        {
            var ret = new Dictionary<string, string>();
            var sncond = "('" + string.Join("','", snlist) + "')";
            var sql = @"select tco.ContainerName,dc.ParamValueString from [InsiteDB].[insite].[dc_IQC_InspectionResult] dc with(nolock)
                        inner join insitedb.insite.Historymainline hml  with(nolock) on hml.HistoryMainlineId = dc.historymainlineid 
                        inner join  InsiteDB.insite.container co (nolock) on co.containerid=hml.HistoryId
                        inner join insitedb.insite.Product p with(nolock) on p.ProductId  = co.ProductId
                        inner join insitedb.insite.IssueActualsHistory iah with(nolock) on iah.FromContainerId = co.ContainerId
                        inner join  InsiteDB.insite.container tco (nolock) on iah.ToContainerId = tco.ContainerId
                        where  tco.ContainerName in <sncond> and dc.[ParamValueString] like '%-%' and p.Description  like '%vcsel%' and dc.ParameterName = 'VendorLotNumber'";
            sql = sql.Replace("<sncond>", sncond);
            var dbret = DBUtility.ExeRealMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = UT.O2S(line[0]).ToUpper();
                var wf = UT.O2S(line[1]);
                var wafer = wf;
                var strs = wafer.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (strs[0].Length == 6 && wafer.Length > 8)
                { wafer = wafer.Substring(0, 9); }
                else if(strs[0].Length == 5 && wafer.Length > 12)
                { wafer = wafer.Substring(0, 13); }

                if (!ret.ContainsKey(sn))
                { ret.Add(sn, wafer); }
            }

            var leftsnlist = new List<string>();
            foreach (var sn in snlist)
            {
                if (!ret.ContainsKey(sn.Trim().ToUpper()))
                {
                    leftsnlist.Add(sn.Trim());
                }
            }

            if (leftsnlist.Count > 0)
            {
                sncond = "('" + string.Join("','", leftsnlist) + "')";
                sql = @"select tco.ContainerName,dc.ParamValueString
	                    from InsiteDB.insite.container fco (nolock) 
	                    inner join insitedb.insite.IssueActualsHistory iah with(nolock) on iah.FromContainerId = fco.ContainerId
	                    inner join  InsiteDB.insite.container tco (nolock) on iah.ToContainerId = tco.ContainerId
	                    inner join insitedb.insite.Product p  (nolock) on p.ProductId  = fco.ProductId
	                    inner join  InsiteDB.insite.container orgco (nolock) on orgco.ContainerName = fco.DateCode
	                    inner join insitedb.insite.Historymainline hml  with(nolock) on hml.HistoryId = orgco.ContainerId
	                    inner join InsiteDB.insite.dc_AOC_ManualInspection dc (nolock) on dc.historymainlineid = hml.historymainlineid
	                     where tco.ContainerName in <sncond> and p.description like '%VCSEL%' and dc.ParameterName = 'Trace_ID' and dc.ParamValueString is not null";
                sql = sql.Replace("<sncond>", sncond);
                dbret = DBUtility.ExeRealMESSqlWithRes(sql);
                foreach (var line in dbret)
                {
                    var sn = UT.O2S(line[0]).ToUpper();
                    var wf = UT.O2S(line[1]);
                    var wafer = wf;
                    var strs = wafer.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    if (strs[0].Length == 6 && wafer.Length > 8)
                    { wafer = wafer.Substring(0, 9); }
                    else if (strs[0].Length == 5 && wafer.Length > 12)
                    { wafer = wafer.Substring(0, 13); }

                    if (!ret.ContainsKey(sn))
                    { ret.Add(sn, wafer); }
                }
            }

            return ret;
        }

        //"~/Scripts/font_ogpsm5x1_450.pb"
        public static int CNN_GetVAL(Mat cmat,Net net)
        {
            var cmatcp = new Mat();
            cmat.CopyTo(cmatcp);
            Cv2.Resize(cmatcp, cmatcp, new Size(50, 50));
            Cv2.CvtColor(cmatcp, cmatcp, ColorConversionCodes.GRAY2RGB);
            cmatcp = cmatcp / 255.0;
            var fmat = new Mat();
            cmatcp.ConvertTo(fmat, MatType.CV_32F, 1.0);
            var blob = CvDnn.BlobFromImage(fmat, 1.0, new Size(50, 50), new Scalar(0, 0, 0), false, false);

            net.SetInput(blob);
            var ret = net.Forward();

            var retdump = ret.Dump();
            var clas = retdump.Split(new string[] { "[","]",","," "},StringSplitOptions.RemoveEmptyEntries);
            var idx = 0;
            var mxval = 0.0;
            var mxidx = -1;
            foreach (var c in clas)
            {
                var v = UT.O2D(c);
                if (v > mxval)
                {
                    mxval = v;
                    mxidx = idx;
                }
                idx++;
            }

            return (mxidx + 48);
        }

        public static Net GetNetByType(string caprev, Controller ctrl)
        {
            var obj = ctrl.HttpContext.Cache.Get(caprev + "_CNN");
            if (obj != null)
            { return (Net)obj; }

            var pbfile = "";
            if (string.Compare(caprev, "OGP-rect5x1", true) == 0)
            { pbfile = "~/Scripts/font_ogp5x1_5000.pb"; }
            else if (string.Compare(caprev, "OGP-rect2x1", true) == 0)
            { pbfile = "~/Scripts/font_ogp2x1_4500.pb"; }
            else if (string.Compare(caprev, "OGP-circle2168", true) == 0)
            { pbfile = "~/Scripts/font_ogp2168_850.pb"; }
            else if (string.Compare(caprev, "OGP-A10G", true) == 0)
            { pbfile = "~/Scripts/font_ogpa10g_750.pb"; }
            else if (string.Compare(caprev, "OGP-iivi", true) == 0)
            { pbfile = "~/Scripts/font_ogpiivi_480.pb"; }
            else if (string.Compare(caprev, "OGP-small5x1", true) == 0)
            { pbfile = "~/Scripts/font_ogpsm5x1_450.pb"; }
            else if (string.Compare(caprev, "OGP-sm-iivi", true) == 0)
            { pbfile = "~/Scripts/font_ogpsmiivi_600.pb"; }

            if (string.IsNullOrEmpty(pbfile))
            { pbfile = "~/Scripts/font_ogp5x1_5000.pb"; }

            var trainedNet = OpenCvSharp.Dnn.Net.ReadNetFromTensorflow(ctrl.Server.MapPath(pbfile));

            if (trainedNet != null)
            { ctrl.HttpContext.Cache.Insert(caprev + "_CNN", trainedNet, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration); }

            return trainedNet;
        }


    }
}