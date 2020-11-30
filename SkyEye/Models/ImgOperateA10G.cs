using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyEye.Models
{
    public class ImgOperateA10G
    {
        public static string DetectA10GRevision(string imgpath)
        {
            Mat srccolor = Cv2.ImRead(imgpath, ImreadModes.Color);
            var detectsize = ImgPreOperate.GetDetectPoint(srccolor);
            var srcrealimg = srccolor.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var srcgray = new Mat();
            Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);
            var srcblurred = new Mat();
            Cv2.GaussianBlur(srcgray, srcblurred, new Size(5, 5), 0);
            var srcedged = new Mat();
            Cv2.AdaptiveThreshold(srcblurred, srcedged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

            var circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 70, 30, 80);

            if (circles.Count() > 1)
            {
                var largecircle = circles[0];
                var smallcircle = circles[0];
                foreach (var c in circles)
                {
                    if (c.Radius > largecircle.Radius)
                    {
                        largecircle = c;
                    }
                    if (c.Radius < smallcircle.Radius)
                    {
                        smallcircle = c;
                    }
                }
                var rate = largecircle.Radius / smallcircle.Radius;
                if (rate > 1.2 && rate < 2.2 && CheckAngle(largecircle.Center, smallcircle.Center, 135, 315))
                {
                    return "OGP-A10G";
                }
            }

            return "";
        }

        public static List<Mat> CutCharRect(string imgpath)
        {
            Mat srccolor = Cv2.ImRead(imgpath, ImreadModes.Color);
            var detectsize = ImgPreOperate.GetDetectPoint(srccolor);
            var srcrealimg = srccolor.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var srcgray = new Mat();
            Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);
            var srcblurred = new Mat();
            Cv2.GaussianBlur(srcgray, srcblurred, new Size(5, 5), 0);
            var srcedged = new Mat();
            Cv2.AdaptiveThreshold(srcblurred, srcedged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

            var circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 65, 30, 80);

            if (circles.Count() > 1)
            {
                var largecircle = circles[0];
                var smallcircle = circles[0];
                foreach (var c in circles)
                {
                    if (c.Radius > largecircle.Radius)
                    {
                        largecircle = c;
                    }
                    if (c.Radius < smallcircle.Radius)
                    {
                        smallcircle = c;
                    }
                }
                var rate = largecircle.Radius / smallcircle.Radius;
                if (rate > 1.2 && rate < 2.2 && CheckAngle(largecircle.Center, smallcircle.Center, 135, 315))
                {
                    var coordmat = new Mat();

                    var xl = largecircle.Center.X - largecircle.Radius - 30;
                    var xh = largecircle.Center.X + largecircle.Radius + 30;
                    if (largecircle.Center.Y < smallcircle.Center.Y)
                    {//pos
                        var yh = largecircle.Center.Y - largecircle.Radius - 19;
                        var yl = yh - 40;
                        if (yl <= 0) { yl = 1; }

                        coordmat = srcrealimg.SubMat((int)yl, (int)yh, (int)xl, (int)xh);
                    }
                    else
                    {//neg
                        var yl = largecircle.Center.Y + largecircle.Radius + 19;
                        var yh = yl + 40;
                        if (yh >= srcrealimg.Height)
                        { yh = srcrealimg.Height - 1; }

                        coordmat = srcrealimg.SubMat((int)yl, (int)yh, (int)xl, (int)xh);
                        var outxymat = new Mat();
                        Cv2.Transpose(coordmat, outxymat);
                        Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                        Cv2.Transpose(outxymat, outxymat);
                        Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                        coordmat = outxymat;
                    }

                    return Get10GMats(coordmat);
                }
            }

            return new List<Mat>();
        }

        private static List<Mat> Get10GMats(Mat coordmat)
        {
            var cmatlist = new List<Mat>();

            var xyenhance4x = new Mat();
            Cv2.Resize(coordmat, xyenhance4x, new Size(coordmat.Width * 4, coordmat.Height * 4));
            Cv2.DetailEnhance(xyenhance4x, xyenhance4x);

            var lowspec = new Scalar(152, 113, 72);
            var highspec = new Scalar(216, 174, 162);

            var coordhsv = new Mat();
            Cv2.CvtColor(xyenhance4x, coordhsv, ColorConversionCodes.BGR2RGB);

            var mask = coordhsv.InRange(lowspec, highspec);
            //using (new Window("mask", mask))
            //{
            //    Cv2.WaitKey();
            //}

            var rectlist = Get10GRect(mask);

            cmatlist.Add(coordmat);
            foreach (var rect in rectlist)
            {
                if (rect.X < 0 || rect.Y < 0
                || ((rect.X + rect.Width) > mask.Width)
                || ((rect.Y + rect.Height) > mask.Height))
                {
                    cmatlist.Clear();
                    return cmatlist;
                }

                cmatlist.Add(mask.SubMat(rect));
            }

            return cmatlist;
        }

        private static List<Rect> Get10GRect(Mat edged)
        {
            var hl = GetHeighLow10G(edged);
            var hh = GetHeighHigh10G(edged);


            var dcl = hl;//(int)(hl + (hh - hl) * 0.1);
            var dch = hh;//(int)(hh - (hh - hl) * 0.1);
            var xxh = GetXXHigh10G(edged, dcl, dch);
            var yxl = GetYXLow10G(edged, dcl, dch);



            var rectlist = new List<Rect>();

            var xxlist = GetXSplitList10G(edged, xxh, hl, hh);
            var flist = (List<int>)xxlist[0];
            var slist = (List<int>)xxlist[1];
            var y = hl - 5;
            var h = hh - hl + 7;

            if (slist.Count == 3)
            {
                var fntw = (int)flist.Average();
                var left = slist[2] - fntw - 10;
                if (left < 0) { left = 1; }
                rectlist.Add(new Rect(left, y, fntw + 4, h));
                rectlist.Add(new Rect(slist[2] - 6, y, slist[1] - slist[2] + 2, h));
                rectlist.Add(new Rect(slist[1] - 6, y, slist[0] - slist[1] + 2, h));
                rectlist.Add(new Rect(slist[0] - 3, y, xxh - slist[0] + 8, h));
            }
            //else if (slist.Count == 2)
            //{
            //    var fntw = (int)flist.Average();
            //    var left = slist[1] - 2 * fntw - 14;
            //    if (left < 0) { left = 1; }
            //    rectlist.Add(new Rect(left, y, fntw + 5, h));
            //    rectlist.Add(new Rect(slist[1] - fntw - 10, y, fntw + 3, h));
            //    rectlist.Add(new Rect(slist[1] - 6, y, slist[0] - slist[1], h));
            //    rectlist.Add(new Rect(slist[0] - 3, y, xxh - slist[0] + 8, h));
            //}
            else
            {
                if ((int)xxh - 210 > 0)
                { rectlist.Add(new Rect(xxh - 210, y, 54, h)); }
                else
                { rectlist.Add(new Rect(0, y, 56, h)); }
                rectlist.Add(new Rect(xxh - 156, y, 52, h));
                rectlist.Add(new Rect(xxh - 100, y, 52, h));
                rectlist.Add(new Rect(xxh - 50, y, 54, h));
            }

            var yxlist = GetYSplitList10G(edged, yxl, hl, hh);
            flist = (List<int>)yxlist[0];
            slist = (List<int>)yxlist[1];
            if (slist.Count == 4)
            {
                rectlist.Add(new Rect(yxl - 3, y, slist[0] - yxl + 6, h));
                rectlist.Add(new Rect(slist[0] + 5, y, slist[1] - slist[0] + 4, h));
                rectlist.Add(new Rect(slist[1] + 5, y, slist[2] - slist[1] + 4, h));
                rectlist.Add(new Rect(slist[2] + 6, y, slist[3] - slist[2] + 8, h));
            }
            else if (slist.Count == 3)
            {
                var fntw = (int)flist.Average();
                rectlist.Add(new Rect(yxl - 3, y, slist[0] - yxl + 6, h));
                rectlist.Add(new Rect(yxl - 3, y, slist[0] - yxl + 6, h));
                rectlist.Add(new Rect(slist[0] + 5, y, slist[1] - slist[0] + 4, h));
                rectlist.Add(new Rect(slist[1] + 5, y, slist[2] - slist[1] + 4, h));
                //var left = slist[2] + 5;
                //if (left + fntw + 5 > edged.Width)
                //{ left = edged.Width - fntw - 5; }
                //rectlist.Add(new Rect(left, y, fntw + 5, h));
            }
            //else if (slist.Count == 2)
            //{
            //    var fntw = (int)flist.Average();
            //    rectlist.Add(new Rect(yxl - 3, y, slist[0] - yxl + 6, h));
            //    rectlist.Add(new Rect(slist[0] + 5, y, slist[1] - slist[0] + 4, h));
            //    rectlist.Add(new Rect(slist[1] + 7, y, fntw + 7, h));
            //    var left = slist[1] + fntw + 14;
            //    if (left + fntw + 8 > edged.Width)
            //    { left = edged.Width - fntw - 8; }
            //    rectlist.Add(new Rect(left, y, fntw + 8, h));
            //}
            else
            {
                rectlist.Add(new Rect(yxl - 4, y, 56, h));
                rectlist.Add(new Rect(yxl + 50, y, 55, h));
                rectlist.Add(new Rect(yxl + 102, y, 55, h));
                if ((yxl + 210) >= (edged.Cols - 1))
                { rectlist.Add(new Rect(yxl + 156, y, edged.Cols - yxl - 156, h)); }
                else
                { rectlist.Add(new Rect(yxl + 156, y, 54, h)); }
            }

            return rectlist;
        }

        private static int GetXDirectSplit10G(Mat edged, int start, int end, int dcl, int dch, int previous)
        {
            var ret = -1;
            for (var idx = start; idx > end; idx = idx - 2)
            {
                var snapmat = edged.SubMat(dcl, dch, idx - 2, idx);
                var cnt = snapmat.CountNonZero();
                if (cnt < 2)
                {
                    if (ret == -1)
                    {
                        ret = idx;
                        if (previous - idx >= 48)
                        { return ret; }
                    }
                    else
                    { return ret; }
                }
                else
                { ret = -1; }
            }
            return -1;
        }

        private static int GetYDirectSplit10G(Mat edged, int start, int end, int dcl, int dch, int previous)
        {
            var ret = -1;
            for (var idx = start; idx < end; idx = idx + 2)
            {
                var snapmat = edged.SubMat(dcl, dch, idx, idx + 2);
                var cnt = snapmat.CountNonZero();
                if (cnt < 2)
                {
                    if (ret == -1)
                    {
                        ret = idx;
                        if (idx - previous >= 48)
                        { return ret; }
                    }
                    else
                    { return ret; }
                }
                else
                { ret = -1; }
            }
            return -1;
        }

        private static List<object> GetXSplitList10G(Mat edged, int xxh, int hl, int hh)
        {
            var offset = 50;
            var ret = new List<object>();
            var flist = new List<int>();
            var slist = new List<int>();
            ret.Add(flist);
            ret.Add(slist);

            var fntw = (int)(edged.Width * 0.333 * 0.25);

            var spx1 = GetXDirectSplit10G(edged, xxh - 20, xxh - 20 - fntw, hl, hh, xxh);
            if (spx1 == -1) { return ret; }
            fntw = xxh - spx1 + 1;
            if (fntw >= 18 && fntw < 35)
            { spx1 = xxh - offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spx1);

            var spx2 = GetXDirectSplit10G(edged, spx1 - 24, spx1 - 24 - fntw, hl, hh, spx1);
            if (spx2 == -1) { return ret; }
            fntw = spx1 - spx2;
            if (fntw >= 18 && fntw < 35)
            { spx2 = spx1 - offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spx2);

            var spx3 = GetXDirectSplit10G(edged, spx2 - 24, spx2 - 24 - fntw, hl, hh, spx2);
            if (spx3 == -1) { return ret; }
            fntw = spx2 - spx3;
            if (fntw >= 18 && fntw < 35)
            { spx3 = spx2 - offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spx3);

            return ret;
        }

        private static List<object> GetYSplitList10G(Mat edged, int yxl, int hl, int hh)
        {
            var offset = 50;
            var ret = new List<object>();
            var flist = new List<int>();
            var slist = new List<int>();
            ret.Add(flist);
            ret.Add(slist);

            var fntw = (int)(edged.Width * 0.333 * 0.25);

            var spy1 = GetYDirectSplit10G(edged, yxl + 24, yxl + 24 + fntw, hl, hh, yxl);
            if (spy1 == -1) { return ret; }
            fntw = spy1 - yxl + 1;
            if (fntw >= 18 && fntw < 35)
            { spy1 = yxl + offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spy1);

            var spy2 = GetYDirectSplit10G(edged, spy1 + 28, spy1 + 28 + fntw, hl, hh, spy1);
            if (spy2 == -1) { return ret; }
            fntw = spy2 - spy1 + 1;
            if (fntw >= 18 && fntw < 35)
            { spy2 = spy1 + offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spy2);

            var spy3 = GetYDirectSplit10G(edged, spy2 + 28, spy2 + 28 + fntw, hl, hh, spy2);
            if (spy3 == -1) { return ret; }
            fntw = spy3 - spy2 + 1;
            if (fntw >= 18 && fntw < 25)
            { spy3 = spy2 + offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spy3);

            var spy4 = GetYDirectSplit10G(edged, spy3 + 28, edged.Width - 10, hl, hh, spy3);
            if (spy4 == -1) { return ret; }
            fntw = spy4 - spy3 + 1;
            if (fntw < 40)
            { return ret; }
            flist.Add(fntw); slist.Add(spy4);

            return ret;
        }

        private static int GetHeighLow10G(Mat edged)
        {
            var cheighxl = (int)(edged.Width * 0.15);
            var cheighxh = (int)(edged.Width * 0.33);
            var cheighyl = (int)(edged.Width * 0.66);
            var cheighyh = (int)(edged.Width * 0.84);

            var xhl = 0;
            var yhl = 0;
            var ymidx = (int)(edged.Height * 0.5);
            for (var idx = ymidx; idx > 5; idx = idx - 2)
            {
                if (xhl == 0)
                {
                    var snapmat = edged.SubMat(idx - 2, idx, cheighxl, cheighxh);
                    var cnt = snapmat.CountNonZero();
                    if (cnt < 3)
                    {
                        xhl = idx;
                    }
                }

                if (yhl == 0)
                {
                    var snapmat = edged.SubMat(idx - 2, idx, cheighyl, cheighyh);
                    var cnt = snapmat.CountNonZero();
                    if (cnt < 3)
                    {
                        yhl = idx;
                    }
                }
            }

            var hl = xhl;
            if (yhl > hl)
            { hl = yhl; }

            return hl;
        }

        private static int GetHeighHigh10G(Mat edged)
        {
            var cheighxl = (int)(edged.Width * 0.15);
            var cheighxh = (int)(edged.Width * 0.33);
            var cheighyl = (int)(edged.Width * 0.66);
            var cheighyh = (int)(edged.Width * 0.84);

            var xhh = 0;
            var yhh = 0;
            var ymidx = (int)(edged.Height * 0.5);
            for (var idx = ymidx; idx < edged.Height - 5; idx = idx + 2)
            {
                if (xhh == 0)
                {
                    var snapmat = edged.SubMat(idx, idx + 2, cheighxl, cheighxh);
                    var cnt = snapmat.CountNonZero();
                    if (cnt < 3)
                    {
                        xhh = idx;
                    }
                }

                if (yhh == 0)
                {
                    var snapmat = edged.SubMat(idx, idx + 2, cheighyl, cheighyh);
                    var cnt = snapmat.CountNonZero();
                    if (cnt < 3)
                    {
                        yhh = idx;
                    }
                }
            }

            var hh = 0;
            if (xhh > ymidx && yhh > ymidx)
            {
                if (yhh < xhh)
                { hh = yhh; }
                else
                { hh = xhh; }
            }
            else if (xhh > ymidx)
            { hh = xhh; }
            else if (yhh > ymidx)
            { hh = yhh; }
            else
            { hh = edged.Height - 5; }
            return hh;
        }

        private static int GetXXHigh10G(Mat edged, int dcl, int dch)
        {
            var ret = -1;
            var tm = 0;
            var wml = (int)(edged.Width * 0.2);
            var wmh = (int)(edged.Width * 0.5);

            for (var idx = wmh; idx > wml; idx = idx - 2)
            {
                var snapmat = edged.SubMat(dcl, dch, idx - 2, idx);
                var cnt = snapmat.CountNonZero();
                if (cnt > 3)
                {
                    tm++;
                    if (ret == -1)
                    { ret = idx; }
                    else if (ret != -1 && tm > 8)
                    { return ret; }
                }
                else
                { ret = -1; tm = 0; }
            }

            return -1;
        }

        private static int GetYXLow10G(Mat edged, int dcl, int dch)
        {
            var ret = -1;
            var tm = 0;
            var wml = (int)(edged.Width * 0.5);
            var wmh = (int)(edged.Width * 0.8);

            for (var idx = wml; idx < wmh; idx = idx + 2)
            {
                var snapmat = edged.SubMat(dcl, dch, idx, idx + 2);
                var cnt = snapmat.CountNonZero();
                if (cnt > 3)
                {
                    tm++;
                    if (ret == -1)
                    { ret = idx; }
                    else if (ret != -1 && tm > 8)
                    { return ret; }
                }
                else
                { ret = -1; tm = 0; }
            }
            return -1;
        }

        private static bool CheckAngle(Point2f P1, Point2f P2, double ang1, double ang2)
        {
            var degree = Math.Atan2((P2.Y - P1.Y), (P2.X - P1.X));
            var d360 = (degree > 0 ? degree : (2 * Math.PI + degree)) * 360 / (2 * Math.PI);
            var lowspec = ang1 - 8;
            var highspec = ang1 + 8;
            if (d360 > lowspec && d360 < highspec)
            { return true; }
            lowspec = ang2 - 8;
            highspec = ang2 + 8;
            if (d360 > lowspec && d360 < highspec)
            { return true; }

            return false;
        }
    }
}