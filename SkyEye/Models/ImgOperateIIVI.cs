using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyEye.Models
{
    public class ImgOperateIIVI
    {
        //115,160,3.1,4.2
        public static bool DetectIIVI(string imgpath,int minrad,int maxrad,double hrminrate,double hrmaxrate, out bool turn)
        {
            Mat srcorgimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var detectsize = ImgPreOperate.GetImageBoundPointX(srcorgimg);
            var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            turn = false;

            var srcgray = new Mat();
            Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);

            var circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, minrad, maxrad);
            if (circles.Count() > 0)
            {
                var ccl = circles[0];
                if (ccl.Radius >= 135 && srcrealimg.Width > srcrealimg.Height)
                {
                    var outxymat = new Mat();
                    Cv2.Transpose(srcrealimg, outxymat);
                    Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                    srcrealimg = outxymat;
                    turn = true;
                }

                var rat = srcrealimg.Height / ccl.Radius;
                if (rat >= hrminrate && rat <= hrmaxrate)
                { return true; }
            }

            turn = false;
            return false;
        }

        //115,140
        public static List<Mat> CutCharRect(string imgpath,ImageDetect detect, int minrad, int maxrad, bool fixangle = false)
        {
            Mat srcorgimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            if (fixangle)
            {
                var angle = ImgPreOperate.GetAngle(imgpath);
                if (angle >= 0.7 && angle <= 359.3)
                { srcorgimg = ImgPreOperate.GetFixedAngleImg(srcorgimg, angle); }
            }

            var detectsize = ImgPreOperate.GetImageBoundPointX(srcorgimg);
            var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            //if (turn)
            //{
            //    var outxymat = new Mat();
            //    Cv2.Transpose(srcrealimg, outxymat);
            //    Cv2.Flip(outxymat, outxymat, FlipMode.Y);
            //    srcrealimg = outxymat;
            //    var w = (int)((460.0 / (double)srcrealimg.Height) * srcrealimg.Width);
            //    srcrealimg = srcrealimg.Resize(new Size(w, 460));
            //}

            var srcgray = new Mat();
            Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);

            var circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, minrad, maxrad);

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

                var ximg = srcrealimg.SubMat(new Rect(xcoordx, xcoordy, 92, 54));
                var yimg = srcrealimg.SubMat(new Rect(ycoordx, ycoordy, 54, 92));

                if (!DetectXCoord(ximg))
                {
                    xcoordx = (int)(ccl.Center.X + 92);
                    xcoordy = (int)(ccl.Center.Y + 22);
                    ximg = srcrealimg.SubMat(new Rect(xcoordx, xcoordy, 100, 54));

                    ycoordx = (int)(ccl.Center.X + 9);
                    ycoordy = (int)(ccl.Center.Y - 278);
                    if (ycoordy < 0) { ycoordy = 0; }

                    yimg = srcrealimg.SubMat(new Rect(ycoordx, ycoordy, 54, 100));
                }

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


        public static bool DetectXCoord(Mat xymat)
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

            var lowx = (int)(0.55 * edged.Width);
            var highx = (int)(0.85 * edged.Width);
            var submat = edged.SubMat(0, edged.Height, lowx, highx);
            var nonzerocnt = submat.CountNonZero();
            if (nonzerocnt < 1000)
            { return false; }
            return true;
        }

        public static List<Mat> GetCharMats(Mat xymat, int id)
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

        public static int GetMatSplit_(Mat img, int start, int ylow, int yhigh)
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

        public static int GetMatEnd(Mat img, int ylow, int yhigh)
        {
            var start = img.Width;
            var end = start - 140;
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
                    if (sum > 15)
                    {
                        es = idx;
                        break;
                    }
                }
                idx = idx - 2;
            }

            if (es == 0)
            { start = img.Width - 80; }
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

        public static List<int> GetMatSplit(Mat img, int ylow, int yhigh)
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

        public static List<int> DetectMatYHighLow(Mat img)
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

        //public static List<Mat> GetCharMats(Mat xymat, int id)
        //{
        //    var charlist = new List<Mat>();

        //    var kaze = KAZE.Create();
        //    var kazeDescriptors = new Mat();
        //    KeyPoint[] kazeKeyPoints = null;
        //    kaze.DetectAndCompute(xymat, null, out kazeKeyPoints, kazeDescriptors);

        //    var wptlist = new List<KeyPoint>();
        //    for (var idx = 20; idx < xymat.Width;)
        //    {
        //        var yhlist = new List<double>();
        //        var wlist = new List<KeyPoint>();
        //        foreach (var pt in kazeKeyPoints)
        //        {
        //            if (pt.Pt.X >= (idx - 20) && pt.Pt.X < idx)
        //            {
        //                if (pt.Pt.Y + 6 > xymat.Height)
        //                { continue; }

        //                wlist.Add(pt);
        //                yhlist.Add(pt.Pt.Y);
        //            }
        //        }

        //        if (wlist.Count > 10 && (yhlist.Max() - yhlist.Min()) > 0.3 * xymat.Height)
        //        { wptlist.AddRange(wlist); }
        //        idx = idx + 20;
        //    }

        //    var xlist = new List<double>();
        //    var ylist = new List<double>();
        //    foreach (var pt in wptlist)
        //    {
        //        xlist.Add(pt.Pt.X);
        //        ylist.Add(pt.Pt.Y);
        //    }

        //    //var dstKaze = new Mat();
        //    //Cv2.DrawKeypoints(xymat, wptlist, dstKaze);
        //    //using (new Window("dstKazexx" + id, dstKaze))
        //    //{
        //    //    Cv2.WaitKey();
        //    //}


        //    var h0 = (int)ylist.Min() - 3;
        //    if (h0 < 0) { h0 = 0; }
        //    var h1 = (int)ylist.Max() + 3;
        //    if (h1 - h0 > 158) { h1 = h0 + 158; }
        //    if (h1 > xymat.Height) { h1 = xymat.Height - 1; }


        //    var xmax = (int)xlist.Max();

        //    var start = xmax - 150; var end = xmax - 30;
        //    var split2x = GetSplitX(xymat, start, end, h0, h1);

        //    start = xmax - 140; end = (xmax - 250) < 0 ? 0 : (xmax - 250);
        //    if (split2x != -1)
        //    { start = split2x - 150; end = split2x - 40; }
        //    var split1x = GetSplitX(xymat, start, end, h0, h1);

        //    start = xmax - 260; end = (xmax - 380) < 0 ? 0 : (xmax - 380);
        //    if (split1x != -1)
        //    { start = split1x - 150; end = split1x - 40; }
        //    var split0x = GetSplitX(xymat, start, end, h0, h1);

        //    if (split1x != -1 && split2x != -1 && split0x != -1)
        //    {
        //        charlist.Add(xymat.SubMat(h0, h1, split0x, split1x));
        //        charlist.Add(xymat.SubMat(h0, h1, split1x, split2x));
        //        charlist.Add(xymat.SubMat(h0, h1, split2x, xymat.Width - 3));
        //    }
        //    else if (split1x != 0 && split2x != 0)
        //    {
        //        var fontwd = xymat.Width - split2x;
        //        var x0 = split1x - fontwd;
        //        if (x0 < 0) { x0 = 0; }
        //        charlist.Add(xymat.SubMat(h0, h1, x0, split1x));
        //        charlist.Add(xymat.SubMat(h0, h1, split1x, split2x));
        //        charlist.Add(xymat.SubMat(h0, h1, split2x, xymat.Width - 3));
        //    }
        //    else if (split2x != 0)
        //    {
        //        var fontwd = xymat.Width - split2x;
        //        var x1 = split2x - fontwd;
        //        var x0 = split2x - 2 * fontwd;
        //        if (x0 < 0) { x0 = 0; }

        //        charlist.Add(xymat.SubMat(h0, h1, x0, x1));
        //        charlist.Add(xymat.SubMat(h0, h1, x1, split2x));
        //        charlist.Add(xymat.SubMat(h0, h1, split2x, xymat.Width - 3));
        //    }
        //    else
        //    {
        //        var x0 = xymat.Width - 304;
        //        if (x0 < 0) { x0 = 0; }
        //        charlist.Add(xymat.SubMat(h0, h1, x0, xymat.Width - 204));
        //        charlist.Add(xymat.SubMat(h0, h1, xymat.Width - 204, xymat.Width - 103));
        //        charlist.Add(xymat.SubMat(h0, h1, xymat.Width - 103, xymat.Width - 3));
        //    }
        //    return charlist;
        //}

        //public static int GetSplitX(Mat xymat, int snapstart, int snapend, int h0, int h1)
        //{
        //    var ret = -1;
        //    var tm = 0;
        //    for (var sidx = snapend; sidx > snapstart;)
        //    {
        //        var snapmat = xymat.SubMat(h0, h1, sidx, sidx + 2);
        //        var cnt = snapmat.CountNonZero();
        //        if (cnt < 3)
        //        {
        //            if (ret != -1 && tm == 1)
        //            { return sidx; }
        //            else
        //            { ret = sidx; tm = 1; }
        //        }
        //        else
        //        { tm = 0; }
        //        sidx = sidx - 2;
        //    }
        //    return -1;
        //}

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
            var sharpimg = new Mat();
            Cv2.GaussianBlur(xymat, sharpimg, new Size(0, 0), 3);
            Cv2.AddWeighted(xymat, 2.0, sharpimg, -0.4, 0, sharpimg);

            var xyenhance4x = new Mat();
            Cv2.DetailEnhance(sharpimg, sharpimg);
            Cv2.Resize(sharpimg, xyenhance4x, new Size(xymat.Width * 4, xymat.Height * 4));
            //Cv2.DetailEnhance(xyenhance4x, xyenhance4x);

            var xyenhgray = new Mat();
            var denoisemat = new Mat();
            //Cv2.FastNlMeansDenoisingColored(xyenhance4x, denoisemat, 10, 10, 7, 21);
            Cv2.MedianBlur(xyenhance4x, denoisemat, 9);
            Cv2.CvtColor(denoisemat, xyenhgray, ColorConversionCodes.BGR2GRAY);

            var blurred = new Mat();
            Cv2.GaussianBlur(xyenhgray, blurred, new Size(3, 3), 0);

            var edged = new Mat();
            Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

            return edged;
        }

        

        //private static List<List<double>> GetDetectPoint(Mat mat)
        //{
        //    var ret = new List<List<double>>();

        //    var xyenhance = new Mat();
        //    Cv2.DetailEnhance(mat, xyenhance);

        //    var kaze = KAZE.Create();
        //    var kazeDescriptors = new Mat();
        //    KeyPoint[] kazeKeyPoints = null;
        //    kaze.DetectAndCompute(xyenhance, null, out kazeKeyPoints, kazeDescriptors);

        //    var wptlist = new List<KeyPoint>();
        //    for (var idx = 20; idx < mat.Width;)
        //    {
        //        var yhlist = new List<double>();
        //        var wlist = new List<KeyPoint>();
        //        foreach (var pt in kazeKeyPoints)
        //        {
        //            if (pt.Pt.X >= (idx - 20) && pt.Pt.X < idx)
        //            {
        //                wlist.Add(pt);
        //                yhlist.Add(pt.Pt.Y);
        //            }
        //        }

        //        if (wlist.Count > 10 && (yhlist.Max() - yhlist.Min()) > 0.3 * mat.Height)
        //        { wptlist.AddRange(wlist); }
        //        idx = idx + 20;
        //    }

        //        var xlist = new List<double>();
        //        var ylist = new List<double>();
        //        foreach (var pt in wptlist)
        //        {
        //            xlist.Add(pt.Pt.X);
        //            ylist.Add(pt.Pt.Y);
        //        }
        //        ret.Add(xlist);
        //        ret.Add(ylist);

        //        return ret;
        //    }

    }
}