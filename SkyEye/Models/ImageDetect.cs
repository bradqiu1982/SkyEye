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
    public class ImageDetect
    {
        public static ImageDetect GetPictureRevsm(Net vcselTypeNet, string f1,string f2,string f3)
        {
            var ret = GetVCSELTypeWithDirect(vcselTypeNet, f1, f2, f3);

            ret = new ImageDetect();
            var turn = false;

            var iividetect = ImgOperateIIVI.DetectIIVI(f1, 115, 160, 3.0, 4.3, out turn);
            if (iividetect)
            { ret.ModelName = "OGP-iivi"; ret.ImgType = "OGP-iivi"; ret.ImgTurn = turn; return ret; }

            var iividetectsm = ImgOperateIIVIsm.DetectIIVIsm(f1, 80, 115, 3.0, 4.8);
            if (iividetectsm)
            { ret.ModelName = "OGP-sm-iivi"; ret.ImgType = "OGP-sm-iivi"; return ret; }

            var xyrectlist = ImgOperateSmall5x1.FindSmall5x1Rect(f1, 18, 34, 4.5, 6.92, 5000, 50);
            if (xyrectlist.Count > 0)
            { ret.ModelName = "OGP-small5x1"; ret.ImgType = "OGP-small5x1"; return ret; }

            xyrectlist = ImgOperate5x1.FindXYRect(f1, 25, 43, 4.5, 6.8, 8000, true, out turn);
            if (xyrectlist.Count > 0)
            { ret.ModelName = "OGP-rect5x1"; ret.ImgType = "OGP-rect5x1"; ret.ImgTurn = turn; return ret; }

            return ret;


        }

        public static Net GetVCSELTypeCNN(Controller ctrl)
        {
            var obj = ctrl.HttpContext.Cache.Get("VCSELTYPE_CNN");
            if (obj != null)
            { return (Net)obj; }

            var trainedNet = OpenCvSharp.Dnn.Net.ReadNetFromTensorflow(ctrl.Server.MapPath("~/Scripts/VCSEL_CLASS.pb"));
            if (trainedNet != null)
            { ctrl.HttpContext.Cache.Insert("VCSELTYPE_CNN", trainedNet, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration); }

            return trainedNet;
        }

        private static ImageDetect GetVCSELType_(Net vcselTypeNet, string fn)
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

            var retv = new ImageDetect();
            retv.ImgType = label_name[mxidx];
            retv.Confidence = mxval * 100.0;
            return retv;
        }

        public static ImageDetect GetVCSELTypeWithDirect(Net vcselTypeNet, string f1, string f2, string f3)
        {
            var ret = new ImageDetect();
            var r1 = GetVCSELType_(vcselTypeNet, f1);
            var r2 = GetVCSELType_(vcselTypeNet, f2);
            var r3 = GetVCSELType_(vcselTypeNet, f3);

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

        public static ImageDetect GetVCSELOnlyType(Net vcselTypeNet, string f1, string f2, string f3)
        {
            var ret = new ImageDetect();
            var r1 = GetVCSELType_(vcselTypeNet, f1);
            var r2 = GetVCSELType_(vcselTypeNet, f2);
            var r3 = GetVCSELType_(vcselTypeNet, f3);

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

        public static ImageDetect GetVCSELTypeWithDirect(Net vcselTypeNet, string f1)
        {
            var ret = new ImageDetect();
            var r1 = GetVCSELType_(vcselTypeNet, f1);
            if (r1.Confidence >= 76.0)
            { ret.ImgType = r1.ImgType; }
            return ret;
        }


        public string ImgType { set; get; }
        public bool ImgTurn { set; get; }
        public double Confidence { set; get; }
        public string ModelName { set; get; }
        public ImageDetect()
        {
            ImgType = "";
            ImgTurn = false;
            Confidence = 0.0;
            ModelName = "";
        }
    }
}