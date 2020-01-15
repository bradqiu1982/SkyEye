using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenCvSharp;
using System.Web.Mvc;
using System.IO;

namespace SkyEye.Models
{
    public class OGPFatherImg
    {

        public static string GetPictureRev(string imgpath)
        {
            var xyrectlist = ImgOperate5x1.FindXYRect(imgpath, 25, 43, 4.5, 6.8, 8000);
            if (xyrectlist.Count > 0)
            { return "OGP-rect5x1"; }

            xyrectlist = ImgOperate2x1.FindXYRect(imgpath, 60, 100, 2.0, 3.0);
            if (xyrectlist.Count > 0)
            { return "OGP-rect2x1"; }

            return string.Empty;
        }

        public static string LoadImg(string imgpath,string wafer,Dictionary<string,string> snmap
            , Dictionary<string, bool> probexymap,string caprev, Controller ctrl)
        {
            if (caprev.Contains("OGP-rect5x1"))
            {
                var xyrectlist = ImgOperate5x1.FindXYRect(imgpath, 25, 43, 4.5, 6.8, 8000);
                if (xyrectlist.Count > 0)
                {
                    var charmatlist = ImgOperate5x1.CutCharRect(imgpath, xyrectlist[0], 50, 90, 40, 65);
                    if (charmatlist.Count > 0)
                    {
                        //var caprev = "OGP-rect5x1";
                        using (var kmode = KMode.GetTrainedMode(caprev, ctrl))
                        {
                            return SolveImg(imgpath, wafer, charmatlist, caprev, snmap, probexymap, ctrl, kmode);
                        }
                    }
                }
            }
            else if (caprev.Contains("OGP-rect2x1"))
            {
                var xyrectlist = ImgOperate2x1.FindXYRect(imgpath, 60, 100, 2.0, 3.0);
                if (xyrectlist.Count > 0)
                {
                    var charmatlist = ImgOperate2x1.CutCharRect(imgpath, xyrectlist[0], 40, 56);
                    if (charmatlist.Count > 0)
                    {
                        //var caprev = "OGP-rect2x1";
                        using (var kmode = KMode.GetTrainedMode(caprev, ctrl))
                        {
                            return SolveImg(imgpath, wafer, charmatlist, caprev, snmap, probexymap, ctrl, kmode);
                        }
                    }
                }
            }

            Mat rawimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var fimg = new OGPFatherImg();
            fimg.WaferNum = wafer;
            fimg.SN = Path.GetFileNameWithoutExtension(imgpath);
            fimg.MainImgKey = GetUniqKey();
            fimg.RAWImgURL = WriteRawImg(rawimg, fimg.MainImgKey, ctrl);
            fimg.CaptureImg = "";
            fimg.CaptureRev = "";
            fimg.MUpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            fimg.StoreFailData();

            return string.Empty;
        }


        private static string SolveImg(string imgpath,string wafer, List<Mat> charmatlist, string caprev
            , Dictionary<string, string> snmap, Dictionary<string, bool> probexymap, Controller ctrl, OpenCvSharp.ML.KNearest kmode)
        {
            var ret = "";

            Mat rawimg = Cv2.ImRead(imgpath, ImreadModes.Color);

            var fimg = new OGPFatherImg();
            fimg.WaferNum = wafer;
            fimg.SN = Path.GetFileNameWithoutExtension(imgpath);
            if (snmap.ContainsKey(fimg.SN))
            { fimg.SN = snmap[fimg.SN]; }

            fimg.MainImgKey = GetUniqKey();
            fimg.RAWImgURL = WriteRawImg(rawimg, fimg.MainImgKey, ctrl);
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
                var imgval = kmode.FindNearest(stcm, 1, resultmat);
                if (imgval > 0)
                { sonimg.ImgVal = (int)imgval; }

                if (idx < 5)
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

        private static string WriteRawImg(Mat rawimg, string mk, Controller ctrl)
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
            var sql = @"insert into WAT.dbo.OGPFatherImg(WaferNum,SN,MainImgKey,RAWImgURL,CaptureImg,CaptureRev,MUpdateTime,Appv_1) 
                    values(@WaferNum,@SN,@MainImgKey,@RAWImgURL,@CaptureImg,@CaptureRev,@MUpdateTime,@ProbeChecked)";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", WaferNum);
            dict.Add("@SN", SN);
            dict.Add("@MainImgKey", MainImgKey);
            dict.Add("@RAWImgURL", RAWImgURL);
            dict.Add("@CaptureImg", CaptureImg);
            dict.Add("@CaptureRev", CaptureRev);
            dict.Add("@MUpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dict.Add("@ProbeChecked", ProbeChecked);
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
                if (xydict.ContainsKey(mk))
                {
                    xcoord = xydict[mk].X;
                    ycoord = xydict[mk].Y;
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
                    ycoord = ycoord
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
                if (xydict.ContainsKey(mk))
                {
                    xcoord = xydict[mk].X;
                    ycoord = xydict[mk].Y;
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
                    ycoord = ycoord
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
        public Dictionary<string, List<SonImg>> ChildImgs { set; get; }
    }
}
