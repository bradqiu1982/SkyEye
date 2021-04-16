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

        public static double GetAngle(string imgpath)
        {
            Mat srcimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var src = new Mat();
            Cv2.CvtColor(srcimg, src, ColorConversionCodes.BGR2GRAY);

            var blurred = new Mat();
            Cv2.GaussianBlur(src, blurred, new Size(5, 5), 0);

            var edged = new Mat();
            Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

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

                if (d360 <= 4 || d360 >= 356)
                { return d360; }
            }
            return 0;
        }

        public static Mat GetFixedAngleImg(Mat src, double angle)
        {
            var center = new Point2f(src.Width / 2, src.Height / 2);
            var m = Cv2.GetRotationMatrix2D(center, angle, 1);
            var outxymat = new Mat();
            Cv2.WarpAffine(src, outxymat, m, new Size(src.Width, src.Height));
            return outxymat;
        }

        public static string FixImgAngle(string imgpath, Controller ctrl)
        {
            var ret = "";

            Mat srcimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var src = new Mat();
            Cv2.CvtColor(srcimg, src, ColorConversionCodes.BGR2GRAY);

            var blurred = new Mat();
            Cv2.GaussianBlur(src, blurred, new Size(5, 5), 0);

            var edged = new Mat();
            Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

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

        public static List<List<double>> GetDetectPoint(Mat mat)
        {
            var ret = new List<List<double>>();
            var xyenhance = new Mat();
            Cv2.DetailEnhance(mat, xyenhance);
            var kaze = KAZE.Create();
            var kazeDescriptors = new Mat();
            KeyPoint[] kazeKeyPoints = null;
            kaze.DetectAndCompute(xyenhance, null, out kazeKeyPoints, kazeDescriptors);

            var wptlist = new List<KeyPoint>();
            for (var idx = 20; idx < mat.Width;)
            {
                var yhlist = new List<double>();
                var wlist = new List<KeyPoint>();
                foreach (var pt in kazeKeyPoints)
                {
                    if (pt.Pt.X >= (idx - 20) && pt.Pt.X < idx)
                    {
                        wlist.Add(pt);
                        yhlist.Add(pt.Pt.Y);
                    }
                }

                if (wlist.Count > 10 && (yhlist.Max() - yhlist.Min()) > 0.3 * mat.Height)
                { wptlist.AddRange(wlist); }
                idx = idx + 20;
            }

            var hptlist = new List<KeyPoint>();
            for (var idx = 20; idx < mat.Height;)
            {
                var xwlist = new List<double>();
                var wlist = new List<KeyPoint>();
                foreach (var pt in wptlist)
                {
                    if (pt.Pt.Y >= (idx - 20) && pt.Pt.Y < idx)
                    {
                        wlist.Add(pt);
                        xwlist.Add(pt.Pt.X);
                    }
                }

                if (wlist.Count >= 2 && (xwlist.Max() - xwlist.Min()) > 0.3 * mat.Width)
                { hptlist.AddRange(wlist); }
                idx = idx + 20;
            }

            var xlist = new List<double>();
            var ylist = new List<double>();
            foreach (var pt in hptlist)
            {
                xlist.Add(pt.Pt.X);
                ylist.Add(pt.Pt.Y);
            }
            ret.Add(xlist);
            ret.Add(ylist);

            //var dstKaze = new Mat();
            //Cv2.DrawKeypoints(mat, wptlist, dstKaze);

            //using (new Window("dstKazexx", dstKaze))
            //{
            //    Cv2.WaitKey();
            //}

            return ret;
        }

        public static Mat DNN_RESIZE(Mat src)
        {
            //var sr = new DnnSuperResImpl("edsr", 4);
            //sr.ReadModel("~/Scripts/EDSR_x4.pb");
            //var coordmat4x = new Mat();
            //sr.Upsample(src, coordmat4x);
            return src;
        }

        public static Mat Sharp(Mat xymat)
        {
            var sharpimg = new Mat();
            Cv2.GaussianBlur(xymat, sharpimg, new Size(0, 0), 3);
            Cv2.AddWeighted(xymat, 2.0, sharpimg, -0.4, 0, sharpimg);
            return sharpimg;
        }

        public static Mat BrightGammCorrect(Mat yimg)
        {
            var cols = yimg.Cols;
            var rows = yimg.Rows;
            var yimgidx = yimg.GetGenericIndexer<Vec3b>();
            for (var y = 0; y < rows; y++)
            {
                for (var x = 0; x < cols; x++)
                {
                    Vec3b color = yimgidx[y, x];
                    var gamm = 1.0 / 2.2;
                    var pixval = Convert.ToDouble(color.Item0);
                    var nval = Math.Pow(pixval / 255.0, gamm) * 255.0;
                    if (nval > 255.0) { nval = 255.0; }
                    color.Item0 = Convert.ToByte(nval);

                    pixval = Convert.ToDouble(color.Item1);
                    nval = Math.Pow(pixval / 255.0, gamm) * 255.0;
                    if (nval > 255.0) { nval = 255.0; }
                    color.Item1 = Convert.ToByte(nval);

                    pixval = Convert.ToDouble(color.Item2);
                    nval = Math.Pow(pixval / 255.0, gamm) * 255.0;
                    if (nval > 255.0) { nval = 255.0; }
                    color.Item2 = Convert.ToByte(nval);
                    yimgidx[y, x] = color;
                }
            }

            return yimg;
        }

        public static List<List<int>> GetImageBoundPointX(Mat srccolor)
        {
            var sharpimg = new Mat();
            Cv2.GaussianBlur(srccolor, sharpimg, new Size(0, 0), 3);
            Cv2.AddWeighted(srccolor, 2.0, sharpimg, -0.4, 0, sharpimg);

            var srcgray = new Mat();
            Cv2.CvtColor(sharpimg, srcgray, ColorConversionCodes.BGR2GRAY);
            var blurred = new Mat();
            Cv2.GaussianBlur(srcgray, blurred, new Size(3, 3), 0);
            var edged = new Mat();
            Cv2.Canny(blurred, edged, 50, 200, 3, false);

            var high = edged.Height;
            var width = edged.Width;

            var lowy = (int)(0.2 * high);
            var highy = (int)(0.8 * high);
            var lowx = (int)(0.2 * width);
            var highx = (int)(0.8 * width);

            var xlist = new List<int>();
            var ylist = new List<int>();
            for (var x = 0; x < width - 2; x++)
            {
                var submat = edged.SubMat(lowy, highy, x, x + 2);
                var nonzero = submat.CountNonZero();
                if (nonzero > 20)
                { xlist.Add(x); }
            }

            for (var y = 0; y < high - 2; y++)
            {
                var submat = edged.SubMat(y, y + 2, lowx, highx);
                var nonzero = submat.CountNonZero();
                if (nonzero > 20)
                { ylist.Add(y); }
            }

            var ret = new List<List<int>>();
            ret.Add(xlist);
            ret.Add(ylist);
            return ret;
        }
    }
}