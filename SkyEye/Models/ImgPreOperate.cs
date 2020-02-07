using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SkyEye.Models
{
    public class ImgPreOperate
    {
        public static string FixImgAngle(string imgpath, Controller ctrl)
        {
            var ret = "";

            Mat srcimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var src = new Mat();
            Cv2.CvtColor(srcimg, src, ColorConversionCodes.BGR2GRAY);

            var blurred = new Mat();
            Cv2.GaussianBlur(src, blurred, new Size(5, 5), 0);

            var edged = new Mat();
            Cv2.Canny(blurred, edged, 50, 200, 3, true);

            var lines = Cv2.HoughLinesP(edged, 1, Math.PI / 180.0, 50, 80, 5);
            foreach (var line in lines)
            {
                var degree = Math.Atan2((line.P2.Y - line.P1.Y), (line.P2.X - line.P1.X));
                var d360 = (degree > 0 ? degree : (2 * Math.PI + degree)) * 360 / (2 * Math.PI);

                if (d360 > 20 && d360 < 340)
                { continue; }

                //Cv2.Line(srcimg, line.P1, line.P2, new Scalar(0, 255, 0), 3);
                //using (new Window("srcimg", srcimg))
                //{
                //    Cv2.WaitKey();
                //}

                if (d360 <= 1 || d360 >= 359)
                { break; }

                var center = new Point2f(srcimg.Width / 2, srcimg.Height / 2);
                var m = Cv2.GetRotationMatrix2D(center, d360, 1);
                var outxymat = new Mat();
                Cv2.WarpAffine(srcimg, outxymat, m, new Size(srcimg.Width, srcimg.Height));

                //using (new Window("edged", outxymat))
                //{
                //    Cv2.WaitKey();
                //}

                return WriteRawImg(outxymat, ctrl);
            }//end foreach

            return ret;
        }

        private static string WriteRawImg(Mat newimg, Controller ctrl)
        {
            try
            {
                var fn = Guid.NewGuid().ToString("N") + ".png";
                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\images\\" + datestring + "\\";
                if (!Directory.Exists(imgdir))
                { Directory.CreateDirectory(imgdir); }
                var wholefn = imgdir + fn;
                var bts = newimg.ToBytes();
                File.WriteAllBytes(wholefn, bts);
                return wholefn;
            }
            catch (Exception ex) { }

            return string.Empty;
        }

    }
}