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

    public class ImgFontCNN
    {
        private static readonly object lockobj = new object();

        //"~/Scripts/font_ogpsm5x1_450.pb"
        public static int CNN_GetCharacterVAL(Mat cmat, Net net,out double rate)
        {
            var cmatcp = new Mat();
            Cv2.CvtColor(cmat, cmatcp, ColorConversionCodes.GRAY2RGB);
            Cv2.Resize(cmatcp, cmatcp, new Size(224, 224));

            var fmat = new Mat();
            cmatcp.ConvertTo(fmat, MatType.CV_32F, 1.0);
            fmat = fmat / 255.0;

            var blob = CvDnn.BlobFromImage(fmat, 1.0, new Size(224, 224), new Scalar(0, 0, 0), false, false);

            lock (ImgFontCNN.lockobj) {

                net.SetInput(blob);
                var ret = net.Forward();

                var retdump = ret.Dump(FormatType.Python);

                //if (retdump.Contains("nan") || retdump.Contains("NAN"))
                //{
                //    rate = 0;
                //    return -1;
                //}

                var clas = retdump.Split(new string[] { "[", "]", "\n", ",", " " }, StringSplitOptions.RemoveEmptyEntries);
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

                rate = mxval * 100;
                return (mxidx + 48);
            }//end lock
        }

        public static Net GetCharacterNetByType(string caprev, Controller ctrl)
        {
            var obj = ctrl.HttpContext.Cache.Get(caprev + "_CNN");
            if (obj != null)
            { return (Net)obj; }

            var pbfile = "";
            if (string.Compare(caprev, "OGP-rect5x1", true) == 0)
            { pbfile = "~/Scripts/font_ogp5x1_8390_6.pb"; }
            else if (string.Compare(caprev, "OGP-rect2x1", true) == 0)
            { pbfile = "~/Scripts/font_ogp2x1_5330_8.pb"; }
            else if (string.Compare(caprev, "OGP-circle2168", true) == 0)
            { pbfile = "~/Scripts/font_ogp2168_1160_10.pb"; }
            else if (string.Compare(caprev, "OGP-A10G", true) == 0)
            { pbfile = "~/Scripts/font_ogpa10g_810_14.pb"; }
            else if (string.Compare(caprev, "OGP-iivi", true) == 0)
            { pbfile = "~/Scripts/font_ogpiivi_620_16.pb"; }
            else if (string.Compare(caprev, "OGP-small5x1", true) == 0)
            { pbfile = "~/Scripts/font_ogpsm5x1_503_16.pb"; }
            else if (string.Compare(caprev, "OGP-sm-iivi", true) == 0)
            { pbfile = "~/Scripts/font_ogpsmiivi_1113_14.pb"; }

            if (string.IsNullOrEmpty(pbfile))
            { pbfile = "~/Scripts/font_ogp5x1_8390_6.pb"; }

            var trainedNet = OpenCvSharp.Dnn.Net.ReadNetFromTensorflow(ctrl.Server.MapPath(pbfile));

            if (trainedNet != null)
            { ctrl.HttpContext.Cache.Insert(caprev + "_CNN", trainedNet, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration); }

            return trainedNet;
        }

    }
}