using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SkyEye.Models;


namespace SkyEye.Controllers
{
    public class MainController : Controller
    {

        // GET: Main
        public ActionResult Index()
        {
            //Update1x4SN();
            return View();
        }

        private void heartbeatlog(string msg)
        {
            try
            {
                var filename = "log" + DateTime.Now.ToString("yyyy-MM-dd");
                var wholefilename = Server.MapPath("~/userfiles") + "\\" + filename;

                var content = "";
                if (System.IO.File.Exists(wholefilename))
                {
                    content = System.IO.File.ReadAllText(wholefilename);
                }
                content = content + msg + " @ " + DateTime.Now.ToString() + "\r\n";
                System.IO.File.WriteAllText(wholefilename, content);
            }
            catch (Exception ex)
            { }
        }


        public ActionResult HeartBeat()
        {
            try
            {
                var heartbeatinprocess = Server.MapPath("~/userfiles") + "\\" + "InHeartBeatProcess";
                if (System.IO.File.Exists(heartbeatinprocess))
                {
                    var lastinprocesstime = System.IO.File.GetLastWriteTime(heartbeatinprocess);
                    if ((DateTime.Now - lastinprocesstime).Hours >= 6)
                    { System.IO.File.Delete(heartbeatinprocess); }
                    else
                    { return View(); }
                }
                System.IO.File.WriteAllText(heartbeatinprocess, "hello");

                heartbeatlog("start heartbeat");

                var dailyscan = Server.MapPath("~/userfiles") + "\\" + "dailyscan_" + DateTime.Now.ToString("yyyy-MM-dd");
                if (!System.IO.File.Exists(dailyscan))
                {
                    System.IO.File.WriteAllText(dailyscan, "hello");
                }

                heartbeatlog("end heartbeat");

                try
                { System.IO.File.Delete(heartbeatinprocess); }
                catch (Exception ex) { }
            }
            catch (Exception ex) { }

            return View();
        }


        //public ActionResult LoadOneImg()
        //{
        //    var imgkey = FatherImg.LoadImg(@"E:\video\die3.BMP",this);
        //    ViewBag.bimg = FatherImg.GetCaptureImg(imgkey);
        //    return View();
        //}

        //public ActionResult TrainingNewImgs()
        //{
        //    return View();
        //}

        //public JsonResult GetCaptureRevList()
        //{
        //    var caprevlist = OGPFatherImg.GetCaptureRevList();
        //    var ret = new JsonResult();
        //    ret.MaxJsonLength = Int32.MaxValue;
        //    ret.Data = new
        //    {
        //        caprevlist = caprevlist
        //    };
        //    return ret;
        //}


        //public JsonResult NewImgTrain()
        //{
        //    var folder = Request.Form["cond"];
        //    var filelist = ExternalDataCollector.DirectoryEnumerateFiles(this, folder);
        //    var keylist = new List<string>();
        //    foreach (var fs in filelist)
        //    {
        //        var fn = System.IO.Path.GetFileName(fs).ToUpper();
        //        if (fn.Contains(".BMP") || fn.Contains(".PNG") || fn.Contains(".JPG"))
        //        {
        //            var imgkey = OGPFatherImg.LoadImg(fs, this);
        //            if (!string.IsNullOrEmpty(imgkey))
        //            {
        //                keylist.Add(imgkey);
        //            }
        //        }
        //    }

        //    var imglist = OGPFatherImg.NewUnTrainedImg(keylist);
        //    var ret = new JsonResult();
        //    ret.MaxJsonLength = Int32.MaxValue;
        //    ret.Data = new
        //    {
        //        imglist = imglist
        //    };
        //    return ret;
        //}


        private void UpdateSN_(string die, string sn, string wafer)
        {
            var sql = "update [WAT].[dbo].[OGPFatherImg] set SN = @SN where WaferNum = '" + wafer + "' and SN = @die";
            var dict = new Dictionary<string, string>();
            dict.Add("@SN", sn);
            dict.Add("@die", die);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private void Update1x4SN()
        {
            var maplist = new List<string>();

            var snlist = new List<string>();
            snlist.Add("191812-30E1020");
            snlist.Add("191812-30E1019");
            snlist.Add("191812-30E0818");
            snlist.Add("191812-30E0817");
            snlist.Add("191812-30E0816");
            snlist.Add("191812-30E0815");
            snlist.Add("191812-30E0814");
            snlist.Add("191812-30E0813");
            snlist.Add("191812-30E0812");
            snlist.Add("191812-30E0811");
            snlist.Add("191812-30E0810");
            snlist.Add("191812-30E0809");
            snlist.Add("191812-30E0808");
            snlist.Add("191812-30E0807");
            snlist.Add("191812-30E0806");
            snlist.Add("191812-30E0805");
            snlist.Add("191812-30E0604");
            snlist.Add("191812-30E0603");
            snlist.Add("191812-30E0602");
            snlist.Add("191812-30E0601");

            var snidx = 0;

            for (var idx = 0; idx < 160; idx++)
            {
                var midx = idx % 8 + 1;
                var sn = snlist[snidx] + ":::" + midx;
                var die = "Die-" + (idx + 1);

                UpdateSN_(die, sn, "191812-30E");

                if (midx == 8)
                { snidx++; }
            }
        }

        private void Update1x12SN()
        {
            var maplist = new List<string>();

            var snlist = new List<string>();
            snlist.Add("192406-50R0601");
            snlist.Add("192406-50R0602");
            snlist.Add("192406-50R0603");
            snlist.Add("192406-50R0604");
            snlist.Add("192406-50R0605");
            snlist.Add("192406-50R0806");
            snlist.Add("192406-50R0807");
            snlist.Add("192406-50R0808");
            snlist.Add("192406-50R0809");
            snlist.Add("192406-50R0810");
            snlist.Add("192406-50R0811");
            snlist.Add("192406-50R0812");
            snlist.Add("192406-50R0813");
            snlist.Add("192406-50R0814");
            snlist.Add("192406-50R0815");
            snlist.Add("192406-50R0816");
            snlist.Add("192406-50R0817");
            snlist.Add("192406-50R0818");
            snlist.Add("192406-50R0819");
            snlist.Add("192406-50R0820");
            snlist.Add("192406-50R0821");
            snlist.Add("192406-50R0822");
            snlist.Add("192406-50R0823");
            snlist.Add("192406-50R0824");
            snlist.Add("192406-50R0825");
            snlist.Add("192406-50R0826");
            snlist.Add("192406-50R0827");
            snlist.Add("192406-50R0828");
            snlist.Add("192406-50R0829");
            snlist.Add("192406-50R0830");
            snlist.Add("192406-50R0831");
            snlist.Add("192406-50R0832");
            snlist.Add("192406-50R0833");
            snlist.Add("192406-50R0834");
            snlist.Add("192406-50R0835");
            snlist.Add("192406-50R0836");
            snlist.Add("192406-50R0837");
            snlist.Add("192406-50R0838");
            snlist.Add("192406-50R0839");
            snlist.Add("192406-50R0840");
            snlist.Add("192406-50R0841");
            snlist.Add("192406-50R0842");
            snlist.Add("192406-50R0843");
            snlist.Add("192406-50R0844");
            snlist.Add("192406-50R0845");
            snlist.Add("192406-50R0846");
            snlist.Add("192406-50R0847");
            snlist.Add("192406-50R0848");
            snlist.Add("192406-50R1049");
            snlist.Add("192406-50R1050");
            snlist.Add("192406-50R1051");
            snlist.Add("192406-50R1052");


            var snidx = 0;

            for (var idx = 0; idx < 104; idx++)
            {
                var midx = idx % 2 + 1;
                var sn = snlist[snidx] + ":::" + midx;
                var die = "Die-" + (idx + 1);

                UpdateSN_(die, sn, "192406-50R");

                if (midx == 2)
                { snidx++; }
            }
        }

        private void Update1x1SN()
        {
            var maplist = new List<string>();

            var snlist = new List<string>();
            snlist.Add("61940-277-040E0809");
            snlist.Add("61940-277-040E0813");
            snlist.Add("61940-277-040E0816");
            snlist.Add("61940-277-040E0808");
            snlist.Add("61940-277-040E0814");
            snlist.Add("61940-277-040E0815");
            snlist.Add("61940-277-040E0807");
            snlist.Add("61940-277-040E0805");
            snlist.Add("61940-277-040E0806");
            snlist.Add("61940-277-040E1020");
            snlist.Add("61940-277-040E1019");
            snlist.Add("61940-277-040E0601");
            snlist.Add("61940-277-040E0602");
            snlist.Add("61940-277-040E0603");
            snlist.Add("61940-277-040E0604");
            snlist.Add("61940-277-040E0812");
            snlist.Add("61940-277-040E0810");
            snlist.Add("61940-277-040E0811");
            snlist.Add("61940-277-040E0818");
            snlist.Add("61940-277-040E0817");

            var snidx = 0;

            for (var idx = 0; idx < 480; idx++)
            {
                var midx = idx % 32 + 1;
                var sn = snlist[snidx] + ":::" + midx;
                var die = "Die-" + (idx + 1);

                UpdateSN_(die, sn, "61940-277-040E");

                if (midx == 32)
                { snidx++; }
            }
        }


    }

}