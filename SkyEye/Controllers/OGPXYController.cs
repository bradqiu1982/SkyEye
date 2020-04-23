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

        public ActionResult OGPXYProfessionReview()
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

        private bool CheckWaferParseFile(string wafer)
        {
            var file = Server.MapPath("~/userfiles") + "\\"+wafer+"_PARSING";
            if (System.IO.File.Exists(file))
            { return false; }
            System.IO.File.WriteAllText(file, "HELLO");
            return true;
        }

        private void CleanWaferParseFile(string wafer)
        {
            var file = Server.MapPath("~/userfiles") + "\\" + wafer + "_PARSING";
            if (System.IO.File.Exists(file))
            {
                try
                { System.IO.File.Delete(file); }
                catch (Exception ex) { }
            }
        }

        public JsonResult NewImgTrain()
        {
            var ret = new JsonResult();
            var folder = Request.Form["fpath"];
            var wafer = Request.Form["wafer"].Trim().Replace("\\", "").Replace("/", "");

            var imglist = new List<object>();
            var failimg = "";

            if (!CheckWaferParseFile(wafer))
            {
                ret.Data = new
                {
                    imglist = imglist,
                    failimg = failimg,
                    MSG = "This wafer "+wafer+" is parsing now!"
                };
                return ret;
            }

            var snmap = OGPXYFileSNMap.GetSNMap(wafer);
            var probexymap = ProbeXYMap.GetProbeXYMap(wafer);

            OGPFatherImg.CleanWaferData(wafer);
            KMode.CleanTrainCache(this);

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
                CleanWaferParseFile(wafer);
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
                    CleanWaferParseFile(wafer);
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

            CleanWaferParseFile(wafer);

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
            var wafer = Request.Form["wafer"].Trim().Replace("\\", "").Replace("/", "");
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
            var wafernum = Request.Form["wafernum"].Trim().Replace("\\", "").Replace("/", "");
            var xylist = OGPSNXYVM.GetConbineXY(wafernum);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                xylist = xylist
            };
            return ret;

        }

        public JsonResult OGPXYRecognize()
        {
            var wafer = Request.Form["wafer"].Trim().Replace("\\", "").Replace("/", "");
            var folder = Request.Form["fpath"];

            var xylist = new List<OGPSNXYVM>();
            var ret = new JsonResult();

            if (!CheckWaferParseFile(wafer))
            {
                ret.Data = new
                {
                    xylist = xylist,
                    MSG = "This wafer " + wafer + " is parsing now!"
                };
                return ret;
            }

            var snmap = OGPXYFileSNMap.GetSNMap(wafer);
            var probexymap = ProbeXYMap.GetProbeXYMap(wafer);

            OGPFatherImg.CleanWaferData(wafer);
            KMode.CleanTrainCache(this);

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
                CleanWaferParseFile(wafer);
                ret.Data = new
                {
                    xylist = xylist,
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
                    CleanWaferParseFile(wafer);
                    ret.Data = new
                    {
                        xylist = xylist,
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
                    var imgkey = OGPFatherImg.LoadImg(fs, wafer, snmap, probexymap, caprev, this);
                    if (!string.IsNullOrEmpty(imgkey))
                    {
                        keylist.Add(imgkey);
                    }
                    else
                    { failimg += fn + "/"; }
                }
            }

            xylist = OGPSNXYVM.GetConbineXY(wafer);

            CleanWaferParseFile(wafer);

            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                xylist = xylist,
                MSG = ""
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

                    OGPFatherImg.UpdateModification(mainkey);
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

        public ActionResult OCR4Operater()
        {
            return View();
        }

        public JsonResult GetOCRLotnumList()
        {
            var vallist = GeneralOCRVM.GetLotNumList();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                vallist = vallist.Keys.ToList()
            };
            return ret;
        }

        public JsonResult GetOCRProdList()
        {
            var vallist = GeneralOCRVM.GetProductList();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                vallist = vallist.Keys.ToList()
            };
            return ret;
        }

        public JsonResult GetOCRMacList()
        {
            var vallist = GeneralOCRVM.GetMachineList();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                vallist = vallist.Keys.ToList()
            };
            return ret;
        }

        public JsonResult GetOCRVM()
        {
            var lotnum = Request.Form["lotnum"];
            var sdate = Request.Form["sdate"];
            var edate = Request.Form["edate"];
            var prod = Request.Form["prod"];
            var mac = Request.Form["mac"];
            var ssdate = "";
            if (!string.IsNullOrEmpty(sdate))
            { ssdate = UT.O2T(sdate).ToString("yyyy-MM-dd") + " 00:00:00"; }
            var eedate = "";
            if (!string.IsNullOrEmpty(edate))
            {  eedate = UT.O2T(edate).ToString("yyyy-MM-dd") + " 23:59:59"; }

            GeneralOCRVM.RefreshNewLotNum(this);
            GeneralOCRVM.ParseNewLot(this);
            var ocrlist = GeneralOCRVM.GetOCRVM(ssdate, eedate, mac, prod, lotnum);
            var ocrkey = "";
            var uploader = "";
            var ocrkeydict = new Dictionary<string, bool>();
            foreach (var item in ocrlist)
            {
                if (!ocrkeydict.ContainsKey(item.OCRKey))
                { ocrkeydict.Add(item.OCRKey, true); }
                uploader = item.Uploader;
            }

            if (ocrkeydict.Count > 0)
            { ocrkey = string.Join(":", ocrkeydict.Keys.ToList()); }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                ocrkey = ocrkey,
                uploader = uploader,
                ocrlist = ocrlist
            };
            return ret;
        }

        public JsonResult ConfirmOCRInfo()
        {
            var ocrkey = Request.Form["ocrkey"];
            var conf = Request.Form["conf"];

            var xyval = Request.Form["xyval"];
            if (!string.IsNullOrEmpty(xyval))
            {
                List<string> kvlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(xyval, (new List<string>()).GetType());
                foreach (var kv in kvlist)
                {
                    var kvs = kv.ToUpper().Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                    var mainkey = kvs[0].Trim();
                    var xy = kvs[1];
                    var val = kvs[2].Trim().Replace("X", "").Replace("Y", "");
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

                        OGPFatherImg.UpdateModification(mainkey);
                    }//end if
                }//end foreach
            }//end if


            var ocrkeys = ocrkey.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var k in ocrkeys)
            { GeneralOCRVM.ConfirmOCR(k, conf); }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                success = true
            };
            return ret;
        }

        public ActionResult OCRAudit()
        { return View(); }

        public JsonResult GetOCRAuditVM()
        {
            var lotnum = Request.Form["lotnum"];
            var sdate = Request.Form["sdate"];
            var edate = Request.Form["edate"];
            var prod = Request.Form["prod"];
            var mac = Request.Form["mac"];
            var ssdate = "";
            if (!string.IsNullOrEmpty(sdate))
            { ssdate = UT.O2T(sdate).ToString("yyyy-MM-dd") + " 00:00:00"; }
            var eedate = "";
            if (!string.IsNullOrEmpty(edate))
            { eedate = UT.O2T(edate).ToString("yyyy-MM-dd") + " 23:59:59"; }

            var ocrlist = GeneralOCRVM.GetOCRVM(ssdate, eedate, mac, prod, lotnum);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                ocrlist = ocrlist
            };
            return ret;
        }

        public ActionResult OCRQuery()
        {
            return View();
        }

        public JsonResult QUERYOCRDATA()
        {
            var marks = Request.Form["marks"];
            List<string> snlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());

            var alldata = new List<OGPSNXYVM>();
            var lotdict = GeneralOCRVM.GetLotNumList();
            foreach (var sn in snlist)
            {
                if (lotdict.ContainsKey(sn.ToUpper()))
                {
                    var xylist = OGPSNXYVM.GetLocalOGPXYSNDict(sn);
                    if (xylist.Count > 0)
                    { alldata.AddRange(xylist.Values.ToList()); }
                }
            }

            var realsnlist = new List<string>();
            foreach (var sn in snlist)
            {
                if (!lotdict.ContainsKey(sn.ToUpper()))
                { realsnlist.Add(sn); }
            }

            if (realsnlist.Count > 0)
            {
                var xylist = OGPSNXYVM.GetLocalOGPXYSNDict(realsnlist);
                if (xylist.Count > 0)
                { alldata.AddRange(xylist.Values.ToList()); }
            }

            var lotproddict = GeneralOCRVM.GetLotProdDict();
            foreach (var item in alldata)
            {
                if (lotproddict.ContainsKey(item.WaferNum))
                { item.Product = lotproddict[item.WaferNum]; }
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                ocrlist = alldata
            };
            return ret;
        }

    }
}