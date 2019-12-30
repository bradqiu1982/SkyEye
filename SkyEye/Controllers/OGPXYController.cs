using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SkyEye.Models;

namespace SkyEye.Controllers
{
    public class OGPXYController : Controller
    {
        public ActionResult TrainingNewImg()
        {
            return View();
        }

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

        public JsonResult NewImgTrain()
        {
            var folder = Request.Form["fpath"];
            var wafer = Request.Form["wafer"];

            var filelist = ExternalDataCollector.DirectoryEnumerateFiles(this, folder);
            var keylist = new List<string>();
            foreach (var fs in filelist)
            {
                var fn = System.IO.Path.GetFileName(fs).ToUpper();
                if (fn.Contains(".BMP") || fn.Contains(".PNG") || fn.Contains(".JPG"))
                {
                    var imgkey = OGPFatherImg.LoadImg(fs,wafer, this);
                    if (!string.IsNullOrEmpty(imgkey))
                    {
                        keylist.Add(imgkey);
                    }
                }
            }

            var imglist = OGPFatherImg.NewUnTrainedImg(keylist);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                imglist = imglist
            };
            return ret;
        }


        public JsonResult UpdateTrainingData()
        {
            var imgkv = Request.Form["imgkv"];
            List<string> kvlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(imgkv, (new List<string>()).GetType());
            foreach (var kv in kvlist)
            {
                var kvs = kv.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var k = kvs[0].Trim();
                var v = (int)Convert.ToChar(kvs[1].Trim().Substring(0, 1).ToUpper());
                OGPFatherImg.UpdateCheckedImgVal(k, v);
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                success = true
            };
            return ret;
        }

    }
}