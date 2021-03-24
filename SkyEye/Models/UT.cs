using OpenCvSharp;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        public static string CNN_SM5X1(Mat cmat,string pbfile)
        {
            var cmatcp = new Mat();
            cmat.CopyTo(cmatcp);
            var net = OpenCvSharp.Dnn.Net.ReadNetFromTensorflow(pbfile);
            Cv2.Resize(cmatcp, cmatcp, new Size(50, 50));
            Cv2.CvtColor(cmatcp, cmatcp, ColorConversionCodes.GRAY2RGB);
            cmatcp = cmatcp / 255.0;
            var fmat = new Mat();
            cmatcp.ConvertTo(fmat, MatType.CV_32F, 1.0);
            var blob = CvDnn.BlobFromImage(fmat, 1.0, new Size(50, 50), new Scalar(0, 0, 0), false, false);

            net.SetInput(blob);
            var ret = net.Forward();
            var retdump = ret.Dump();
            return retdump;
        }


    }
}