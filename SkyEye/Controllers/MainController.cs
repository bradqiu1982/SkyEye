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

                    heartbeatlog("GeneralOCRVM.RefreshNewLotNum");
                    try
                    {
                        GeneralOCRVM.RefreshNewLotNum(this);
                    }
                    catch (Exception ex) { }

                    heartbeatlog("GeneralOCRVM.ParseNewLot");
                    try
                    {
                        GeneralOCRVM.ParseNewLot(this);
                    }
                    catch (Exception ex) { }
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





        public ActionResult XYSN()
        { return View(); }

        public JsonResult UPDATEXYSNData()
        {
            var marks = Request.Form["marks"];
            List<string> snlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());

            var snfilelist = new List<object>();
            if (snlist.Count == 15
                || snlist.Count == 20
                || snlist.Count == 52)
            {
                if (snlist[0].Length == 14 || snlist[0].Length == 18)
                {
                    var wafer = "";
                    if (snlist[0].Length == 14)
                    { wafer = snlist[0].Substring(0, 10); }
                    else
                    { wafer = snlist[0].Substring(0, 14); }

                    if (snlist.Count == 15)
                    {
                        OGPFatherImg.Update1x1SN(snlist, wafer);
                    }
                    else if (snlist.Count == 20)
                    {
                        OGPFatherImg.Update1x4SN(snlist, wafer);
                    }
                    else
                    {
                        OGPFatherImg.Update1x12SN(snlist, wafer);
                    }

                    //OGPFatherImg.Update1x1SN(snlist, wafer,640);

                    snfilelist = OGPFatherImg.GetSNFileData(wafer);
                    var ret = new JsonResult();
                    ret.MaxJsonLength = Int32.MaxValue;
                    ret.Data = new
                    {
                        snfilelist = snfilelist,
                        MSG = ""
                    };
                    return ret;
                }
                else
                {
                    var ret = new JsonResult();
                    ret.MaxJsonLength = Int32.MaxValue;
                    ret.Data = new
                    {
                        snfilelist = snfilelist,
                        MSG = "the sn length is not correct"
                    };
                    return ret;
                }
            }
            else
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    snfilelist = snfilelist,
                    MSG = "the sn list count is not correct"
                };
                return ret;
            }
        }



    }

}