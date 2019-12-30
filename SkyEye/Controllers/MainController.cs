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

        //public JsonResult ExistImgTrain()
        //{
        //    var caprev = Request.Form["cond"];
        //    var imglist = OGPFatherImg.GetExistUnTrainedImg(caprev);
        //    var ret = new JsonResult();
        //    ret.MaxJsonLength = Int32.MaxValue;
        //    ret.Data = new {
        //        imglist = imglist
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


        //public JsonResult UpdateTrainingData()
        //{
        //    var imgkv = Request.Form["imgkv"];
        //    List<string> kvlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(imgkv, (new List<string>()).GetType());
        //    foreach (var kv in kvlist)
        //    {
        //        var kvs = kv.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
        //        var k = kvs[0].Trim();
        //        var v = (int)Convert.ToChar(kvs[1].Trim().Substring(0, 1).ToUpper());
        //        SonImg.UpdateCheckedImgVal(k, v);
        //    }

        //    var ret = new JsonResult();
        //    ret.MaxJsonLength = Int32.MaxValue;
        //    ret.Data = new
        //    {
        //        success = true
        //    };
        //    return ret;
        //}
    }

}