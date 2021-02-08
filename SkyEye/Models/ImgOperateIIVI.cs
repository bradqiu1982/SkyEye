using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyEye.Models
{
    public class ImgOperateIIVI
    {
        //115,140,3.1,4.2
        public static bool DetectIIVI(string imgpath,int minrad,int maxrad,double hrminrate,double hrmaxrate)
        {
            Mat srcorgimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var detectsize = GetDetectPoint(srcorgimg);
            var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var srcgray = new Mat();
            Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);

            var circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, minrad, maxrad);
            if (circles.Count() > 0)
            {
                var ccl = circles[0];
                var rat = srcrealimg.Height / ccl.Radius;
                if (rat >= hrminrate && rat <= hrmaxrate)
                { return true; }
            }
            return false;
        }

        //115,140
        public static List<Mat> CutCharRect(string imgpath, int minrad, int maxrad, bool fixangle = false)
        {
            Mat srcorgimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            if (fixangle)
            {
                var angle = ImgPreOperate.GetAngle(imgpath);
                if (angle >= 0.7 && angle <= 359.3)
                { srcorgimg = ImgPreOperate.GetFixedAngleImg(srcorgimg, angle); }
            }

            var detectsize = GetDetectPoint(srcorgimg);
            var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var srcgray = new Mat();
            Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);

            var circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, 115, 140);

            if (circles.Count() > 0)
            {
                var ccl = circles[0];

                //Cv2.Circle(srcrealimg, (int)ccl.Center.X, (int)ccl.Center.Y, (int)ccl.Radius, new Scalar(0, 255, 0), 3);

                var halfheight = srcrealimg.Height / 2;
                if (ccl.Center.Y < halfheight)
                {
                    var outxymat = new Mat();
                    Cv2.Transpose(srcrealimg, outxymat);
                    Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                    Cv2.Transpose(outxymat, outxymat);
                    Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                    srcrealimg = outxymat;

                    srcgray = new Mat();
                    Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);
                    circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, 115, 140);
                    ccl = circles[0];
                }

                var xcoordx = (int)(ccl.Center.X + 100);
                var xcoordy = (int)(ccl.Center.Y - 53);
                var ycoordx = (int)(ccl.Center.X + 7);
                var ycoordy = (int)(ccl.Center.Y - 259);
                if (ycoordy < 0) { ycoordy = 3; }
                var markx = (int)(ccl.Center.X + 123);
                var marky = (int)(ccl.Center.Y - 125);

                var ximg = srcrealimg.SubMat(new Rect(xcoordx, xcoordy, 90, 54));
                var yimg = srcrealimg.SubMat(new Rect(ycoordx, ycoordy, 54, 90));
                {
                    var outxymat = new Mat();
                    Cv2.Transpose(yimg, outxymat);
                    Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                    yimg = outxymat;
                }

                var combinimg = new Mat();
                Cv2.HConcat(ximg, yimg, combinimg);

                var markgrey = srcgray.SubMat(new Rect(markx, marky, 60, 60));
                var markmat = new Mat();
                Cv2.AdaptiveThreshold(markgrey, markmat, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

                ximg = GetEnhanceEdge(ximg);
                yimg = GetEnhanceEdge(yimg);

                var charlist = new List<Mat>();
                charlist.Add(combinimg);
                charlist.Add(markmat);
                charlist.AddRange(GetCharMats(ximg, 1));
                charlist.Add(markmat);
                charlist.AddRange(GetCharMats(yimg, 2));

                return charlist;
            }

            return new List<Mat>();
       }

        public static List<Mat> GetCharMats(Mat xymat, int id)
        {
            var charlist = new List<Mat>();

            var kaze = KAZE.Create();
            var kazeDescriptors = new Mat();
            KeyPoint[] kazeKeyPoints = null;
            kaze.DetectAndCompute(xymat, null, out kazeKeyPoints, kazeDescriptors);

            var wptlist = new List<KeyPoint>();
            for (var idx = 20; idx < xymat.Width;)
            {
                var yhlist = new List<double>();
                var wlist = new List<KeyPoint>();
                foreach (var pt in kazeKeyPoints)
                {
                    if (pt.Pt.X >= (idx - 20) && pt.Pt.X < idx)
                    {
                        if (pt.Pt.Y + 6 > xymat.Height)
                        { continue; }

                        wlist.Add(pt);
                        yhlist.Add(pt.Pt.Y);
                    }
                }

                if (wlist.Count > 10 && (yhlist.Max() - yhlist.Min()) > 0.3 * xymat.Height)
                { wptlist.AddRange(wlist); }
                idx = idx + 20;
            }

            var xlist = new List<double>();
            var ylist = new List<double>();
            foreach (var pt in wptlist)
            {
                xlist.Add(pt.Pt.X);
                ylist.Add(pt.Pt.Y);
            }

            //var dstKaze = new Mat();
            //Cv2.DrawKeypoints(xymat, wptlist, dstKaze);
            //using (new Window("dstKazexx" + id, dstKaze))
            //{
            //    Cv2.WaitKey();
            //}


            var h0 = (int)ylist.Min() - 3;
            if (h0 < 0) { h0 = 0; }
            var h1 = (int)ylist.Max() + 3;
            if (h1 - h0 > 158) { h1 = h0 + 158; }
            if (h1 > xymat.Height) { h1 = xymat.Height - 1; }


            var xmax = (int)xlist.Max();

            var start = xmax - 150; var end = xmax - 30;
            var split2x = GetSplitX(xymat, start, end, h0, h1);

            start = xmax - 140; end = (xmax - 250) < 0 ? 0 : (xmax - 250);
            if (split2x != -1)
            { start = split2x - 150; end = split2x - 40; }
            var split1x = GetSplitX(xymat, start, end, h0, h1);

            start = xmax - 260; end = (xmax - 380) < 0 ? 0 : (xmax - 380);
            if (split1x != -1)
            { start = split1x - 150; end = split1x - 40; }
            var split0x = GetSplitX(xymat, start, end, h0, h1);

            if (split1x != -1 && split2x != -1 && split0x != -1)
            {
                charlist.Add(xymat.SubMat(h0, h1, split0x, split1x));
                charlist.Add(xymat.SubMat(h0, h1, split1x, split2x));
                charlist.Add(xymat.SubMat(h0, h1, split2x, xymat.Width - 3));
            }
            else if (split1x != 0 && split2x != 0)
            {
                var fontwd = xymat.Width - split2x;
                var x0 = split1x - fontwd;
                if (x0 < 0) { x0 = 0; }
                charlist.Add(xymat.SubMat(h0, h1, x0, split1x));
                charlist.Add(xymat.SubMat(h0, h1, split1x, split2x));
                charlist.Add(xymat.SubMat(h0, h1, split2x, xymat.Width - 3));
            }
            else if (split2x != 0)
            {
                var fontwd = xymat.Width - split2x;
                var x1 = split2x - fontwd;
                var x0 = split2x - 2 * fontwd;
                if (x0 < 0) { x0 = 0; }

                charlist.Add(xymat.SubMat(h0, h1, x0, x1));
                charlist.Add(xymat.SubMat(h0, h1, x1, split2x));
                charlist.Add(xymat.SubMat(h0, h1, split2x, xymat.Width - 3));
            }
            else
            {
                var x0 = xymat.Width - 304;
                if (x0 < 0) { x0 = 0; }
                charlist.Add(xymat.SubMat(h0, h1, x0, xymat.Width - 204));
                charlist.Add(xymat.SubMat(h0, h1, xymat.Width - 204, xymat.Width - 103));
                charlist.Add(xymat.SubMat(h0, h1, xymat.Width - 103, xymat.Width - 3));
            }
            return charlist;
        }

        public static int GetSplitX(Mat xymat, int snapstart, int snapend, int h0, int h1)
        {
            var ret = -1;
            var tm = 0;
            for (var sidx = snapend; sidx > snapstart;)
            {
                var snapmat = xymat.SubMat(h0, h1, sidx, sidx + 2);
                var cnt = snapmat.CountNonZero();
                if (cnt < 3)
                {
                    if (ret != -1 && tm == 1)
                    { return sidx; }
                    else
                    { ret = sidx; tm = 1; }
                }
                else
                { tm = 0; }
                sidx = sidx - 2;
            }
            return -1;
        }

        //public static int GetSplitX_old(Mat xymat, int snapstart, int snapend, int h0, int h1)
        //{
        //    bool hassplit = false;
        //    var wtob = 0;
        //    var btow = 0;
        //    var previouscolor = 1;

        //    for (var sidx = snapend; sidx > snapstart;)
        //    {
        //        var snapmat = xymat.SubMat(h0, h1, sidx, sidx + 2);
        //        var cnt = snapmat.CountNonZero();
        //        if (cnt < 2)
        //        {
        //            hassplit = true;
        //            if (previouscolor == 1)
        //            {
        //                previouscolor = 0;
        //                wtob = sidx;
        //            }
        //            previouscolor = 0;
        //        }
        //        else
        //        {
        //            if (previouscolor == 0)
        //            {
        //                btow = sidx;
        //                break;
        //            }
        //            previouscolor = 1;
        //        }

        //        sidx = sidx - 2;
        //    }

        //    if (hassplit && wtob != 0 && btow != 0)
        //    { return (wtob + btow) / 2; }

        //    return 0;
        //}

        public static Mat GetEnhanceEdge(Mat xymat)
        {
            var xyenhance4x = new Mat();
            Cv2.Resize(xymat, xyenhance4x, new Size(xymat.Width * 4, xymat.Height * 4));
            Cv2.DetailEnhance(xyenhance4x, xyenhance4x);

            var xyenhgray = new Mat();
            var denoisemat = new Mat();
            Cv2.FastNlMeansDenoisingColored(xyenhance4x, denoisemat, 10, 10, 7, 21);
            Cv2.CvtColor(denoisemat, xyenhgray, ColorConversionCodes.BGR2GRAY);

            var blurred = new Mat();
            Cv2.GaussianBlur(xyenhgray, blurred, new Size(3, 3), 0);

            var edged = new Mat();
            Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

            return edged;
        }

        private static List<List<double>> GetDetectPoint(Mat mat)
        {
            var ret = new List<List<double>>();
            var kaze = KAZE.Create();
            var kazeDescriptors = new Mat();
            KeyPoint[] kazeKeyPoints = null;
            kaze.DetectAndCompute(mat, null, out kazeKeyPoints, kazeDescriptors);

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

                var xlist = new List<double>();
                var ylist = new List<double>();
                foreach (var pt in wptlist)
                {
                    xlist.Add(pt.Pt.X);
                    ylist.Add(pt.Pt.Y);
                }
                ret.Add(xlist);
                ret.Add(ylist);

                return ret;
            }

    }
}