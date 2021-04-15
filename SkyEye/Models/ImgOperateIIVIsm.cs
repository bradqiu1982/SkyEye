using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyEye.Models
{
    public class ImgOperateIIVIsm
    {
        //80,115,3.0,4.3
        public static bool DetectIIVIsm(string imgpath, int minrad, int maxrad, double hrminrate, double hrmaxrate)
        {
            Mat srcorgimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var detectsize = GetDetectPoint(srcorgimg);
            var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            {
                var outxymat = new Mat();
                Cv2.Transpose(srcrealimg, outxymat);
                Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                srcrealimg = outxymat;
            }

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

        //80,115
        public static List<Mat> CutCharRect(string imgpath, int minrad, int maxrad, bool fixangle = false)
        {
            Mat srcorgimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var detectsize = GetDetectPoint(srcorgimg);
            var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            {
                var outxymat = new Mat();
                Cv2.Transpose(srcrealimg, outxymat);
                Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                srcrealimg = outxymat;
            }


            var srcgray = new Mat();
            Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);

            var circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, minrad, maxrad);

            if (circles.Count() > 0)
            {
                var ccl = circles[0];

                //Cv2.Circle(srcrealimg, (int)ccl.Center.X, (int)ccl.Center.Y, (int)ccl.Radius, new Scalar(0, 255, 0), 3);
                //using (new Window("srcimg2", srcrealimg))
                //{
                //    Cv2.WaitKey();
                //}

                var rat = srcrealimg.Height / ccl.Radius;

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
                    circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, minrad, maxrad);
                    ccl = circles[0];
                }


                var xcoordx = (int)(ccl.Center.X + 70);
                var xcoordy = (int)(ccl.Center.Y + 22);
                var ycoordx = (int)(ccl.Center.X + 6);
                var ycoordy = (int)(ccl.Center.Y - 212);

                var markx = (int)(ccl.Center.X + 100);
                var marky = (int)(ccl.Center.Y - 30);

                if (ycoordy < 0) { ycoordy = 3; }

                var ximg = srcrealimg.SubMat(new Rect(xcoordx, xcoordy, 82, 40));


                var yimg = srcrealimg.SubMat(new Rect(ycoordx, ycoordy, 40, 98));
                {
                    var outxymat = new Mat();
                    Cv2.Transpose(yimg, outxymat);
                    Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                    yimg = outxymat;
                }

                var combinimg = new Mat();
                Cv2.HConcat(ximg, yimg, combinimg);

                var markgrey = srcgray.SubMat(new Rect(markx, marky, 40, 40));
                var markmat = new Mat();
                Cv2.AdaptiveThreshold(markgrey, markmat, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);


                ximg = GetEnhanceEdge(ximg);
                yimg = GetEnhanceEdge(yimg);

                var charlist = new List<Mat>();
                charlist.Add(combinimg);
                charlist.Add(markmat);
                charlist.AddRange(GetCharMatsSM(ximg, 1));
                charlist.Add(markmat);
                charlist.AddRange(GetCharMatsSM(yimg, 2));
                return charlist;
            }

            return new List<Mat>();
        }

        private static Mat GetEnhanceEdge(Mat xymat)
        {
            var sharpimg = new Mat();
            Cv2.GaussianBlur(xymat, sharpimg, new Size(0, 0), 3);
            Cv2.AddWeighted(xymat, 2.0, sharpimg, -0.4, 0, sharpimg);

            var xyenhance4x = new Mat();
            Cv2.Resize(sharpimg, xyenhance4x, new Size(xymat.Width * 5, xymat.Height * 5));
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

        private static List<Mat> GetCharMatsSM(Mat xymat, int id)
        {
            var charlist = new List<Mat>();
            var ylowhigh = DetectMatYHighLow(xymat);
            var ylow = ylowhigh[0];
            var yhigh = ylowhigh[1];
            var splitxlist = GetMatSplit(xymat, ylow, yhigh);

            var mat1 = xymat.SubMat(ylow, yhigh, splitxlist[0], splitxlist[1]);

            var mat2 = xymat.SubMat(ylow, yhigh, splitxlist[1], splitxlist[2]);
            var mat3 = xymat.SubMat(ylow, yhigh, splitxlist[2], splitxlist[3]);
            var ret = new List<Mat>();
            ret.Add(mat1); ret.Add(mat2); ret.Add(mat3);
            return ret;
        }

        private static int GetMatSplit_(Mat img, int start, int ylow, int yhigh)
        {
            var getfont = false;
            var sum = 0;
            var end = start - 110;
            if (end < 0) { end = 2; }

            for (var idx = start; idx > end;)
            {
                var submat = img.SubMat(ylow, yhigh, idx - 2, idx);

                var zcnt = submat.CountNonZero();
                if (getfont && zcnt < 2)
                {
                    return (idx - 3);
                }

                if (zcnt < 2)
                { sum = 0; }
                else
                {
                    sum++;
                    if (sum > 20)
                    {
                        getfont = true;
                    }
                }

                idx = idx - 2;
            }
            return end;
        }

        private static int GetMatEnd(Mat img, int ylow, int yhigh)
        {
            var start = img.Width;
            var end = start - 100;
            var sum = 0;
            var es = 0;
            for (var idx = start; idx > end;)
            {
                var submat = img.SubMat(ylow, yhigh, idx - 2, idx);
                var zcnt = submat.CountNonZero();
                if (zcnt < 2)
                { sum = 0; }
                else
                {
                    sum++;
                    if (sum > 10)
                    {
                        es = idx;
                        break;
                    }
                }
                idx = idx - 2;
            }

            if (es == 0)
            { start = img.Width - 50; }
            else
            { start = es; }
            end = img.Width - 10;

            for (var idx = start; idx < end;)
            {
                var submat = img.SubMat(ylow, yhigh, idx, idx + 2);
                var zcnt = submat.CountNonZero();
                if (zcnt < 2)
                { return idx + 2; }

                idx = idx + 2;
            }

            return img.Width - 5;
        }

        private static List<int> GetMatSplit(Mat img, int ylow, int yhigh)
        {
            var imgend = GetMatEnd(img, ylow, yhigh);
            var splitx1 = GetMatSplit_(img, imgend - 1, ylow, yhigh);
            var splitx2 = GetMatSplit_(img, splitx1 - 1, ylow, yhigh);
            var splitx3 = GetMatSplit_(img, splitx2 - 1, ylow, yhigh);
            var ret = new List<int>();
            ret.Add(splitx3);
            ret.Add(splitx2);
            ret.Add(splitx1);
            ret.Add(imgend);
            return ret;
        }

        private static List<int> DetectMatYHighLow(Mat img)
        {
            var ylow = 0;
            var yhigh = 0;

            var xlow = (int)(img.Width * 0.25);
            var xhigh = (int)(img.Width * 0.75);
            var midy = (int)(img.Height * 0.5);
            var matheigh = img.Height;

            for (var idx = midy; idx > 4;)
            {
                var submat = img.SubMat(idx, idx + 2, xlow, xhigh);
                var zcnt = submat.CountNonZero();
                if (zcnt < 3)
                { ylow = idx - 1; break; }
                idx = idx - 2;
            }

            if (ylow == 0)
            { ylow = 4; }

            for (var idx = midy; idx < matheigh - 4;)
            {
                var submat = img.SubMat(idx, idx + 2, xlow, xhigh);
                var zcnt = submat.CountNonZero();
                if (zcnt < 3)
                { yhigh = idx - 1; break; }

                idx = idx + 2;
            }

            if (yhigh == 0)
            { yhigh = matheigh - 4; }

            var ret = new List<int>();
            ret.Add(ylow); ret.Add(yhigh);
            return ret;
        }

        private static List<List<double>> GetDetectPoint(Mat mat)
        {
            var xyenhance = new Mat();
            Cv2.DetailEnhance(mat, xyenhance);

            var ret = new List<List<double>>();
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