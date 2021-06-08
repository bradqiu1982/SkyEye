using OpenCvSharp;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace SkyEye.Models
{
    public class ImageTypeDetect
    {
        //var label_name = new string[] { "A10-UP","A10-RT","A10-DW","A10-LF"
        //    ,"F2X1-UP","F2X1-RT","F2X1-DW","F2X1-LF"
        //    ,"F5X1-UP","F5X1-RT","F5X1-DW","F5X1-LF"
        //    ,"IIVI-UP","IIVI-RT","IIVI-DW","IIVI-LF"
        //    ,"SIX-UP","SIX-RT","SIX-DW","SIX-LF"}.ToList();

        public static ImageTypeDetect GetPictureRevsm(Net vcselTypeNet, string f1,string f2,string f3)
        {
            var ret = GetVCSELTypeWithDirect(vcselTypeNet, f1, f2, f3);
            if (!string.IsNullOrEmpty(ret.ImgType))
            {
                if (ret.ImgType.Contains("-UP") || ret.ImgType.Contains("-DW"))
                {
                    if (ret.ImgType.Contains("SIX-"))
                    { }
                    else if (ret.ImgType.Contains("IIVI-"))
                    { ret.ModelName = "OGP-iivi";  return ret; }
                    else if (ret.ImgType.Contains("F5X1-"))
                    { ret.ModelName = "OGP-rect5x1"; return ret; }
                    else if (ret.ImgType.Contains("F2X1-"))
                    { }
                    else if (ret.ImgType.Contains("A10-"))
                    { }
                }
                else
                {
                    if (ret.ImgType.Contains("SIX-"))
                    { }
                    else if (ret.ImgType.Contains("IIVI-"))
                    { ret.ModelName = "OGP-sm-iivi"; return ret; }
                    else if (ret.ImgType.Contains("F5X1-"))
                    { ret.ModelName = "OGP-small5x1"; return ret; }
                    else if (ret.ImgType.Contains("F2X1-"))
                    { }
                    else if (ret.ImgType.Contains("A10-"))
                    { }
                }

                return ret;
            }
            else
            {
                return PhysicalDetectSM(f1);
            }
        }

        public static ImageTypeDetect GetPictureRev4Train(Net vcselTypeNet, string f1, string f2, string f3)
        {
            var ret = GetVCSELTypeWithDirect(vcselTypeNet, f1, f2, f3);
            if (!string.IsNullOrEmpty(ret.ImgType))
            {
                if (ret.ImgType.Contains("-UP") || ret.ImgType.Contains("-DW"))
                {
                    if (ret.ImgType.Contains("SIX-"))
                    { ret.ModelName = "OGP-circle2168"; return ret; }
                    else if (ret.ImgType.Contains("IIVI-"))
                    { ret.ModelName = "OGP-iivi"; return ret; }
                    else if (ret.ImgType.Contains("F5X1-"))
                    { ret.ModelName = "OGP-rect5x1"; return ret; }
                    else if (ret.ImgType.Contains("F2X1-"))
                    { ret.ModelName = "OGP-rect2x1"; return ret; }
                    else if (ret.ImgType.Contains("A10-"))
                    { ret.ModelName = "OGP-A10G"; return ret; }
                }
                else
                {
                    if (ret.ImgType.Contains("SIX-"))
                    { }
                    else if (ret.ImgType.Contains("IIVI-"))
                    { ret.ModelName = "OGP-sm-iivi"; return ret; }
                    else if (ret.ImgType.Contains("F5X1-"))
                    { ret.ModelName = "OGP-small5x1"; return ret; }
                    else if (ret.ImgType.Contains("F2X1-"))
                    { }
                    else if (ret.ImgType.Contains("A10-"))
                    { }
                }

                return ret;
            }
            else
            {
                return PhysicalDetect4Train(f1);
            }
        }

        public static ImageTypeDetect GetPictureRev4Product(Net vcselTypeNet, string f1, string f2, string f3, bool fixangle = false)
        {
            var ret = GetVCSELTypeWithDirect(vcselTypeNet, f1, f2, f3);
            if (!string.IsNullOrEmpty(ret.ImgType))
            {
                if (ret.ImgType.Contains("-UP") || ret.ImgType.Contains("-DW"))
                {
                    if (ret.ImgType.Contains("SIX-"))
                    { ret.ModelName = "OGP-circle2168"; return ret; }
                    else if (ret.ImgType.Contains("IIVI-"))
                    { ret.ModelName = "OGP-iivi"; return ret; }
                    else if (ret.ImgType.Contains("F5X1-"))
                    { ret.ModelName = "OGP-rect5x1"; return ret; }
                    else if (ret.ImgType.Contains("F2X1-"))
                    { ret.ModelName = "OGP-rect2x1"; return ret; }
                    else if (ret.ImgType.Contains("A10-"))
                    { ret.ModelName = "OGP-A10G"; return ret; }
                }
                else
                {
                    if (ret.ImgType.Contains("SIX-"))
                    { }
                    else if (ret.ImgType.Contains("IIVI-"))
                    { ret.ModelName = "OGP-sm-iivi"; return ret; }
                    else if (ret.ImgType.Contains("F5X1-"))
                    { ret.ModelName = "OGP-small5x1"; return ret; }
                    else if (ret.ImgType.Contains("F2X1-"))
                    { }
                    else if (ret.ImgType.Contains("A10-"))
                    { }
                }

                return ret;
            }
            else
            {
                return PhysicalDetect4Product(f1,fixangle);
            }
        }

        public static ImageTypeDetect GetPicture4inchRev(Net vcselTypeNet, string f1, string f2, string f3, bool fixangle = false)
        {
            var ret = GetVCSELTypeWithDirect(vcselTypeNet, f1, f2, f3);
            if (!string.IsNullOrEmpty(ret.ImgType))
            {
                if (ret.ImgType.Contains("-UP") || ret.ImgType.Contains("-DW"))
                {
                    if (ret.ImgType.Contains("SIX-"))
                    { ret.ModelName = "OGP-circle2168"; return ret; }
                    else if (ret.ImgType.Contains("IIVI-"))
                    { ret.ModelName = "OGP-iivi"; return ret; }
                    else if (ret.ImgType.Contains("F5X1-"))
                    { ret.ModelName = "OGP-rect5x1"; return ret; }
                    else if (ret.ImgType.Contains("F2X1-"))
                    { ret.ModelName = "OGP-rect2x1"; return ret; }
                    else if (ret.ImgType.Contains("A10-"))
                    { ret.ModelName = "OGP-A10G"; return ret; }
                }
                else
                {
                    if (ret.ImgType.Contains("SIX-"))
                    { }
                    else if (ret.ImgType.Contains("IIVI-"))
                    { ret.ModelName = "OGP-sm-iivi"; return ret; }
                    else if (ret.ImgType.Contains("F5X1-"))
                    { ret.ModelName = "OGP-small5x1"; return ret; }
                    else if (ret.ImgType.Contains("F2X1-"))
                    { }
                    else if (ret.ImgType.Contains("A10-"))
                    { }
                }

                return ret;
            }
            else
            {
                return PhysicalDetect4Inch(f1,fixangle);
            }
        }

        private static ImageTypeDetect PhysicalDetectSM(string imgpath)
        {
            var ret = new ImageTypeDetect();
            var turn = false;

            var iividetect = ImgOperateIIVI.DetectIIVI(imgpath, 115, 160, 3.0, 4.3, out turn);
            if (iividetect)
            { ret.ModelName = "OGP-iivi"; ret.ImgType = "IIVI-UP"; ret.ImgTurn = turn; return ret; }

            var iividetectsm = ImgOperateIIVIsm.DetectIIVIsm(imgpath, 80, 115, 3.0, 4.8);
            if (iividetectsm)
            { ret.ModelName = "OGP-sm-iivi"; ret.ImgType = "IIVI-LF"; return ret; }

            var xyrectlist = ImgOperateSmall5x1.FindSmall5x1Rect(imgpath, 18, 34, 4.5, 6.92, 5000, 50);
            if (xyrectlist.Count > 0)
            { ret.ModelName = "OGP-small5x1"; ret.ImgType = "F5X1-RT"; return ret; }

            xyrectlist = ImgOperate5x1.FindXYRect(imgpath, 25, 43, 4.5, 6.8, 8000, true, out turn);
            if (xyrectlist.Count > 0)
            { ret.ModelName = "OGP-rect5x1"; ret.ImgType = "F5X1-UP"; ret.ImgTurn = turn; return ret; }

            return ret;
        }

        public static ImageTypeDetect PhysicalDetect4Train(string imgpath, bool fixangle = false)
        {
            var ret = new ImageTypeDetect();
            var turn = false;

            var iividetectsm = ImgOperateIIVIsm.DetectIIVIsm(imgpath, 80, 115, 3.0, 4.8);
            if (iividetectsm)
            { ret.ModelName = "OGP-sm-iivi"; ret.ImgType = "IIVI-LF"; return ret; }

            var xyrectlist = ImgOperate5x1.FindXYRect(imgpath, 25, 43, 4.5, 6.8, 8000, true, out turn, fixangle);
            if (xyrectlist.Count > 0)
            { ret.ModelName = "OGP-rect5x1"; ret.ImgType = "F5X1-UP"; return ret; }

            var alen10g = ImgOperateA10G.DetectA10GRevision(imgpath);
            if (!string.IsNullOrEmpty(alen10g))
            { ret.ModelName = "OGP-A10G"; ret.ImgType = "A10-UP"; return ret; }

            var iividetect = ImgOperateIIVI.DetectIIVI(imgpath, 115, 160, 3.0, 4.3, out turn);
            if (iividetect)
            { ret.ModelName = "OGP-iivi"; ret.ImgType = "IIVI-UP"; return ret; }

            xyrectlist = ImgOperateSmall5x1.FindSmall5x1Rect(imgpath, 18, 34, 4.5, 6.92, 5000, 50);
            if (xyrectlist.Count > 0)
            { ret.ModelName = "OGP-small5x1"; ret.ImgType = "F5X1-RT"; return ret; }

            var circle2168 = ImgOperateCircle2168.Detect2168Revision(imgpath, fixangle);
            if (!string.IsNullOrEmpty(circle2168))
            { ret.ModelName = "OGP-circle2168"; ret.ImgType = "SIX-UP"; return ret; }

            xyrectlist = ImgOperate2x1.FindXYRect(imgpath, 60, 100, 2.0, 3.0);
            if (xyrectlist.Count > 0)
            { ret.ModelName = "OGP-rect2x1"; ret.ImgType = "F2X1-UP"; return ret; }

            return ret;
        }

        public static ImageTypeDetect PhysicalDetect4Product(string imgpath, bool fixangle = false)
        {
            var ret = new ImageTypeDetect();
            var turn = false;

            var xyrectlist = ImgOperate5x1.FindXYRect(imgpath, 25, 43, 4.5, 6.8, 8000, true, out turn, fixangle);
            if (xyrectlist.Count > 0)
            { ret.ModelName = "OGP-rect5x1"; ret.ImgType = "F5X1-UP"; return ret; }

            var alen10g = ImgOperateA10G.DetectA10GRevision(imgpath);
            if (!string.IsNullOrEmpty(alen10g))
            { ret.ModelName = "OGP-A10G"; ret.ImgType = "A10-UP"; return ret; }

            var iividetect = ImgOperateIIVI.DetectIIVI(imgpath, 115, 160, 3.0, 4.3, out turn);
            if (iividetect)
            { ret.ModelName = "OGP-iivi"; ret.ImgType = "IIVI-UP"; return ret; }

            xyrectlist = ImgOperateSmall5x1.FindSmall5x1Rect(imgpath, 18, 34, 4.5, 6.92, 5000, 50);
            if (xyrectlist.Count > 0)
            { ret.ModelName = "OGP-small5x1"; ret.ImgType = "F5X1-RT"; return ret; }

            var circle2168 = ImgOperateCircle2168.Detect2168Revision(imgpath, fixangle);
            if (!string.IsNullOrEmpty(circle2168))
            { ret.ModelName = "OGP-circle2168"; ret.ImgType = "SIX-UP"; return ret; }

            xyrectlist = ImgOperate2x1.FindXYRect(imgpath, 60, 100, 2.0, 3.0);
            if (xyrectlist.Count > 0)
            { ret.ModelName = "OGP-rect2x1"; ret.ImgType = "F2X1-UP"; return ret; }

            return ret;
        }
         
        public static ImageTypeDetect PhysicalDetect4Inch(string imgpath, bool fixangle = false)
        {
            var ret = new ImageTypeDetect();
            var turn = false;

            var xyrectlist = ImgOperate5x1.FindXYRect(imgpath, 25, 43, 4.5, 6.8, 8000, true, out turn, fixangle);
            if (xyrectlist.Count > 0)
            { ret.ModelName = "OGP-rect5x1"; ret.ImgType = "F5X1-UP"; return ret; }

            var alen10g = ImgOperateA10G.DetectA10GRevision(imgpath);
            if (!string.IsNullOrEmpty(alen10g))
            { ret.ModelName = "OGP-A10G"; ret.ImgType = "A10-UP"; return ret; }

            xyrectlist = ImgOperateSmall5x1.FindSmall5x1Rect(imgpath, 18, 34, 4.5, 6.92, 5000, 50);
            if (xyrectlist.Count > 0)
            { ret.ModelName = "OGP-small5x1"; ret.ImgType = "F5X1-RT"; return ret; }

            xyrectlist = ImgOperate2x1.FindXYRect(imgpath, 60, 100, 2.0, 3.0);
            if (xyrectlist.Count > 0)
            { ret.ModelName = "OGP-rect2x1"; ret.ImgType = "F2X1-UP"; return ret; }

            return ret;
        }


        public static Net GetVCSELTypeCNN(Controller ctrl)
        {
            var obj = ctrl.HttpContext.Cache.Get("VCSELTYPE_CNN");
            if (obj != null)
            { return (Net)obj; }

            var pbfile = "~/Scripts/VCSEL_CLASS.pb";
            var trainedNet = OpenCvSharp.Dnn.Net.ReadNetFromTensorflow(ctrl.Server.MapPath(pbfile));

            if (trainedNet != null)
            { ctrl.HttpContext.Cache.Insert("VCSELTYPE_CNN", trainedNet, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration); }

            return trainedNet;
        }

        public static ImageTypeDetect GetVCSELTypeSingle(Net vcselTypeNet, string fn)
        {
            var label_name = new string[] { "A10-UP","A10-RT","A10-DW","A10-LF"
            ,"F2X1-UP","F2X1-RT","F2X1-DW","F2X1-LF"
            ,"F5X1-UP","F5X1-RT","F5X1-DW","F5X1-LF"
            ,"IIVI-UP","IIVI-RT","IIVI-DW","IIVI-LF"
            ,"SIX-UP","SIX-RT","SIX-DW","SIX-LF"}.ToList();

            var SZ = 454;
            var img = Cv2.ImRead(fn, ImreadModes.Color);
            Cv2.CvtColor(img, img, ColorConversionCodes.BGR2RGB);
            Cv2.Resize(img, img, new Size(SZ, SZ));

            var fmat = new Mat();
            img.ConvertTo(fmat, MatType.CV_32F, 1.0);
            fmat = fmat / 255.0;

            var blob = CvDnn.BlobFromImage(fmat, 1.0, new Size(SZ, SZ), new Scalar(0, 0, 0), false, false);
            vcselTypeNet.SetInput(blob);
            var ret = vcselTypeNet.Forward();
            var retdump = ret.Dump();
            var clas = retdump.Split(new string[] { "[", "]", ",", " " }, StringSplitOptions.RemoveEmptyEntries);

            var idx = 0;
            var mxval = 0.0;
            var mxidx = -1;
            foreach (var c in clas)
            {
                var v = UT.O2D(c);
                if (v > mxval)
                {
                    mxval = v;
                    mxidx = idx;
                }
                idx++;
            }

            var retv = new ImageTypeDetect();
            retv.ImgType = label_name[mxidx];
            retv.Confidence = mxval * 100.0;
            return retv;
        }

        public static ImageTypeDetect GetVCSELTypeWithDirect(Net vcselTypeNet, string f1, string f2, string f3)
        {
            var ret = new ImageTypeDetect();
            var r1 = GetVCSELTypeSingle(vcselTypeNet, f1);
            var r2 = GetVCSELTypeSingle(vcselTypeNet, f2);
            var r3 = GetVCSELTypeSingle(vcselTypeNet, f3);

            var r1r2 = false;
            var r2r3 = false;
            var r1r3 = false;
            var r1r2v = r1.Confidence + r2.Confidence;
            var r1r3v = r1.Confidence + r3.Confidence;
            var r2r3v = r2.Confidence + r3.Confidence;

            if (r1.ImgType.Contains(r2.ImgType)) { r1r2 = true; }
            if (r2.ImgType.Contains(r3.ImgType)) { r2r3 = true; }
            if (r1.ImgType.Contains(r3.ImgType)) { r1r3 = true; }

            if (r1r2 && r2r3)
            { ret.ImgType = r1.ImgType; return ret; }

            if (r1r2)
            {
                ret.ImgType = r1.ImgType;
                if (r1r2v > 160.0) { return ret; }
            }
            if (r1r3)
            {
                ret.ImgType = r1.ImgType;
                if (r1r3v > 160.0) { return ret; }
            }
            if (r2r3)
            {
                ret.ImgType = r2.ImgType;
                if (r2r3v > 160.0) { return ret; }
            }

            var mc = r1.Confidence;
            ret.ImgType = r1.ImgType;
            if (r2.Confidence > mc)
            { mc = r2.Confidence; ret.ImgType = r2.ImgType; }
            if (r3.Confidence > mc)
            { mc = r3.Confidence; ret.ImgType = r3.ImgType; }
            if (mc > 90.0) { return ret; }

            ret.ImgType = "";
            return ret;
        }

        public static ImageTypeDetect GetVCSELOnlyType(Net vcselTypeNet, string f1, string f2, string f3)
        {
            var ret = new ImageTypeDetect();
            var r1 = GetVCSELTypeSingle(vcselTypeNet, f1);
            var r2 = GetVCSELTypeSingle(vcselTypeNet, f2);
            var r3 = GetVCSELTypeSingle(vcselTypeNet, f3);

            r1.ImgType = r1.ImgType.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries)[0];
            r2.ImgType = r2.ImgType.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries)[0];
            r3.ImgType = r3.ImgType.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries)[0];

            var r1r2 = false;
            var r2r3 = false;
            var r1r3 = false;
            var r1r2v = r1.Confidence + r2.Confidence;
            var r1r3v = r1.Confidence + r3.Confidence;
            var r2r3v = r2.Confidence + r3.Confidence;

            if (r1.ImgType.Contains(r2.ImgType)) { r1r2 = true; }
            if (r2.ImgType.Contains(r3.ImgType)) { r2r3 = true; }
            if (r1.ImgType.Contains(r3.ImgType)) { r1r3 = true; }

            if (r1r2 && r2r3)
            { ret.ImgType = r1.ImgType; return ret; }

            if (r1r2)
            {
                ret.ImgType = r1.ImgType;
                if (r1r2v > 160.0) { return ret; }
            }
            if (r1r3)
            {
                ret.ImgType = r1.ImgType;
                if (r1r3v > 160.0) { return ret; }
            }
            if (r2r3)
            {
                ret.ImgType = r2.ImgType;
                if (r2r3v > 160.0) { return ret; }
            }

            var mc = r1.Confidence;
            ret.ImgType = r1.ImgType;
            if (r2.Confidence > mc)
            { mc = r2.Confidence; ret.ImgType = r2.ImgType; }
            if (r3.Confidence > mc)
            { mc = r3.Confidence; ret.ImgType = r3.ImgType; }
            if (mc > 90.0) { return ret; }

            ret.ImgType = "";
            return ret;
        }

        public static ImageTypeDetect GetVCSELTypeWithDirect(Net vcselTypeNet, string f1)
        {
            var ret = new ImageTypeDetect();
            var r1 = GetVCSELTypeSingle(vcselTypeNet, f1);
            if (r1.Confidence >= 76.0)
            { ret.ImgType = r1.ImgType; }
            return ret;
        }

        public string ModelName { set; get; }
        public bool ImgTurn { set; get; }
        public double Confidence { set; get; }
        public string ImgType { set; get; }
        public ImageTypeDetect()
        {
            ModelName = "";
            ImgTurn = false;
            Confidence = 0.0;
            ImgType = "";
        }
    }
}