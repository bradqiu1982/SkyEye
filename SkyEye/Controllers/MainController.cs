using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SkyEye.Models;
using System.Net;
using System.IO;
using OpenCvSharp;

namespace SkyEye.Controllers
{
    public class MainController : Controller
    {

        // GET: Main
        public ActionResult Index()
        {
            return View();
        }

        //http://wuxinpi.chn.ii-vi.net:9091/Main/RefreshLotCoord
        public ActionResult RefreshLotCoord()
        {
            try
            {
                GeneralOCRVM.RefreshNewLotNum(this);
            }
            catch (Exception ex) { }

            try
            {
                GeneralOCRVM.ParseNewLot(this);
            }
            catch (Exception ex) { }

            return View("Index");
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
                    else if (snlist.Count == 52)
                    {
                        OGPFatherImg.Update1x12SN(snlist, wafer);
                    }


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

        public JsonResult UPDATEXYSNDataNew()
        {
            var marks = Request.Form["marks"];
            List<string> snlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());

            var snfilelist = new List<object>();
            if (snlist.Count == 11
                || snlist.Count == 16
                || snlist.Count == 47)
            {
                if (snlist[0].Length == 14 || snlist[0].Length == 18)
                {
                    var wafer = "";
                    if (snlist[0].Length == 14)
                    { wafer = snlist[0].Substring(0, 10); }
                    else
                    { wafer = snlist[0].Substring(0, 14); }

                    if (snlist.Count == 11)
                    {
                        OGPFatherImg.Update1x1SN(snlist, wafer,352);
                    }
                    else if (snlist.Count == 16)
                    {
                        OGPFatherImg.Update1x4SN(snlist, wafer,128);
                    }
                    else if (snlist.Count == 47)
                    {
                        OGPFatherImg.Update1x12SN(snlist, wafer,94);
                    }

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

        //private void UpdateOCRSNClient()
        //{
        //    var updatesnobj = new List<object>();
        //    updatesnobj.Add(new
        //    {
        //        lotnum = "S2004100792",
        //        sn = "X3FAPZS_0",
        //        x = "X225",
        //        y = "Y272"
        //    });

        //    updatesnobj.Add(new
        //    {
        //        lotnum = "S2004100792",
        //        sn = "X3FAPZS_1",
        //        x = "X228",
        //        y = "Y273"
        //    });

        //    var client = new RestSharp.RestClient("http://wuxinpi.chn.ii-vi.net:9091/Main/UpdateGeneralOCRSN");
        //    var request = new RestSharp.RestRequest(RestSharp.Method.POST);
        //    request.RequestFormat = RestSharp.DataFormat.Json;
        //    request.AddParameter("updatesn", Newtonsoft.Json.JsonConvert.SerializeObject(updatesnobj));
        //    client.Execute(request);
        //}

        public JsonResult UpdateGeneralOCRSN()
        {
            string updatesn = Request.Form["updatesn"];
            var upsnlist = (List<SnCoord>)Newtonsoft.Json.JsonConvert.DeserializeObject(updatesn, (new List<SnCoord>()).GetType());

            foreach (var item in upsnlist)
            {
                var vm = OGPSNXYVM.GetLocalOGPXYSNDict(item.lotnum, item.sn);
                if (vm.Count > 0)
                {
                    var val = vm.Values.ToList()[0];
                    if (val.X.Contains(item.x.Replace("X", "").Replace("x", ""))
                        && val.Y.Contains(item.y.Replace("Y", "").Replace("y", "")))
                    { }
                    else
                    {
                        var modified = false;
                        var xcharlist = item.x.Replace("X", "").Replace("x", "").ToList();
                        if (xcharlist.Count == 3)
                        {
                            SonImg.UpdateImgVal(val.MainImgKey, 1, (int)Convert.ToChar("X"));
                            SonImg.UpdateImgVal(val.MainImgKey, 2, (int)xcharlist[0]);
                            SonImg.UpdateImgVal(val.MainImgKey, 3, (int)xcharlist[1]);
                            SonImg.UpdateImgVal(val.MainImgKey, 4, (int)xcharlist[2]);
                            modified = true;
                        }

                        var ycharlist = item.y.Replace("Y", "").Replace("y", "").ToList();
                        if (ycharlist.Count == 3)
                        {
                            SonImg.UpdateImgVal(val.MainImgKey, 5, (int)Convert.ToChar("Y"));
                            SonImg.UpdateImgVal(val.MainImgKey, 6, (int)ycharlist[0]);
                            SonImg.UpdateImgVal(val.MainImgKey, 7, (int)ycharlist[1]);
                            SonImg.UpdateImgVal(val.MainImgKey, 8, (int)ycharlist[2]);
                            modified = true;
                        }

                        if (modified)
                        { OGPFatherImg.UpdateModification(val.MainImgKey); }

                    }//end else
                }//end if
            }//foreach

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                MSG = "OK"
            };
            return ret;
        }

        public ActionResult SpecialXYSN()
        {
            return View();
        }

        public JsonResult UPDATEXYSNDataSpecial()
        {
            var ocrnum = Request.Form["ocrnum"];
            var marks = Request.Form["marks"];
            List<string> snlist = (List<string>)Newtonsoft.Json.JsonConvert.DeserializeObject(marks, (new List<string>()).GetType());
            var arrayzie = UT.O2I(Request.Form["arraysize"]);

            var snfilelist = new List<object>();
            if (snlist[0].Length == 11 || snlist[0].Length == 14 || snlist[0].Length == 18)
            {
                var wafer = "";

                if (!string.IsNullOrEmpty(ocrnum))
                {
                    wafer = ocrnum;
                }
                else
                {
                    if (snlist[0].Length == 14)
                    { wafer = snlist[0].Substring(0, 10); }
                    else
                    { wafer = snlist[0].Substring(0, 14); }
                }

                if (arrayzie == 1)
                {
                    var cnt = snlist.Count * 32;
                    OGPFatherImg.Update1x1SN(snlist, wafer, cnt);
                }
                else if (arrayzie == 2)
                {
                    var cnt = snlist.Count * 16;
                    OGPFatherImg.Update1x2SN(snlist, wafer, cnt);
                }
                else if (arrayzie == 4)
                {
                    var cnt = snlist.Count * 8;
                    OGPFatherImg.Update1x4SN(snlist, wafer, cnt);
                }
                else if (arrayzie == 12)
                {
                    var cnt = snlist.Count * 2;
                    OGPFatherImg.Update1x12SN(snlist, wafer, cnt);
                }

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

        //public ActionResult PostSNData()
        //{
        //    var updatesnobj = new List<object>();
        //    updatesnobj.Add(new
        //    {
        //        lotnum = "S2004100792",
        //        sn = "X3FAPZS_0",
        //        x = "X225",
        //        y = "Y273"
        //    });

        //    updatesnobj.Add(new
        //    {
        //        lotnum = "S2004100792",
        //        sn = "X3FAPZS_3",
        //        x = "X228",
        //        y = "Y273"
        //    });

        //    var client = new RestSharp.RestClient("http://localhost:9091/Main/UpdateGeneralOCRSN");
        //    var request = new RestSharp.RestRequest(RestSharp.Method.POST);
        //    request.RequestFormat = RestSharp.DataFormat.Json;
        //    request.AddParameter("updatesn", Newtonsoft.Json.JsonConvert.SerializeObject(updatesnobj));
        //    client.Execute(request);

        //    return View("Index");
        //}


        public ActionResult VCSELAIDemo()
        {
            return View();
        }

        public JsonResult VCSELAIDemoData()
        {
            var imgtypedetect = ImageDetect.GetVCSELTypeCNN(this);
            var urllist = new List<string>();

            var folder = Request.Form["fpath"];
            var filelist = ExternalDataCollector.DirectoryEnumerateFiles(this, folder);
            var samplepicture = new List<string>();
            foreach (var fs in filelist)
            {
                var fn = System.IO.Path.GetFileName(fs).ToUpper();
                if (fn.Contains(".BMP") || fn.Contains(".PNG") || fn.Contains(".JPG"))
                {
                    var typedetect = ImageDetect.GetVCSELTypeSingle(imgtypedetect, fs);
                    var imgtp = typedetect.ImgType.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    var vtype = imgtp[0];
                    var vdir = imgtp[1];

                    var img = Cv2.ImRead(fs, ImreadModes.Color);
                    var detectsize= ImgPreOperate.GetImageBoundPointX(img);
                    img = img.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

                    if (vdir.Contains("LF"))
                    {
                        var outxymat = new Mat();
                        Cv2.Transpose(img, outxymat);
                        Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                        img = outxymat;
                    }
                    else if (vdir.Contains("DW"))
                    {
                        var outxymat = new Mat();
                        Cv2.Transpose(img, outxymat);
                        Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                        Cv2.Transpose(outxymat, outxymat);
                        Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                        img = outxymat;
                    }
                    else if (vdir.Contains("RT"))
                    {
                        var outxymat = new Mat();
                        Cv2.Transpose(img, outxymat);
                        Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                        Cv2.Transpose(outxymat, outxymat);
                        Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                        Cv2.Transpose(outxymat, outxymat);
                        Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                        img = outxymat;
                    }

                    var jpgfile = ImageObjDetect.WriteRawImg(img, this);
                    if (!string.IsNullOrEmpty(jpgfile))
                    {
                        var boxes = ImageObjDetect.PYOBJDect(jpgfile.Replace("\\","/"), vtype);
                        foreach (var box in boxes)
                        {
                            var pt1 = new Point((int)(box.left * img.Width), (int)(box.top * img.Height));
                            var pt2 = new Point((int)(box.right * img.Width), (int)(box.botm * img.Height));
                            Cv2.Rectangle(img,pt1,pt2,new Scalar(0,0,255),3);
                        }//end foreach

                        Cv2.PutText(img, vtype, new Point(10, 40), HersheyFonts.HersheySimplex, 1, new Scalar(0, 0, 255),2,LineTypes.Link8);
                        jpgfile = ImageObjDetect.WriteRawImg(img, this);
                        var url = "/userfiles" + jpgfile.Split(new string[] { "userfiles" }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("\\", "/");
                        urllist.Add(url);
                    }
                }//end if
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                urllist = urllist
            };
            return ret;
        }


    }

}