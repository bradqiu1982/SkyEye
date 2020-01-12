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
            var ret = new JsonResult();
            var folder = Request.Form["fpath"];
            var wafer = Request.Form["wafer"];

            var snmap = OGPXYFileSNMap.GetSNMap(wafer);
            var probexymap = ProbeXYMap.GetProbeXYMap(wafer);

            OGPFatherImg.CleanWaferData(wafer);
            KMode.CleanTrainCache(this);

            var imglist = new List<object>();
            var failimg = "";

            var filelist = ExternalDataCollector.DirectoryEnumerateFiles(this, folder);
            var samplepicture = new List<string>();
            foreach (var fs in filelist)
            {
                var fn = System.IO.Path.GetFileName(fs).ToUpper();
                if (fn.Contains(".BMP") || fn.Contains(".PNG") || fn.Contains(".JPG"))
                {
                    samplepicture.Add(fs);
                    if (samplepicture.Count > 1)
                    { break; }
                }
            }

            if (samplepicture.Count == 0)
            {
                ret.Data = new
                {
                    imglist = imglist,
                    failimg = failimg,
                    MSG = "Failed to get enough sample picture for revsion dugement!"
                };
                return ret;
            }

            var caprev = "";
            caprev = OGPFatherImg.GetPictureRev(samplepicture[0]);
            if (string.IsNullOrEmpty(caprev))
            {
                caprev = OGPFatherImg.GetPictureRev(samplepicture[1]);
                if (string.IsNullOrEmpty(caprev))
                {
                    ret.Data = new
                    {
                        imglist = imglist,
                        failimg = failimg,
                        MSG = "Failed to get revsion from sample pictures!"
                    };
                    return ret;
                }
            }

            var keylist = new List<string>();
            foreach (var fs in filelist)
            {
                var fn = System.IO.Path.GetFileName(fs).ToUpper();
                if (fn.Contains(".BMP") || fn.Contains(".PNG") || fn.Contains(".JPG"))
                {
                    var imgkey = OGPFatherImg.LoadImg(fs,wafer,snmap, probexymap,caprev, this);
                    if (!string.IsNullOrEmpty(imgkey))
                    {
                        keylist.Add(imgkey);
                    }
                    else
                    { failimg += fn + "/"; }
                }
            }

            imglist = OGPFatherImg.NewUnTrainedImg(keylist,wafer);
            
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                imglist = imglist,
                failimg = failimg,
                MSG = ""
            };
            return ret;
        }

        public JsonResult ExistImgTrain()
        {
            var wafer = Request.Form["wafer"];
            var imglist = OGPFatherImg.ExistTrainedImg(wafer);
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

        public ActionResult OGPXYCompare()
        { return View(); }

        public JsonResult OGPXYCompareData()
        {
            var wafernum = Request.Form["wafernum"];
            var xylist = OGPSNXYVM.GetConbineXY(wafernum);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                xylist = xylist
            };
            return ret;

        }

        public JsonResult UpdateOGPXYData()
        {
            var imgkv = Request.Form["imgkv"];
            List<string> kvlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(imgkv, (new List<string>()).GetType());
            foreach (var kv in kvlist)
            {
                var kvs = kv.ToUpper().Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var mainkey = kvs[0].Trim();
                var xy = kvs[1];
                var val = kvs[2].Trim().Replace("X","").Replace("Y","");
                if (val.Length == 3)
                {
                    var charlist = val.ToList();
                    if (xy.Contains("X"))
                    {
                        SonImg.UpdateImgVal(mainkey, 1, (int)Convert.ToChar("X"));
                        SonImg.UpdateImgVal(mainkey, 2, (int)charlist[0]);
                        SonImg.UpdateImgVal(mainkey, 3, (int)charlist[1]);
                        SonImg.UpdateImgVal(mainkey, 4, (int)charlist[2]);
                    }
                    else
                    {
                        SonImg.UpdateImgVal(mainkey, 5, (int)Convert.ToChar("Y"));
                        SonImg.UpdateImgVal(mainkey, 6, (int)charlist[0]);
                        SonImg.UpdateImgVal(mainkey, 7, (int)charlist[1]);
                        SonImg.UpdateImgVal(mainkey, 8, (int)charlist[2]);
                    }
                }
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