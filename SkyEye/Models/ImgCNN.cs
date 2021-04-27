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

    public class ImgCNN
    {
        //"~/Scripts/font_ogpsm5x1_450.pb"
        public static int CNN_GetCharacterVAL(Mat cmat, Net net)
        {
            var cmatcp = new Mat();
            cmat.CopyTo(cmatcp);
            Cv2.Resize(cmatcp, cmatcp, new Size(50, 50));
            Cv2.CvtColor(cmatcp, cmatcp, ColorConversionCodes.GRAY2RGB);

            var fmat = new Mat();
            cmatcp.ConvertTo(fmat, MatType.CV_32F, 1.0);
            fmat = fmat / 255.0;

            var blob = CvDnn.BlobFromImage(fmat, 1.0, new Size(50, 50), new Scalar(0, 0, 0), false, false);

            net.SetInput(blob);
            var ret = net.Forward();

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

            return (mxidx + 48);
        }

        public static Net GetCharacterNetByType(string caprev, Controller ctrl)
        {
            var obj = ctrl.HttpContext.Cache.Get(caprev + "_CNN");
            if (obj != null)
            { return (Net)obj; }

            var pbfile = "";
            if (string.Compare(caprev, "OGP-rect5x1", true) == 0)
            { pbfile = "~/Scripts/font_ogp5x1_5000.pb"; }
            else if (string.Compare(caprev, "OGP-rect2x1", true) == 0)
            { pbfile = "~/Scripts/font_ogp2x1_4500.pb"; }
            else if (string.Compare(caprev, "OGP-circle2168", true) == 0)
            { pbfile = "~/Scripts/font_ogp2168_850.pb"; }
            else if (string.Compare(caprev, "OGP-A10G", true) == 0)
            { pbfile = "~/Scripts/font_ogpa10g_750.pb"; }
            else if (string.Compare(caprev, "OGP-iivi", true) == 0)
            { pbfile = "~/Scripts/font_ogpiivi_480.pb"; }
            else if (string.Compare(caprev, "OGP-small5x1", true) == 0)
            { pbfile = "~/Scripts/font_ogpsm5x1_450.pb"; }
            else if (string.Compare(caprev, "OGP-sm-iivi", true) == 0)
            { pbfile = "~/Scripts/font_ogpsmiivi_600.pb"; }

            if (string.IsNullOrEmpty(pbfile))
            { pbfile = "~/Scripts/font_ogp5x1_5000.pb"; }

            var trainedNet = OpenCvSharp.Dnn.Net.ReadNetFromTensorflow(ctrl.Server.MapPath(pbfile));

            if (trainedNet != null)
            { ctrl.HttpContext.Cache.Insert(caprev + "_CNN", trainedNet, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration); }

            return trainedNet;
        }

    }
}