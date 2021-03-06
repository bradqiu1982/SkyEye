﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenCvSharp;
using System.Web.Mvc;
using System.IO;
using OpenCvSharp.Dnn;

namespace SkyEye.Models
{

    public class OGPFatherImg
    {

        public static string LoadImg(string imgpath,string wafer,Dictionary<string,string> snmap
            , Dictionary<string, bool> probexymap,ImageTypeDetect caprev, Controller ctrl,OpenCvSharp.ML.KNearest onemode,Net AIFontMode = null, bool fixangle = false,bool newalg = true)
        {
            try
            {
                if (caprev.ModelName.Contains("OGP-iivi"))
                {
                    var charmatlist = ImgOperateIIVI.CutCharRect(imgpath,caprev, 115, 140,fixangle);
                    if (charmatlist.Count > 0)
                    {

                        {
                            return SolveImg(imgpath, wafer, charmatlist, caprev, snmap, probexymap, ctrl, onemode, AIFontMode);
                        }
                    }
                    //else
                    //{
                    //    Mat rawimg1 = Cv2.ImRead(imgpath, ImreadModes.Color);
                    //    var fimg1 = new OGPFatherImg();
                    //    fimg1.WaferNum = wafer;
                    //    fimg1.SN = Path.GetFileNameWithoutExtension(imgpath);
                    //    fimg1.MainImgKey = GetUniqKey();
                    //    fimg1.RAWImgURL = WriteRawImg(rawimg1, fimg1.MainImgKey, ctrl);
                    //    fimg1.CaptureImg = "";
                    //    fimg1.CaptureRev = caprev;
                    //    fimg1.MUpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    //    fimg1.StoreFailData();
                    //    return string.Empty;
                    //}
                }
                else if (caprev.ModelName.Contains("OGP-small5x1"))
                {
                    var charmatlist = ImgOperateSmall5x1.CutCharRect(imgpath,caprev, 18, 35, 4.5, 8, 5500,50);
                    if (charmatlist.Count > 0)
                    {

                        {
                            return SolveImg(imgpath, wafer, charmatlist, caprev, snmap, probexymap, ctrl, onemode, AIFontMode);
                        }
                    }
                }
                else if (caprev.ModelName.Contains("OGP-rect5x1"))
                {
                    var turn = false;
                    var xyrectlist = ImgOperate5x1.FindXYRect(imgpath, 25, 43, 4.5, 6.8, 8000,false, out turn, fixangle);
                    if (xyrectlist.Count > 0)
                    {
                        var charmatlist = ImgOperate5x1.CutCharRect(imgpath,caprev, xyrectlist[0], 50, 90, 40, 67, fixangle,newalg);
                        if (charmatlist.Count > 0)
                        {

                            {
                                return SolveImg(imgpath, wafer, charmatlist, caprev, snmap, probexymap, ctrl, onemode, AIFontMode);
                            }
                        }
                        else
                        {
                            charmatlist = ImgOperate5x1.CutBadCharRect(imgpath, xyrectlist[0], 50, 90, 40, 67);
                            if (charmatlist.Count == 9)
                            { return SolveUnrecognizeImg(imgpath, wafer, charmatlist, caprev.ModelName, ctrl); }
                        }
                    }
                }
                else if (caprev.ModelName.Contains("OGP-rect2x1"))
                {
                    var xyrectlist = ImgOperate2x1.FindXYRect(imgpath, 60, 100, 2.0, 3.0);
                    if (xyrectlist.Count > 0)
                    {
                        var charmatlist = ImgOperate2x1.CutCharRect(imgpath,caprev, xyrectlist[0], 40, 56, 65);
                        if (charmatlist.Count > 0)
                        {

                            {
                                return SolveImg(imgpath, wafer, charmatlist, caprev, snmap, probexymap, ctrl, onemode, AIFontMode);
                            }
                        }
                    }
                }
                else if (caprev.ModelName.Contains("OGP-circle2168"))
                {
                    var charmatlist = ImgOperateCircle2168.CutCharRect(imgpath,caprev,fixangle);
                    if (charmatlist.Count > 0)
                    {

                        {
                            return SolveImg(imgpath, wafer, charmatlist, caprev, snmap, probexymap, ctrl, onemode, AIFontMode);
                        }
                    }
                }
                else if (caprev.ModelName.Contains("OGP-A10G"))
                {
                    var charmatlist = ImgOperateA10G.CutCharRect(imgpath,caprev);
                    if (charmatlist.Count > 0)
                    {

                        {
                            return SolveImg(imgpath, wafer, charmatlist, caprev, snmap, probexymap, ctrl, onemode, AIFontMode);
                        }
                    }
                }
                else if (caprev.ModelName.Contains("OGP-sm-iivi"))
                {
                    var charmatlist = ImgOperateIIVIsm.CutCharRect(imgpath,caprev, 80, 115);
                    if (charmatlist.Count > 0)
                    {

                        {
                            return SolveImg(imgpath, wafer, charmatlist, caprev, snmap, probexymap, ctrl, onemode, AIFontMode);
                        }
                    }
                }
            }
            catch (Exception ex) { }

            Mat rawimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var fimg = new OGPFatherImg();
            fimg.WaferNum = wafer;
            fimg.SN = Path.GetFileNameWithoutExtension(imgpath);
            fimg.MainImgKey = GetUniqKey();
            fimg.RAWImgURL = WriteRawImg(rawimg, fimg.MainImgKey, ctrl,imgpath);
            fimg.CaptureImg = "";
            fimg.CaptureRev = caprev.ModelName;
            fimg.MUpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            fimg.StoreFailData();

            return string.Empty;
        }

        public static string Load200xImg(string imgpath, string wafer, ImageTypeDetect caprev, Controller ctrl, OpenCvSharp.ML.KNearest kmode)
        {
            try
            {
                if (caprev.ModelName.Contains("OGP-small5x1"))
                {
                    var charmatlist = ImgOperateSmall5x1.CutCharRect(imgpath,caprev, 18, 35, 4.5, 8, 5500, 50);
                    if (charmatlist.Count > 0)
                    {

                        {
                            return Solve200xImg(imgpath, wafer, charmatlist, caprev, ctrl, kmode);
                        }
                    }
                }
                else if (caprev.ModelName.Contains("OGP-sm-iivi"))
                {
                    var charmatlist = ImgOperateIIVIsm.CutCharRect(imgpath,caprev, 80, 115);
                    if (charmatlist.Count > 0)
                    {

                        {
                            return Solve200xImg(imgpath, wafer, charmatlist, caprev, ctrl, kmode);
                        }
                    }
                }
                else if (caprev.ModelName.Contains("OGP-iivi"))
                {
                    var charmatlist = ImgOperateIIVI.CutCharRect(imgpath,caprev, 115, 140, caprev.ImgTurn);
                    if (charmatlist.Count > 0)
                    {

                        {
                            return Solve200xImg(imgpath, wafer, charmatlist, caprev, ctrl, kmode);
                        }
                    }
                }
                else if (caprev.ModelName.Contains("OGP-rect5x1"))
                {
                    var turn = false;
                    var xyrectlist = ImgOperate5x1.FindXYRect(imgpath, 25, 43, 4.5, 6.8, 8000, false, out turn, false);
                    if (xyrectlist.Count > 0)
                    {
                        var charmatlist = ImgOperate5x1.CutCharRect(imgpath,caprev, xyrectlist[0], 50, 90, 40, 67, false, true);
                        if (charmatlist.Count > 0)
                        {

                            {
                                return Solve200xImg(imgpath, wafer, charmatlist, caprev , ctrl, kmode);
                            }
                        }
                    }
                }
                //else if (caprev.Contains("OGP-rect2x1"))
                //{
                //    var xyrectlist = ImgOperate2x1.FindXYRect(imgpath, 60, 100, 2.0, 3.0);
                //    if (xyrectlist.Count > 0)
                //    {
                //        var charmatlist = ImgOperate2x1.CutCharRect(imgpath, xyrectlist[0], 40, 56);
                //        if (charmatlist.Count > 0)
                //        {
                //            using (var kmode = KMode.GetTrainedMode(caprev, ctrl))
                //            {
                //                return SolveImg(imgpath, wafer, charmatlist, caprev, snmap, probexymap, ctrl, kmode);
                //            }
                //        }
                //    }
                //}
                //else if (caprev.Contains("OGP-circle2168"))
                //{
                //    var charmatlist = ImgOperateCircle2168.CutCharRect(imgpath, 38, 64, 50, 100, 1.85, 2.7, 3.56, 40, 60);
                //    if (charmatlist.Count > 0)
                //    {
                //        using (var kmode = KMode.GetTrainedMode(caprev, ctrl))
                //        {
                //            return SolveImg(imgpath, wafer, charmatlist, caprev, snmap, probexymap, ctrl, kmode);
                //        }
                //    }
                //}
            }
            catch (Exception ex) { }

            Mat rawimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var fimg = new OGPFatherImg();
            fimg.WaferNum = wafer;
            fimg.SN = Path.GetFileNameWithoutExtension(imgpath);
            fimg.MainImgKey = GetUniqKey();
            fimg.RAWImgURL = WriteRawImg(rawimg, fimg.MainImgKey, ctrl, imgpath);
            fimg.CaptureImg = "";
            fimg.CaptureRev = caprev.ModelName;
            fimg.MUpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            fimg.StoreFailData();

            return string.Empty;
        }

        private static string SolveImg(string imgpath,string wafer, List<Mat> charmatlist, ImageTypeDetect caprev
            , Dictionary<string, string> snmap, Dictionary<string, bool> probexymap, Controller ctrl, OpenCvSharp.ML.KNearest kmode, Net AIFontModel)
        {
            var ret = "";
            var ratelist = new List<double>();
            ratelist.Add(70);
            ratelist.Add(10); ratelist.Add(10);
            ratelist.Add(3); ratelist.Add(3);
            ratelist.Add(2); ratelist.Add(2);

            Mat rawimg = Cv2.ImRead(imgpath, ImreadModes.Color);

            var fimg = new OGPFatherImg();
            fimg.WaferNum = wafer;
            fimg.SN = Path.GetFileNameWithoutExtension(imgpath);
            fimg.FileName = fimg.SN;
            if (snmap.ContainsKey(fimg.SN))
            { fimg.SN = snmap[fimg.SN]; }

            fimg.MainImgKey = GetUniqKey();
            fimg.RAWImgURL = WriteRawImg(rawimg, fimg.MainImgKey, ctrl, imgpath);
            fimg.CaptureImg = Convert.ToBase64String(charmatlist[0].ToBytes());
            fimg.CaptureRev = caprev.ModelName;
            fimg.MUpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            ret = fimg.MainImgKey;

            var xstr = "";
            var ystr = "";
            var idx = 0;
            var midx = (charmatlist.Count - 1) / 2 + 1;

            foreach (var sm in charmatlist)
            {
                if (idx == 0)
                { idx++; continue; }

                var tcm = new Mat();
                sm.ConvertTo(tcm, MatType.CV_32FC1);
                var tcmresize = new Mat();
                Cv2.Resize(tcm, tcmresize, new Size(50, 50), 0, 0, InterpolationFlags.Linear);

                var sonimg = new SonImg();
                sonimg.MainImgKey = fimg.MainImgKey;
                sonimg.ChildImgKey = GetUniqKey();
                sonimg.ChildImg = Convert.ToBase64String(tcmresize.ToBytes());
                sonimg.ImgOrder = idx;
                sonimg.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var stcm = tcmresize.Reshape(1, 1);
                var resultmat = new Mat();

                if (idx == 1)
                {
                    sonimg.ImgVal = (int)Convert.ToChar("X");
                    sonimg.Rate = "100";
                }
                else if (idx == 5)
                {
                    sonimg.ImgVal = (int)Convert.ToChar("Y");
                    sonimg.Rate = "100";
                }
                else
                {
                    if (AIFontModel != null)
                    {
                        //var chimg = Mat.ImDecode(Convert.FromBase64String(sonimg.ChildImg), ImreadModes.Grayscale);
                        var outrate = 0.0;
                        var aiimgval = ImgFontCNN.CNN_GetCharacterVAL(sm, AIFontModel, out outrate);
                        //if (aiimgval != -1)
                        //{
                            sonimg.ImgVal = aiimgval;
                            sonimg.Rate = outrate.ToString();
                        //}
                    }
                    else
                    {
                        var imgval = kmode.FindNearest(stcm, 1, resultmat);
                        if (imgval > 0)
                        { sonimg.ImgVal = (int)imgval; }

                        var rate = 0.0;
                        var matched = new Mat();
                        kmode.FindNearest(stcm, 7, resultmat, matched);
                        var matchstr = matched.Dump();
                        var ms = matchstr.Split(new string[] { "[", "]", "," }, StringSplitOptions.RemoveEmptyEntries);
                        var msidx = 0;
                        foreach (var m in ms)
                        {
                            if (string.Compare(m.Trim(), imgval.ToString()) == 0)
                            {
                                rate += ratelist[msidx];
                                if (rate >= 93 && msidx == 3)
                                {
                                    rate = 100; break;
                                }
                            }
                            msidx++;
                        }
                        sonimg.Rate = rate.ToString();
                    }
                }

                if (idx < midx)
                {
                    sonimg.ChildCat = "X";
                    if (sonimg.ImgVal > 0)
                    { xstr += Convert.ToString((char)sonimg.ImgVal); }
                }
                else
                {
                    sonimg.ChildCat = "Y";
                    if (sonimg.ImgVal > 0)
                    { ystr += Convert.ToString((char)sonimg.ImgVal); }
                }
                sonimg.StoreData();
                idx++;
            }

            var xykey =UT.O2S(UT.O2I(xstr.Replace("X", "")))+":::"+ UT.O2S(UT.O2I(ystr.Replace("Y", "")));
            if (probexymap.ContainsKey(xykey))
            { fimg.ProbeChecked = "CHECKED"; }

            fimg.StoreData();

            return ret;
        }

        private static string Solve200xImg(string imgpath, string wafer, List<Mat> charmatlist, ImageTypeDetect caprev
            , Controller ctrl, OpenCvSharp.ML.KNearest kmode)
        {
            var ret = "";
            var ratelist = new List<double>();
            ratelist.Add(70);
            ratelist.Add(10); ratelist.Add(10);
            ratelist.Add(3); ratelist.Add(3);
            ratelist.Add(2); ratelist.Add(2);

            Mat rawimg = Cv2.ImRead(imgpath, ImreadModes.Color);

            var fimg = new OGPFatherImg();
            fimg.WaferNum = wafer;
            var sns = Path.GetFileNameWithoutExtension(imgpath).Split(new string[] { "_", "." },StringSplitOptions.RemoveEmptyEntries);
            fimg.SN = sns[0];
            if (sns.Length > 2)
            { fimg.SN += "_"+sns[1]; }
            fimg.FileName = fimg.SN;
            fimg.MainImgKey = GetUniqKey();
            fimg.RAWImgURL = WriteRawImg(rawimg, fimg.MainImgKey, ctrl, imgpath);
            fimg.CaptureImg = Convert.ToBase64String(charmatlist[0].ToBytes());
            fimg.CaptureRev = caprev.ModelName;
            fimg.MUpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            ret = fimg.MainImgKey;

            var idx = 0;
            var midx = (charmatlist.Count - 1) / 2 + 1;

            foreach (var sm in charmatlist)
            {
                if (idx == 0)
                { idx++; continue; }

                var tcm = new Mat();
                sm.ConvertTo(tcm, MatType.CV_32FC1);
                var tcmresize = new Mat();
                Cv2.Resize(tcm, tcmresize, new Size(50, 50), 0, 0, InterpolationFlags.Linear);

                var sonimg = new SonImg();
                sonimg.MainImgKey = fimg.MainImgKey;
                sonimg.ChildImgKey = GetUniqKey();
                sonimg.ChildImg = Convert.ToBase64String(tcmresize.ToBytes());
                sonimg.ImgOrder = idx;
                sonimg.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var stcm = tcmresize.Reshape(1, 1);
                var resultmat = new Mat();

                if (idx == 1)
                {
                    sonimg.ImgVal = (int)Convert.ToChar("X");
                    sonimg.Rate = "100";
                }
                else if (idx == 5)
                {
                    sonimg.ImgVal = (int)Convert.ToChar("Y");
                    sonimg.Rate = "100";
                }
                else
                {
                    var imgval = kmode.FindNearest(stcm, 1, resultmat);
                    if (imgval > 0)
                    { sonimg.ImgVal = (int)imgval; }

                    var rate = 0.0;
                    var matched = new Mat();
                    kmode.FindNearest(stcm, 7, resultmat, matched);
                    var matchstr = matched.Dump();
                    var ms = matchstr.Split(new string[] { "[", "]", "," }, StringSplitOptions.RemoveEmptyEntries);
                    var msidx = 0;
                    foreach (var m in ms)
                    {
                        if (string.Compare(m.Trim(), imgval.ToString()) == 0)
                        {
                            rate += ratelist[msidx];
                            if (rate == 93 && msidx == 3)
                            {
                                rate = 100; break;
                            }
                        }
                        msidx++;
                    }
                    sonimg.Rate = rate.ToString();
                }

                if (idx < midx)
                { sonimg.ChildCat = "X"; }
                else
                { sonimg.ChildCat = "Y"; }
                sonimg.StoreData();
                idx++;
            }

            fimg.StoreData();

            return ret;
        }

        private static string SolveUnrecognizeImg(string imgpath, string wafer, List<Mat> charmatlist, string caprev , Controller ctrl)
        {
            var ret = "";

            Mat rawimg = Cv2.ImRead(imgpath, ImreadModes.Color);

            var fimg = new OGPFatherImg();
            fimg.WaferNum = wafer;
            fimg.SN = Path.GetFileNameWithoutExtension(imgpath);
            fimg.FileName = fimg.SN;

            fimg.MainImgKey = GetUniqKey();
            fimg.RAWImgURL = WriteRawImg(rawimg, fimg.MainImgKey, ctrl, imgpath);

            fimg.CaptureImg = Convert.ToBase64String(charmatlist[0].ToBytes());
            fimg.CaptureRev = caprev;
            fimg.MUpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            ret = fimg.MainImgKey;

            var xstr = "";
            var ystr = "";
            var idx = 0;
            foreach (var sm in charmatlist)
            {
                if (idx == 0)
                { idx++; continue; }

                var tcm = new Mat();
                sm.ConvertTo(tcm, MatType.CV_32FC1);
                var tcmresize = new Mat();
                Cv2.Resize(tcm, tcmresize, new Size(50, 50), 0, 0, InterpolationFlags.Linear);

                var sonimg = new SonImg();
                sonimg.MainImgKey = fimg.MainImgKey;
                sonimg.ChildImgKey = GetUniqKey();
                sonimg.ChildImg = Convert.ToBase64String(tcmresize.ToBytes());
                sonimg.ImgOrder = idx;
                sonimg.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var stcm = tcmresize.Reshape(1, 1);
                var resultmat = new Mat();
                //var imgval = kmode.FindNearest(stcm, 1, resultmat);
                //if (imgval > 0)
                //{ sonimg.ImgVal = (int)imgval; }

                if (idx < 5)
                {
                    sonimg.ChildCat = "X";
                    sonimg.ImgVal = 88;
                    if (sonimg.ImgVal > 0)
                    { xstr += Convert.ToString((char)sonimg.ImgVal); }
                }
                else
                {
                    sonimg.ChildCat = "Y";
                    sonimg.ImgVal = 89;
                    if (sonimg.ImgVal > 0)
                    { ystr += Convert.ToString((char)sonimg.ImgVal); }
                }
                sonimg.StoreData();
                idx++;
            }

            //var xykey = UT.O2S(UT.O2I(xstr.Replace("X", ""))) + ":::" + UT.O2S(UT.O2I(ystr.Replace("Y", "")));
            //if (probexymap.ContainsKey(xykey))
            //{ fimg.ProbeChecked = "CHECKED"; }

            fimg.StoreUnrecognizeData();

            return ret;
        }


        private static string WriteRawImg(Mat rawimg, string mk, Controller ctrl,string imgpath)
        {
            try
            {
                var fn = mk + ".png";
                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\images\\" + datestring + "\\";
                if (!Directory.Exists(imgdir))
                { Directory.CreateDirectory(imgdir); }
                var wholefn = imgdir + fn;
                var bts = rawimg.ToBytes();
                File.WriteAllBytes(wholefn, bts);
                var url = "/userfiles/images/" + datestring + "/" + fn;
                //var uri = new Uri(imgpath);
                //var url = uri.AbsoluteUri.Replace("//"+uri.Host,"/////"+uri.Host);
                return url;
            }
            catch (Exception ex) { }

            return string.Empty;
        }

        public static string GetUniqKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public void StoreData()
        {
            var sql = @"insert into WAT.dbo.OGPFatherImg(WaferNum,SN,MainImgKey,RAWImgURL,CaptureImg,CaptureRev,MUpdateTime,Appv_1,Appv_3) 
                    values(@WaferNum,@SN,@MainImgKey,@RAWImgURL,@CaptureImg,@CaptureRev,@MUpdateTime,@ProbeChecked,@FileName)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", WaferNum);
            dict.Add("@SN", SN);
            dict.Add("@MainImgKey", MainImgKey);
            dict.Add("@RAWImgURL", RAWImgURL);
            dict.Add("@CaptureImg", CaptureImg);
            dict.Add("@CaptureRev", CaptureRev);
            dict.Add("@MUpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@ProbeChecked", ProbeChecked);
            dict.Add("@FileName", FileName);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public void StoreUnrecognizeData()
        {
            var sql = @"insert into WAT.dbo.OGPFatherImg(WaferNum,SN,MainImgKey,RAWImgURL,CaptureImg,CaptureRev,MUpdateTime,Appv_1,Appv_2,Appv_3) 
                    values(@WaferNum,@SN,@MainImgKey,@RAWImgURL,@CaptureImg,@CaptureRev,@MUpdateTime,@ProbeChecked,'U',@FileName)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", WaferNum);
            dict.Add("@SN", SN);
            dict.Add("@MainImgKey", MainImgKey);
            dict.Add("@RAWImgURL", RAWImgURL);
            dict.Add("@CaptureImg", CaptureImg);
            dict.Add("@CaptureRev", CaptureRev);
            dict.Add("@MUpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@ProbeChecked", ProbeChecked);
            dict.Add("@FileName", FileName);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public void StoreFailData()
        {
            var sql = @"insert into WAT.dbo.FailAnalyzeImg(WaferNum,SN,MainImgKey,RAWImgURL,CaptureImg,CaptureRev,MUpdateTime) 
                    values(@WaferNum,@SN,@MainImgKey,@RAWImgURL,@CaptureImg,@CaptureRev,@MUpdateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", WaferNum);
            dict.Add("@SN", SN);
            dict.Add("@MainImgKey", MainImgKey);
            dict.Add("@RAWImgURL", RAWImgURL);
            dict.Add("@CaptureImg", CaptureImg);
            dict.Add("@CaptureRev", CaptureRev);
            dict.Add("@MUpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static List<string> GetCaptureRevList()
        {
            var ret = new List<string>();
            var sql = "select distinct CaptureRev from WAT.dbo.OGPFatherImg";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                ret.Add(UT.O2S(line[0]));
            }
            return ret;
        }

        public static string GetCaptureImg(string key)
        {
            var ret = "";
            var sql = "select CaptureImg from WAT.dbo.OGPFatherImg where MainImgKey=@MainImgKey";
            var dict = new Dictionary<string, string>();
            dict.Add("@MainImgKey", key);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                return UT.O2S(line[0]);
            }
            return ret;
        }

      
        public static List<object> NewUnTrainedImg(List<string> imgkeys,string wafer)
        {
            var xydict = OGPSNXYVM.GetLocalOGPXYMKDict(wafer);

            var ret = new List<object>();
            var keycond = "('" + string.Join("','", imgkeys) + "')";
            var sql = @"select  f.CaptureImg,f.RAWImgURL,s.ChildImg,s.ImgOrder,s.ChildImgKey,s.ImgVal,f.Appv_1,s.MainImgKey from [WAT].[dbo].[SonImg] (nolock) s
                      inner join [WAT].[dbo].[OGPFatherImg] (nolock) f on f.MainImgKey = s.MainImgKey
                      where s.MainImgKey in <keycond> order by s.MainImgKey,s.ImgOrder asc";
            sql = sql.Replace("<keycond>", keycond);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var imgval = "";
                var ival = UT.O2I(line[5]);
                if (ival != -1)
                { imgval = Convert.ToString((char)ival); }

                var mk = UT.O2S(line[7]);
                var xcoord = "";
                var ycoord = "";
                var cfdlevel = 2;
                if (xydict.ContainsKey(mk))
                {
                    xcoord = xydict[mk].X;
                    ycoord = xydict[mk].Y;
                    cfdlevel = xydict[mk].CFDLevel;
                }

                ret.Add(new
                {
                    capimg = UT.O2S(line[0]),
                    rawurl = UT.O2S(line[1]),
                    chimg = UT.O2S(line[2]),
                    chidx = UT.O2S(line[3]),
                    cimgkey = UT.O2S(line[4]),
                    cimgval = imgval,
                    pchecked = UT.O2S(line[6]),
                    xcoord = xcoord,
                    ycoord = ycoord,
                    cfdlevel = cfdlevel
                });
            }
            return ret;
        }

        public static List<object> ExistTrainedImg(string wafernum)
        {
            var xydict = OGPSNXYVM.GetLocalOGPXYMKDict(wafernum);

            var ret = new List<object>();
            var sql = @"select  f.CaptureImg,f.RAWImgURL,s.ChildImg,s.ImgOrder,s.ChildImgKey,s.ImgVal,f.Appv_1,s.MainImgKey from [WAT].[dbo].[SonImg] (nolock) s
                      inner join [WAT].[dbo].[OGPFatherImg] (nolock) f on f.MainImgKey = s.MainImgKey
                      where f.WaferNum = @wafernum order by s.MainImgKey,s.ImgOrder asc";
            var dict = new Dictionary<string, string>();
            dict.Add("@wafernum", wafernum);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,dict);
            foreach (var line in dbret)
            {
                var imgval = "";
                var ival = UT.O2I(line[5]);
                if (ival != -1)
                { imgval = Convert.ToString((char)ival); }

                var mk = UT.O2S(line[7]);
                var xcoord = "";
                var ycoord = "";
                var cfdlevel = 2;
                if (xydict.ContainsKey(mk))
                {
                    xcoord = xydict[mk].X;
                    ycoord = xydict[mk].Y;
                    cfdlevel = xydict[mk].CFDLevel;
                }

                ret.Add(new
                {
                    capimg = UT.O2S(line[0]),
                    rawurl = UT.O2S(line[1]),
                    chimg = UT.O2S(line[2]),
                    chidx = UT.O2S(line[3]),
                    cimgkey = UT.O2S(line[4]),
                    cimgval = imgval,
                    pchecked = UT.O2S(line[6]),
                    xcoord = xcoord,
                    ycoord = ycoord,
                    cfdlevel = cfdlevel
                });
            }
            return ret;
        }

        public static void UpdateCheckedImgVal(string childimgkey, int imgval)
        {
            SonImg.UpdateImgVal(childimgkey, imgval);
            UpdateTrainningData(childimgkey, imgval);
        }

        private static void UpdateTrainningData(string childimgkey,int imgval)
        {
            var sql = @"select s.ChildImgKey,s.ChildImg,f.CaptureRev,f.WaferNum from [WAT].[dbo].[SonImg] (nolock) s
                      inner join [WAT].[dbo].[OGPFatherImg] (nolock) f on f.MainImgKey = s.MainImgKey
                      where s.ChildImgKey = @ChildImgKey order by s.MainImgKey,s.ImgOrder asc";

            var dict = new Dictionary<string, string>();
            dict.Add("@ChildImgKey", childimgkey);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var aidata = new AITrainingData();
                aidata.ImgKey = UT.O2S(line[0]);
                aidata.TrainingImg = UT.O2S(line[1]);
                aidata.ImgVal = imgval;
                aidata.Revision = UT.O2S(line[2]);
                aidata.UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                aidata.WaferNum = UT.O2S(line[3]);
                aidata.StoreData();
            }
        }

        public static void CleanWaferData(string wafernum)
        {
            SonImg.CleanData(wafernum);
            CleanData(wafernum);
        }

        private static void CleanData(string wafernum) {
            var sql = @"delete from [WAT].[dbo].[OGPFatherImg] where WaferNum = @WaferNum";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", wafernum);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static List<object> GetSNFileData(string wafer)
        {
            var ret = new List<object>();

            var sql = "select distinct SN,Appv_3 from [WAT].[dbo].[OGPFatherImg] where WaferNum = @wafer order by SN";
            var dict = new Dictionary<string, string>();
            dict.Add("@wafer", wafer);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var sn = UT.O2S(line[0]);
                var file = UT.O2S(line[1]);
                ret.Add(new
                {
                    sn = sn,
                    file = file
                });
            }

            return ret;
        }

        public static void UpdateModification(string MainImgKey)
        {
            var sql = "update [WAT].[dbo].[OGPFatherImg] set Appv_4 = 'UPDATED' where MainImgKey = @MainImgKey";
            var dict = new Dictionary<string, string>();
            dict.Add("@MainImgKey", MainImgKey);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void UpdateSN_(string die, string sn, string wafer)
        {
            var sql = "update [WAT].[dbo].[OGPFatherImg] set SN = @SN where WaferNum = '" + wafer + "' and Appv_3 = @die";
            var dict = new Dictionary<string, string>();
            dict.Add("@SN", sn);
            dict.Add("@die", die);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void Update1x2SN(List<string> snlist, string wafer, int totaldies = 176)
        {
            //snlist.Add("191812-30E0601");

            var snidx = 0;

            for (var idx = 0; idx < totaldies; idx++)
            {
                var midx = idx % 16 + 1;
                var sn = snlist[snidx] + ":::" + midx;
                var die = "Die-" + (idx + 1);

                UpdateSN_(die, sn, wafer);

                if (midx == 16)
                { snidx++; }
            }
        }

        public static void Update1x4SN(List<string> snlist, string wafer, int totaldies = 160)
        {
            //snlist.Add("191812-30E0601");

            var snidx = 0;

            for (var idx = 0; idx < totaldies; idx++)
            {
                var midx = idx % 8 + 1;
                var sn = snlist[snidx] + ":::" + midx;
                var die = "Die-" + (idx + 1);

                UpdateSN_(die, sn, wafer);

                if (midx == 8)
                { snidx++; }
            }
        }

        public static void Update1x12SN(List<string> snlist, string wafer, int totaldies = 104)
        {
            //snlist.Add("192406-50R1052");
            var snidx = 0;

            for (var idx = 0; idx < totaldies; idx++)
            {
                var midx = idx % 2 + 1;
                var sn = snlist[snidx] + ":::" + midx;
                var die = "Die-" + (idx + 1);

                UpdateSN_(die, sn, wafer);

                if (midx == 2)
                { snidx++; }
            }
        }

        public static void Update1x1SN(List<string> snlist, string wafer,int totaldies = 480)
        {
            //snlist.Add("61940-277-040E0817");

            var snidx = 0;
            for (var idx = 0; idx < totaldies; idx++)
            {
                var midx = idx % 32 + 1;
                var sn = snlist[snidx] + ":::" + midx;
                var die = "Die-" + (idx + 1);

                UpdateSN_(die, sn, wafer);

                if (midx == 32)
                { snidx++; }
            }
        }

        public OGPFatherImg()
        {
            WaferNum = "";
            SN = "";
            MainImgKey = "";
            RAWImgURL = "";
            CaptureImg = "";
            CaptureRev = "";
            MUpdateTime = "";
            ProbeChecked = "";
            FileName = "";
            ChildImgs = new Dictionary<string, List<SonImg>>();
        }

        public string WaferNum { set; get; }
        public string SN { set; get; }
        public string MainImgKey { set; get; }
        public string RAWImgURL { set; get; }
        public string CaptureImg { set; get; }
        public string CaptureRev { set; get; }
        public string MUpdateTime { set; get; }
        public string ProbeChecked { set; get; }
        public string FileName { set; get; }
        public Dictionary<string, List<SonImg>> ChildImgs { set; get; }
    }
}
