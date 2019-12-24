using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenCvSharp;
using System.Web.Mvc;
using System.IO;

namespace SkyEye.Models
{
    public class FatherImg
    {

        public static string LoadImg(string imgpath,Controller ctrl)
        {
            var xyrectlist = ImgOperate4x1.FindXYRect(imgpath, 27, 43, 4800, 8000);
            if (xyrectlist.Count > 0)
            {
                var charmatlist = ImgOperate4x1.CutCharRect(imgpath, xyrectlist[0], 30, 50, 20, 50);
                if (charmatlist.Count > 0)
                {
                    return SolveImg4x1(imgpath, charmatlist, ctrl);;
                }
            }
            return string.Empty;
        }


        private static string SolveImg4x1(string imgpath, List<Mat> charmatlist, Controller ctrl)
        {
            var ret = "";

            Mat rawimg = Cv2.ImRead(imgpath, ImreadModes.Color);

            var fimg = new FatherImg();
            fimg.MainImgKey = GetUniqKey();
            fimg.RAWImgURL = WriteRawImg(rawimg, fimg.MainImgKey, ctrl);
            fimg.CaptureImg = Convert.ToBase64String(charmatlist[0].ToBytes());
            fimg.CaptureRev = "rect4x1";
            fimg.MUpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            fimg.StoreData();

            ret = fimg.MainImgKey;

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

                if (idx < 5)
                { sonimg.ChildCat = "X"; }
                else
                { sonimg.ChildCat = "Y"; }
                sonimg.StoreData();

                idx++;
            }

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
            var sql = @"insert into WAT.dbo.FatherImg(MainImgKey,RAWImgURL,CaptureImg,CaptureRev,MUpdateTime) 
                    values(@MainImgKey,@RAWImgURL,@CaptureImg,@CaptureRev,@MUpdateTime)";
            var dict = new Dictionary<string, string>();
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
            var sql = "select distinct CaptureRev from WAT.dbo.FatherImg";
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
            var sql = "select CaptureImg from WAT.dbo.FatherImg where MainImgKey=@MainImgKey";
            var dict = new Dictionary<string, string>();
            dict.Add("@MainImgKey", key);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                return UT.O2S(line[0]);
            }
            return ret;
        }

        public static List<object> GetExistUnTrainedImg(string caprev)
        {
            var ret = new List<object>();

            var sql = "";
            var dict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(caprev))
            {
                sql = @" select top 1000 f.CaptureImg,f.RAWImgURL,s.ChildImg,s.ImgOrder,s.ChildImgKey,s.ImgVal from [WAT].[dbo].[SonImg] (nolock) s
                          inner join [WAT].[dbo].[FatherImg] (nolock) f on f.MainImgKey = s.MainImgKey
                          where s.ImgChecked = 'FALSE' order by UpdateTime desc";
            }
            else
            {
                sql = @"select top 1000  f.CaptureImg,f.RAWImgURL,s.ChildImg,s.ImgOrder,s.ChildImgKey,s.ImgVal from [WAT].[dbo].[SonImg] (nolock) s
                      inner join [WAT].[dbo].[FatherImg] (nolock) f on f.MainImgKey = s.MainImgKey
                      where s.ImgChecked = 'FALSE' and f.CaptureRev = @CaptureRev order by UpdateTime desc";
                dict.Add("@CaptureRev", caprev);
            }
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var imgval = "";
                var ival = UT.O2I(line[5]);
                if (ival != -1)
                { imgval = Convert.ToString((char)ival); }

                ret.Add(new {
                    capimg = UT.O2S(line[0]),
                    rawurl = UT.O2S(line[1]),
                    chimg = UT.O2S(line[2]),
                    chidx = UT.O2S(line[3]),
                    cimgkey = UT.O2S(line[4]),
                    cimgval = imgval
                });
            }
            return ret;
        }

        public static List<object> NewUnTrainedImg(List<string> imgkeys)
        {
            var ret = new List<object>();

            var keycond = "('" + string.Join("','", imgkeys) + "')";
            var sql = @"select  f.CaptureImg,f.RAWImgURL,s.ChildImg,s.ImgOrder,s.ChildImgKey,s.ImgVal from [WAT].[dbo].[SonImg] (nolock) s
                      inner join [WAT].[dbo].[FatherImg] (nolock) f on f.MainImgKey = s.MainImgKey
                      where s.MainImgKey in <keycond> order by s.MainImgKey,s.ImgOrder asc";
            sql = sql.Replace("<keycond>", keycond);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var imgval = "";
                var ival = UT.O2I(line[5]);
                if (ival != -1)
                { imgval = Convert.ToString((char)ival); }

                ret.Add(new
                {
                    capimg = UT.O2S(line[0]),
                    rawurl = UT.O2S(line[1]),
                    chimg = UT.O2S(line[2]),
                    chidx = UT.O2S(line[3]),
                    cimgkey = UT.O2S(line[4]),
                    cimgval = imgval
                });
            }
            return ret;
        }

        public FatherImg()
        {
            MainImgKey = "";
            RAWImgURL = "";
            CaptureImg = "";
            CaptureRev = "";
            MUpdateTime = "";
            ChildImgs = new Dictionary<string, List<SonImg>>();
        }

        public string MainImgKey {set;get;}
        public string RAWImgURL { set; get; }
        public string CaptureImg { set; get; }
        public string CaptureRev { set; get; }
        public string MUpdateTime { set; get; }
        public Dictionary<string, List<SonImg>> ChildImgs { set; get; }
    }
}