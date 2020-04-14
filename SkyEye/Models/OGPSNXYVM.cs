﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SkyEye.Models
{
    public class OGPSNXYVM
    {
        public static Dictionary<string, OGPSNXYVM> GetLocalOGPXYSNDict(string wafernum)
        {
            var sql = @"SELECT f.SN,s.ImgVal,s.ChildCat,s.ImgOrder,f.MainImgKey,f.CaptureImg,f.RAWImgURL,f.Appv_4,WaferNum FROM [WAT].[dbo].[OGPFatherImg] f with(nolock)
                        inner join [WAT].[dbo].[SonImg] s with (nolock) on f.MainImgKey = s.MainImgKey
                        where WaferNum like '<wafernum>%' order by SN,ImgOrder asc";
            sql = sql.Replace("<wafernum>", wafernum);

            var dict = new Dictionary<string, OGPSNXYVM>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = UT.O2S(line[0]);
                var k = UT.O2S(line[4])+":"+sn;

                var imgval = "";
                var ival = UT.O2I(line[1]);
                if (ival != -1)
                { imgval = Convert.ToString((char)ival); }

                var cat = UT.O2S(line[2]).ToUpper();
                if (dict.ContainsKey(k))
                {
                    if (cat.Contains("X"))
                    { dict[k].X += imgval; }
                    else
                    { dict[k].Y += imgval; }
                }
                else
                {
                    var tempvm = new OGPSNXYVM();
                    tempvm.MainImgKey = UT.O2S(line[4]);
                    tempvm.CaptureImg = UT.O2S(line[5]);
                    tempvm.SN = sn;
                    tempvm.RawImg = UT.O2S(line[6]);
                    tempvm.Modified = UT.O2S(line[7]);
                    tempvm.WaferNum = UT.O2S(line[8]);

                    if (cat.Contains("X"))
                    { tempvm.X += imgval; }
                    else
                    { tempvm.Y += imgval; }
                    dict.Add(k, tempvm);
                }
            }

            return dict;
        }

        public static Dictionary<string, OGPSNXYVM> GetLocalOGPXYSNDict(List<string> snlist)
        {
            var sb = new StringBuilder();
            foreach (var sn in snlist)
            { sb.Append("or f.SN like '" + sn + "%' "); }

            var sncond = "(" + sb.ToString().Substring(2) + ")";

            var sql = @"SELECT f.SN,s.ImgVal,s.ChildCat,s.ImgOrder,f.MainImgKey,f.CaptureImg,f.RAWImgURL,f.Appv_4,WaferNum FROM [WAT].[dbo].[OGPFatherImg] f with(nolock)
                        inner join [WAT].[dbo].[SonImg] s with (nolock) on f.MainImgKey = s.MainImgKey
                        where "+sncond+" order by SN,ImgOrder asc";

            var dict = new Dictionary<string, OGPSNXYVM>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = UT.O2S(line[0]);
                var k = UT.O2S(line[4]) + ":" + sn;

                var imgval = "";
                var ival = UT.O2I(line[1]);
                if (ival != -1)
                { imgval = Convert.ToString((char)ival); }

                var cat = UT.O2S(line[2]).ToUpper();
                if (dict.ContainsKey(k))
                {
                    if (cat.Contains("X"))
                    { dict[k].X += imgval; }
                    else
                    { dict[k].Y += imgval; }
                }
                else
                {
                    var tempvm = new OGPSNXYVM();
                    tempvm.MainImgKey = UT.O2S(line[4]);
                    tempvm.CaptureImg = UT.O2S(line[5]);
                    tempvm.SN = sn;
                    tempvm.RawImg = UT.O2S(line[6]);
                    tempvm.Modified = UT.O2S(line[7]);
                    tempvm.WaferNum = UT.O2S(line[8]).ToUpper();

                    if (cat.Contains("X"))
                    { tempvm.X += imgval; }
                    else
                    { tempvm.Y += imgval; }
                    dict.Add(k, tempvm);
                }
            }

            return dict;
        }

        public static Dictionary<string, OGPSNXYVM> GetLocalOGPXYMKDict(string wafernum)
        {
            var sql = @"SELECT f.SN,s.ImgVal,s.ChildCat,s.ImgOrder,f.MainImgKey FROM [WAT].[dbo].[OGPFatherImg] f with(nolock)
                        inner join [WAT].[dbo].[SonImg] s with (nolock) on f.MainImgKey = s.MainImgKey
                        where WaferNum = '<wafernum>' order by SN,ImgOrder asc";
            sql = sql.Replace("<wafernum>", wafernum);

            var dict = new Dictionary<string, OGPSNXYVM>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                //var sn = UT.O2S(line[0]);

                var imgval = "";
                var ival = UT.O2I(line[1]);
                if (ival != -1)
                { imgval = Convert.ToString((char)ival); }

                var cat = UT.O2S(line[2]).ToUpper();
                var mk = UT.O2S(line[4]);

                if (dict.ContainsKey(mk))
                {
                    if (cat.Contains("X"))
                    { dict[mk].X += imgval; }
                    else
                    { dict[mk].Y += imgval; }
                }
                else
                {
                    var tempvm = new OGPSNXYVM();

                    if (cat.Contains("X"))
                    { tempvm.X += imgval; }
                    else
                    { tempvm.Y += imgval; }
                    dict.Add(mk, tempvm);
                }
            }

            return dict;
        }


        public static List<OGPSNXYVM> GetMEOGPXY(string wafernum)
        {
            var ret = new List<OGPSNXYVM>();

            var sql = @"select  [SN]+':::'+[Index],X,Y from [AIProjects].[dbo].[CouponData] where SN like '<wafernum>%'";
            sql = sql.Replace("<wafernum>", wafernum);
            var dbret = DBUtility.ExeOGPSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new OGPSNXYVM();
                tempvm.SN = UT.O2S(line[0]);
                tempvm.MX = UT.O2S(line[1]).ToUpper();
                tempvm.MY = UT.O2S(line[2]).ToUpper();
                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<OGPSNXYVM> GetConbineXY(string wafernum)
        {
            var localxy = GetLocalOGPXYSNDict(wafernum);
            //var mexy = GetMEOGPXY(wafernum);
            //foreach (var m in mexy)
            //{
            //    if (localxy.ContainsKey(m.SN))
            //    {
            //        localxy[m.SN].MX = m.MX;
            //        localxy[m.SN].MY = m.MY;
            //    }
            //}

            var templist = localxy.Values.ToList();
            var unmatchlist = new List<OGPSNXYVM>();
            var matchlist = new List<OGPSNXYVM>();
            foreach (var item in templist)
            {
                if (!string.IsNullOrEmpty(item.Checked))
                { matchlist.Add(item); }
                else
                { unmatchlist.Add(item); }
            }
            unmatchlist.AddRange(matchlist);

            return unmatchlist;
        }

        public OGPSNXYVM()
        {
            MainImgKey = "";
            CaptureImg = "";
            RawImg = "";
            SN = "";
            X = "";
            Y = "";
            MX = "";
            MY = "";
            Modified = "";
            WaferNum = "";
            Product = "";
        }

        public string MainImgKey { set; get; }
        public string CaptureImg { set; get; }
        public string RawImg { set; get; }
        public string SN { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
        public string MX { set; get; }
        public string MY { set; get; }
        public string Checked {
            get {
                if (X.ToUpper().Contains(MX) && Y.ToUpper().Contains(MY) 
                    && !string.IsNullOrEmpty(MX) && !string.IsNullOrEmpty(MY)
                    && X.ToUpper().Substring(0,1).Contains("X")
                    && Y.ToUpper().Substring(0, 1).Contains("Y"))
                { return "CHECKED"; }
                else
                { return ""; }
            }
        }
        public string Modified { set; get; }
        public string WaferNum { set; get; }
        public string Product { set; get; }
    }
}