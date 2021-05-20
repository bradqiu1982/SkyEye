using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenCvSharp;

namespace SkyEye.Models
{
    public class ImgOperate5x1
    {

        private static List<Rect> FindXYRect_(Mat blurred,bool cflag, int heighlow, int heighhigh, double ratelow, double ratehigh, int areahigh)
        {
            var ret = new List<Rect>();

            var edged = new Mat();
            //10,100
            Cv2.Canny(blurred, edged, 50, 200, 3, cflag);

            //using (new Window("edged", edged))
            //{
            //    Cv2.WaitKey();
            //}

            var outmat = new Mat();
            var ids = OutputArray.Create(outmat);
            var cons = new Mat[] { };
            Cv2.FindContours(edged, out cons, ids, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            var conslist = cons.ToList();

            foreach (var item in conslist)
            {
                var rect = Cv2.BoundingRect(item);
                var a = rect.Width * rect.Height;
                var whrate = (double)rect.Width / (double)rect.Height;
                //var hwrate = (double)rect.Height / (double)rect.Width;

                if (rect.Height >= heighlow && rect.Height <= heighhigh
                    && whrate > ratelow && whrate < ratehigh && a < areahigh)
                {
                    if (ret.Count > 0)
                    {
                        if (a > ret[0].Width * ret[0].Height)
                        {
                            ret.Clear();
                            ret.Add(rect);
                        }
                    }
                    else
                    { ret.Add(rect); }
                }
                //else if (rect.Width >= heighlow && rect.Width <= heighhigh
                //    && hwrate > ratelow && hwrate < ratehigh && a < areahigh)
                //{
                //    if (ret.Count > 0)
                //    {
                //        if (a > ret[0].Width * ret[0].Height)
                //        {
                //            ret.Clear();
                //            ret.Add(rect);
                //        }
                //    }
                //    else
                //    { ret.Add(rect); }
                //}
            }//end foreach

            return ret;
        }

        public static List<Rect> FindXYRect(string file, int heighlow, int heighhigh, double ratelow, double ratehigh,int areahigh,bool checkcharimg,out bool turn, bool fixangle = false)
        {
            var ret = new List<Rect>();

            var srccolor = Cv2.ImRead(file, ImreadModes.Color);
            if (fixangle)
            {
                var angle = ImgPreOperate.GetAngle(file);
                if (angle >= 0.7 && angle <= 359.3)
                { srccolor = ImgPreOperate.GetFixedAngleImg(srccolor, angle); }
            }

            Mat src = new Mat();
            Cv2.CvtColor(srccolor, src, ColorConversionCodes.BGR2GRAY);

            //var denoisemat = new Mat();
            //Cv2.FastNlMeansDenoising(src, denoisemat, 10, 7, 21);

            var blurred = new Mat();
            Cv2.GaussianBlur(src, blurred, new Size(5, 5), 0);


            turn = false;

            var truerect = FindXYRect_( blurred, true,  heighlow,  heighhigh,  ratelow,  ratehigh,  areahigh);
            var falserect = FindXYRect_( blurred, false,  heighlow,  heighhigh,  ratelow,  ratehigh,  areahigh);

            if (truerect.Count > 0 && falserect.Count > 0)
            {
                if (truerect[0].Width * truerect[0].Height >= falserect[0].Width * falserect[0].Height)
                { ret.AddRange(truerect); }
                else
                { ret.AddRange(falserect); }
            }
            else if (truerect.Count > 0)
            { ret.AddRange(truerect); }
            else
            { ret.AddRange(falserect); }

            if (ret.Count > 0 && checkcharimg)
            {
                var charmat = srccolor.SubMat(ret[0]);
                Cv2.DetailEnhance(charmat, charmat);
                var charmat4x = new Mat();
                Cv2.Resize(charmat, charmat4x, new Size(charmat.Width * 4, charmat.Height * 4));
                Cv2.DetailEnhance(charmat4x, charmat4x);

                var kaze = KAZE.Create();
                var kazeDescriptors = new Mat();
                KeyPoint[] kazeKeyPoints = null;
                kaze.DetectAndCompute(charmat4x, null, out kazeKeyPoints, kazeDescriptors);
                var hptlist = new List<KeyPoint>();
                var cl = 0.3 * charmat4x.Height;
                var ch = 0.7 * charmat4x.Height;
                var rl = 60;
                var rlh = charmat4x.Width * 0.3;
                var rhl = charmat4x.Width * 0.7;
                var rh = charmat4x.Width - 60;


                foreach (var pt in kazeKeyPoints)
                {
                    if (pt.Pt.Y >= cl && pt.Pt.Y <= ch
                        && ((pt.Pt.X >= rl && pt.Pt.X <= rlh) || (pt.Pt.X >= rhl && pt.Pt.X <= rh)))
                    {
                        hptlist.Add(pt);
                    }
                }

                if (hptlist.Count < 140)
                {
                    return new List<Rect>();
                }
            }

            src.Dispose();
            return ret;
        }


  
        public static List<Mat> CutCharRect(string imgpath, ImageDetect detect, Rect xyrect, int heighlow, int heighhigh, int widthlow, int widthhigh, bool fixangle,bool newalg)
        {
            var cmatlist = new List<Mat>();

            Mat src = Cv2.ImRead(imgpath, ImreadModes.Color);
            if (fixangle)
            {
                var angle = ImgPreOperate.GetAngle(imgpath);
                if (angle >= 0.7 && angle <= 359.3)
                { src = ImgPreOperate.GetFixedAngleImg(src, angle); }
            }

            //if (turn)
            //{
            //    var outxymat = new Mat();
            //    Cv2.Transpose(src, outxymat);
            //    Cv2.Flip(outxymat, outxymat, FlipMode.Y);
            //    src = outxymat;
            //}

            var xymat = src.SubMat(xyrect);

            var availableimgpt = ImgPreOperate.GetImageBoundPointX(src);
            //var srcmidy = src.Height / 2;
            var srcmidy = (availableimgpt[1].Max() + availableimgpt[1].Min()) / 2;

            if (xyrect.Y > srcmidy)
            {
                var center = new Point2f(xymat.Width / 2, xymat.Height / 2);
                var m = Cv2.GetRotationMatrix2D(center, 180, 1);
                var outxymat = new Mat();
                Cv2.WarpAffine(xymat, outxymat, m, new Size(xymat.Width, xymat.Height));
                xymat = outxymat;
            }

            var sharpimg = new Mat();
            Cv2.GaussianBlur(xymat, sharpimg, new Size(0, 0), 3);
            Cv2.AddWeighted(xymat, 2.0, sharpimg, -0.4, 0, sharpimg);

            Cv2.DetailEnhance(sharpimg, sharpimg);
            var xyenhance4x = new Mat();
            Cv2.Resize(sharpimg, xyenhance4x, new Size(sharpimg.Width * 4, sharpimg.Height * 4));

            //Cv2.DetailEnhance(xyenhance4x, xyenhance4x);

            var xyenhgray = new Mat();
            var denoisemat = new Mat();
            //Cv2.FastNlMeansDenoisingColored(xyenhance4x, denoisemat, 10, 10, 7, 21);
            Cv2.MedianBlur(xyenhance4x, denoisemat, 9);
            Cv2.CvtColor(denoisemat, xyenhgray, ColorConversionCodes.BGR2GRAY);


            var blurred = new Mat();
            Cv2.GaussianBlur(xyenhgray, blurred, new Size(5, 5), 0);

            var edged = new Mat();
            Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

            var crectlist = new List<Rect>();
            if (newalg)
            { crectlist = GetNew5x1Rect(edged, xyenhance4x); }
            else
            { crectlist = Get5x1Rect(blurred, edged, xyenhance4x); }
                

            if (crectlist.Count > 0)
            {
                cmatlist.Add(sharpimg);
                foreach (var rect in crectlist)
                {
                    if (rect.X < 0 || rect.Y < 0
                        ||((rect.X+rect.Width) > edged.Width)
                        ||((rect.Y+rect.Height) > edged.Height))
                    {
                        cmatlist.Clear();
                        return cmatlist;
                    }
                    cmatlist.Add(edged.SubMat(rect));
                }
            }

            return cmatlist;
        }



        private static List<int> GetBadPossibleXList(Mat edged, int heighlow, int heighhigh, int widthlow, int widthhigh)
        {
            var ret = new List<int>();
            ret.Add(5);ret.Add(60);ret.Add(105);ret.Add(160);
            ret.Add(edged.Width - 225); ret.Add(edged.Width - 170); ret.Add(edged.Width - 115); ret.Add(edged.Width-60);

            ret.Add(5);ret.Add(edged.Height - 5);

            return ret;
        }

        public static List<Mat> CutBadCharRect(string imgpath, Rect xyrect, int heighlow, int heighhigh, int widthlow, int widthhigh)
        {
            var cmatlist = new List<Mat>();

            Mat src = Cv2.ImRead(imgpath, ImreadModes.Color);
            var xymat = src.SubMat(xyrect);

            var availableimgpt = ImgPreOperate.GetImageBoundPointX(src);
            //var srcmidy = src.Height / 2;
            var srcmidy = (availableimgpt[1].Max() + availableimgpt[1].Min()) / 2;

            if (xyrect.Y > srcmidy)
            {
                var center = new Point2f(xymat.Width / 2, xymat.Height / 2);
                var m = Cv2.GetRotationMatrix2D(center, 180, 1);
                var outxymat = new Mat();
                Cv2.WarpAffine(xymat, outxymat, m, new Size(xymat.Width, xymat.Height));
                xymat = outxymat;
            }

            var xyenhance = new Mat();
            Cv2.DetailEnhance(xymat, xyenhance);

            var xyenhgray = new Mat();
            Cv2.CvtColor(xyenhance, xyenhgray, ColorConversionCodes.BGR2GRAY);

            Cv2.Resize(xyenhgray, xyenhgray, new Size(xyenhgray.Width * 4, xyenhgray.Height * 4));

            var subx = new List<int>();
            subx.Add(5); subx.Add(15);
            subx.Add(25); subx.Add(30);
            var xyenhgraylist = new List<Mat>();
            foreach (var x in subx)
            {
                var tmpmat = new Mat();
                tmpmat = xyenhgray.SubMat(x, xyenhgray.Rows - x, x, xyenhgray.Cols - x);
                xyenhgraylist.Add(tmpmat);
            }

            foreach (var tempmat in xyenhgraylist)
            {
                var blurred = new Mat();
                Cv2.GaussianBlur(tempmat, blurred, new Size(5, 5), 0);

                //using (new Window("blurred", blurred))
                //{
                //    Cv2.WaitKey();
                //}

                var edged = new Mat();
                Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 11, 2);
                //using (new Window("edged", edged))
                //{
                //    Cv2.WaitKey();
                //}

                var possxlist = GetBadPossibleXList(edged, heighlow, heighhigh, widthlow, widthhigh);
                if (possxlist.Count > 0)
                {
                    try
                    {

                        var eheight0 = possxlist[8];
                        var eheight1 = possxlist[9];

                        cmatlist.Add(xyenhance);

                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[0], possxlist[1]));
                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[1], possxlist[2]));
                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[2], possxlist[3]));
                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[3], possxlist[3]+55));

                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[4], possxlist[5]));
                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[5], possxlist[6]));
                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[6], possxlist[7]));
                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[7], possxlist[7]+54));
                        return cmatlist;
                    }
                    catch (Exception ex)
                    {
                        cmatlist.Clear();
                        return cmatlist;
                    }
                }
            }

            return cmatlist;
        }


        public static List<Rect> GetNew5x1Rect(Mat edged, Mat xyenhance4x)
        {
            var hl = GetHeighLow(edged);
            var hh = GetHeighHigh(edged);
            if (hl < 6)
            { hl = 10; }
            if (hh > edged.Height - 6)
            { hh = edged.Height - 10; }

            var dcl = (int)(hl + (hh - hl) * 0.1);
            var dch = (int)(hh - (hh - hl) * 0.1);
            var xxh = GetXXHigh(edged, dcl, dch);
            var yxl = GetYXLow(edged, dcl, dch);
            if (xxh == -1 || yxl == -1)
            {
                var xlist = GetCoordWidthPT1(xyenhance4x, edged);
                var xmid = (xlist.Max() + xlist.Min()) / 2;
                var xcxlist = new List<double>();
                var ycxlist = new List<double>();
                foreach (var x in xlist)
                {
                    if (x < xmid)
                    { xcxlist.Add(x); }
                    else
                    { ycxlist.Add(x); }
                }
                xxh = (int)xcxlist.Max() + 2;
                yxl = (int)ycxlist.Min() - 2;
            }

            var rectlist = new List<Rect>();

            var xxlist = GetXSplitList(edged, xxh, hl, hh);
            var flist = (List<int>)xxlist[0];
            var slist = (List<int>)xxlist[1];
            var y = hl - 2;
            var h = hh - hl + 4;

            if (slist.Count == 3)
            {
                var fntw = (int)flist.Average();
                var left = slist[2] - fntw - 3;
                if (left < 0) { left = 1; }
                rectlist.Add(new Rect(left, y, fntw + 1, h));
                rectlist.Add(new Rect(slist[2] - 3, y, slist[1] - slist[2], h));
                rectlist.Add(new Rect(slist[1] - 3, y, slist[0] - slist[1], h));
                rectlist.Add(new Rect(slist[0] - 3, y, xxh - slist[0] + 2, h));
            }
            else if (slist.Count == 2)
            {
                var fntw = (int)flist.Average();
                var left = slist[1] - 2 * fntw - 4;
                if (left < 0) { left = 1; }
                rectlist.Add(new Rect(left, y, fntw + 1, h));
                rectlist.Add(new Rect(slist[1] - fntw - 3, y, fntw + 1, h));
                rectlist.Add(new Rect(slist[1] - 3, y, slist[0] - slist[1], h));
                rectlist.Add(new Rect(slist[0] - 3, y, xxh - slist[0] + 2, h));
            }
            else
            {
                if ((int)xxh - 226 > 0)
                { rectlist.Add(new Rect(xxh - 226, y, 48, h)); }
                else
                { rectlist.Add(new Rect(0, y, 48, h)); }
                rectlist.Add(new Rect(xxh - 164, y, 48, h));
                rectlist.Add(new Rect(xxh - 110, y, 48, h));
                rectlist.Add(new Rect(xxh - 55, y, 48, h));
            }

            var yxlist = GetYSplitList(edged, yxl, hl, hh);
            flist = (List<int>)yxlist[0];
            slist = (List<int>)yxlist[1];
            if (slist.Count == 4)
            {
                rectlist.Add(new Rect(yxl - 1, y, slist[0] - yxl + 2, h));
                rectlist.Add(new Rect(slist[0] + 3, y, slist[1] - slist[0], h));
                rectlist.Add(new Rect(slist[1] + 3, y, slist[2] - slist[1], h));
                rectlist.Add(new Rect(slist[2] + 3, y, slist[3] - slist[2], h));
            }
            else if (slist.Count == 3)
            {
                var fntw = (int)flist.Average();
                rectlist.Add(new Rect(yxl - 1, y, slist[0] - yxl + 2, h));
                rectlist.Add(new Rect(slist[0] + 3, y, slist[1] - slist[0], h));
                rectlist.Add(new Rect(slist[1] + 3, y, slist[2] - slist[1], h));
                var left = slist[2] + 3;
                if (left + fntw > edged.Width)
                { left = edged.Width - fntw - 2; }
                rectlist.Add(new Rect(left, y, fntw, h));
            }
            else if (slist.Count == 2)
            {
                var fntw = (int)flist.Average();
                rectlist.Add(new Rect(yxl - 1, y, slist[0] - yxl + 2, h));
                rectlist.Add(new Rect(slist[0] + 3, y, slist[1] - slist[0], h));
                rectlist.Add(new Rect(slist[1] + 3, y, fntw + 2, h));
                var left = slist[1] + fntw + 3;
                if (left + fntw + 4 > edged.Width)
                { left = edged.Width - fntw - 4; }
                rectlist.Add(new Rect(left, y, fntw + 2, h));
            }
            else
            {
                rectlist.Add(new Rect(yxl - 2, y, 48, h));
                rectlist.Add(new Rect(yxl + 53, y, 48, h));
                rectlist.Add(new Rect(yxl + 110, y, 48, h));
                if ((yxl + 211) >= (edged.Cols - 1))
                { rectlist.Add(new Rect(yxl + 161, y, edged.Cols - yxl - 161, h)); }
                else
                { rectlist.Add(new Rect(yxl + 161, y, 50, h)); }
            }
            return rectlist;
        }

        public static int GetHeighLow(Mat edged)
        {
            var cheighxl = (int)(edged.Width * 0.20);
            var cheighxh = (int)(edged.Width * 0.33);
            var cheighyl = (int)(edged.Width * 0.66);
            var cheighyh = (int)(edged.Width * 0.79);

            var xhl = 0;
            var yhl = 0;
            var ymidx = (int)(edged.Height * 0.5);
            for (var idx = ymidx; idx > 10; idx = idx - 2)
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
        public static int GetHeighHigh(Mat edged)
        {
            var cheighxl = (int)(edged.Width * 0.20);
            var cheighxh = (int)(edged.Width * 0.33);
            var cheighyl = (int)(edged.Width * 0.66);
            var cheighyh = (int)(edged.Width * 0.79);

            var xhh = 0;
            var yhh = 0;
            var ymidx = (int)(edged.Height * 0.5);
            for (var idx = ymidx; idx < edged.Height - 10; idx = idx + 2)
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
            { hh = edged.Height - 10; }
            return hh;
        }

        public static int GetXXHigh(Mat edged, int dcl, int dch)
        {
            var wml = (int)(edged.Width * 0.35);

            for (var idx = wml; idx > wml / 2; idx = idx - 2)
            {
                var snapmat = edged.SubMat(dcl, dch, idx - 2, idx);
                var cnt = snapmat.CountNonZero();
                if (cnt > 3)
                {
                    return idx;
                }
            }

            return -1;
        }
        public static int GetYXLow(Mat edged, int dcl, int dch)
        {
            var wml = (int)(edged.Width * 0.35);
            var wmh = (int)(edged.Width * 0.65);

            for (var idx = wmh; idx < (wmh + wml / 2); idx = idx + 2)
            {
                var snapmat = edged.SubMat(dcl, dch, idx, idx + 2);
                var cnt = snapmat.CountNonZero();
                if (cnt > 3)
                {
                    return idx;
                }
            }
            return -1;
        }

        public static int GetXDirectSplit(Mat edged, int start, int end, int dcl, int dch)
        {
            for (var idx = start; idx > end; idx = idx - 2)
            {
                var snapmat = edged.SubMat(dcl, dch, idx - 2, idx);
                var cnt = snapmat.CountNonZero();
                if (cnt < 2)
                {
                    return idx;
                }
            }
            return -1;
        }
        public static int GetYDirectSplit(Mat edged, int start, int end, int dcl, int dch)
        {
            for (var idx = start; idx < end; idx = idx + 2)
            {
                var snapmat = edged.SubMat(dcl, dch, idx, idx + 2);
                var cnt = snapmat.CountNonZero();
                if (cnt < 2)
                {
                    return idx;
                }
            }
            return -1;
        }

        public static List<object> GetXSplitList(Mat edged, int xxh, int hl, int hh)
        {
            var offset = 50;
            var ret = new List<object>();
            var flist = new List<int>();
            var slist = new List<int>();
            ret.Add(flist);
            ret.Add(slist);

            var fntw = (int)(edged.Width * 0.333 * 0.25);

            var spx1 = GetXDirectSplit(edged, xxh - 20, xxh - 20 - fntw, hl, hh);
            if (spx1 == -1) { return ret; }
            fntw = xxh - spx1 + 1;
            if (fntw >= 18 && fntw < 40)
            { spx1 = xxh - offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spx1);

            var spx2 = GetXDirectSplit(edged, spx1 - 21, spx1 - 21 - fntw, hl, hh);
            if (spx2 == -1) { return ret; }
            fntw = spx1 - spx2;
            if (fntw >= 18 && fntw < 40)
            { spx2 = spx1 - offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spx2);

            var spx3 = GetXDirectSplit(edged, spx2 - 21, spx2 - 21 - fntw, hl, hh);
            if (spx3 == -1) { return ret; }
            fntw = spx2 - spx3;
            if (fntw >= 18 && fntw < 40)
            { spx3 = spx2 - offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spx3);

            return ret;
        }
        public static List<object> GetYSplitList(Mat edged, int yxl, int hl, int hh)
        {
            var offset = 50;
            var ret = new List<object>();
            var flist = new List<int>();
            var slist = new List<int>();
            ret.Add(flist);
            ret.Add(slist);

            var fntw = (int)(edged.Width * 0.333 * 0.25);

            var spy1 = GetYDirectSplit(edged, yxl + 20, yxl + 20 + fntw, hl, hh);
            if (spy1 == -1) { return ret; }
            fntw = spy1 - yxl + 1;
            if (fntw >= 18 && fntw < 40)
            { spy1 = yxl + offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spy1);

            var spy2 = GetYDirectSplit(edged, spy1 + 21, spy1 + 21 + fntw, hl, hh);
            if (spy2 == -1) { return ret; }
            fntw = spy2 - spy1 + 1;
            if (fntw >= 18 && fntw < 40)
            { spy2 = spy1 + offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spy2);

            var spy3 = GetYDirectSplit(edged, spy2 + 20, spy2 + 20 + fntw, hl, hh);
            if (spy3 == -1) { return ret; }
            fntw = spy3 - spy2 + 1;
            if (fntw >= 18 && fntw < 40)
            { spy3 = spy2 + offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spy3);

            var spy4 = GetYDirectSplit(edged, spy3 + 20, edged.Width - 10, (int)(hl + 0.1 * (hh - hl)), (int)(hh - 0.1 * (hh - hl)));
            if (spy4 == -1) { return ret; }
            fntw = spy4 - spy3 + 1;
            if (fntw < 45)
            { return ret; }
            flist.Add(fntw); slist.Add(spy4);

            return ret;
        }


        public static List<Rect> Get5x1Rect(Mat blurred, Mat edged, Mat xyenhance4)
        {
            var resizeenhance = new Mat();
            Cv2.DetailEnhance(xyenhance4, resizeenhance);
            var xlist = GetCoordWidthPT1(resizeenhance,edged);
            if (xlist.Count == 0 || (xlist.Max() - xlist.Min()) < 400)
            { return new List<Rect>(); }

            var cbond = GetCoordBond1(blurred, edged);

            var xmid = (xlist.Max() + xlist.Min()) / 2;
            var xcxlist = new List<double>();
            var ycxlist = new List<double>();
            foreach (var x in xlist)
            {
                if (x < xmid)
                { xcxlist.Add(x); }
                else
                { ycxlist.Add(x); }
            }

            var xcmax = xcxlist.Max() + 3;
            var ycmin = ycxlist.Min() - 3;

            var ret = new List<Rect>();
            var wd = 48;
            var rectrange = 25;

            if (cbond.Count > 0)
            {

                var ylist = new List<int>();
                var hlist = new List<int>();
                foreach (var item in cbond)
                {
                    ylist.Add(item.Y);
                    hlist.Add(item.Height);
                }

                var y0 = (int)ylist.Average() - 2;
                var y1 = (int)hlist.Max() + 1;
                if ((y1 + y0) > edged.Height)
                { y1 = edged.Height - y0 - 1; }

                if ((int)xcmax - 226 > 0)
                {
                    ret.Add(new Rect((int)xcmax - 226, y0, wd, y1));
                }
                else
                {
                    ret.Add(new Rect(0, y0, wd, y1));
                }

                ret.Add(new Rect((int)xcmax - 164, y0, wd, y1));
                ret.Add(new Rect((int)xcmax - 110, y0, wd, y1));
                ret.Add(new Rect((int)xcmax - 55, y0, wd, y1));

                ret.Add(new Rect((int)ycmin - 2, y0, wd, y1));
                ret.Add(new Rect((int)ycmin + 53, y0, wd, y1));
                ret.Add(new Rect((int)ycmin + 110, y0, wd, y1));

                if (((int)ycmin + 211) >= (edged.Cols - 1))
                {
                    ret.Add(new Rect((int)ycmin + 161, y0, edged.Cols - (int)ycmin - 161, y1));
                }
                else
                {
                    ret.Add(new Rect((int)ycmin + 161, y0, 50, y1));
                }

                cbond.Sort(delegate (Rect o1, Rect o2)
                { return o1.X.CompareTo(o2.X); });

                var filteredbond = new List<Rect>();
                foreach (var item in cbond)
                {
                    if (item.X <= 3)
                    { continue; }

                    if (filteredbond.Count == 0)
                    {
                        filteredbond.Add(item);
                    }
                    else
                    {
                        var bcnt = filteredbond.Count;
                        if (item.X - filteredbond[bcnt - 1].X > 35)
                        {
                            filteredbond.Add(item);
                        }
                    }
                }

                var changedict = new Dictionary<int, bool>();

                for (var idx = 0; idx < 7; idx++)
                {
                    foreach (var item in filteredbond)
                    {
                        if ((item.X >= ret[idx].X - rectrange) && (item.X <= ret[idx].X + rectrange))
                        {
                            var currentrect = new Rect(item.X - 2, ret[idx].Y, item.Width + 4, ret[idx].Height);
                            if (idx == 0)
                            {
                                currentrect = new Rect((item.X - 6) > 0 ? (item.X - 6) : 0, ret[idx].Y, item.Width + 2, ret[idx].Height);
                            }

                            if (idx == 7)
                            {
                                currentrect = new Rect(item.X, ret[idx].Y, (edged.Width - item.Width - 4) > 0 ? (item.Width + 4) : (edged.Width - item.X - 1), ret[idx].Height);
                            }

                            //ret[idx] = currentrect;

                            if (!changedict.ContainsKey(idx) && currentrect.Width < (wd+10))
                            {
                                ret[idx] = currentrect;
                                changedict.Add(idx, true);
                            }
                            break;
                        }//end if
                    }//end foreach
                }//end for

                for (var idx = 0; idx < 7; idx++)
                {
                    foreach (var item in filteredbond)
                    {
                        if ((item.X >= ret[idx].X - rectrange) && (item.X <= ret[idx].X + rectrange))
                        {
                            if ((idx >= 0 && idx <= 2) || (idx >= 4 && idx <= 6))
                            {
                                var nextrect = new Rect(item.X + item.Width + 2, ret[idx].Y, item.Width + 4, ret[idx].Height);
                                if (idx + 1 == 7)
                                {
                                    nextrect = new Rect(item.X + item.Width + 4, ret[idx].Y, (edged.Width - item.X - 2 * item.Width - 8) > 0 ? (item.Width + 4) : (edged.Width - item.X - item.Width - 4), ret[idx].Height);
                                }

                                if (!changedict.ContainsKey(idx + 1) && nextrect.Width < (wd + 10))
                                {
                                    ret[idx + 1] = nextrect;
                                    changedict.Add(idx + 1, true);
                                }
                            }

                            if ((idx >= 1 && idx <= 3) || (idx >= 5 && idx <= 7))
                            {
                                var nextrect = new Rect((item.X - item.Width - 2) > 0 ? (item.X - item.Width - 2) : 0, ret[idx].Y, item.Width + 4, ret[idx].Height);
                                if (idx - 1 == 0)
                                {
                                    nextrect = new Rect((item.X - item.Width - 6) > 0 ? (item.X - item.Width - 6) : 0, ret[idx].Y, item.Width, ret[idx].Height);
                                }

                                if (!changedict.ContainsKey(idx - 1) && nextrect.Width < (wd + 10))
                                {
                                    ret[idx - 1] = nextrect;
                                    changedict.Add(idx - 1, true);
                                }
                            }
                            break;
                        }//end if
                    }//end foreach
                }//end for
            }
            else
            {
                var y0 = 32;
                var y1 = edged.Height - 60;

                if ((int)xcmax - 226 > 0)
                {
                    ret.Add(new Rect((int)xcmax - 226, y0, wd, y1));
                }
                else
                {
                    ret.Add(new Rect(0, y0, wd, y1));
                }

                ret.Add(new Rect((int)xcmax - 164, y0, wd, y1));
                ret.Add(new Rect((int)xcmax - 110, y0, wd, y1));
                ret.Add(new Rect((int)xcmax - 55, y0, wd, y1));

                ret.Add(new Rect((int)ycmin - 2, y0, wd, y1));
                ret.Add(new Rect((int)ycmin + 53, y0, wd, y1));
                ret.Add(new Rect((int)ycmin + 110, y0, wd, y1));

                if (((int)ycmin + 211) >= (edged.Cols - 1))
                {
                    ret.Add(new Rect((int)ycmin + 161, y0, edged.Cols - (int)ycmin - 161, y1));
                }
                else
                {
                    ret.Add(new Rect((int)ycmin + 161, y0, 50, y1));
                }

            }

            return ret;
        }


        public static List<Rect> GetCoordBond1(Mat blurred, Mat edged)
        {
            var rectlist = new List<Rect>();

            var xlow = (int)(edged.Width * 0.333 - 20);
            var xhigh = (int)(edged.Width * 0.666 - 10);

            var edged23 = new Mat();
            Cv2.AdaptiveThreshold(blurred, edged23, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 23, 20);
            var struc = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(3, 3));
            var erodemat = new Mat();
            Cv2.Erode(edged, erodemat, struc);

            var matlist = new List<Mat>();
            matlist.Add(edged);
            matlist.Add(edged23);
            matlist.Add(erodemat);
            foreach (var m in matlist)
            {
                var outmat = new Mat();
                var ids = OutputArray.Create(outmat);
                var cons = new Mat[] { };
                Cv2.FindContours(m, out cons, ids, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

                var idx1 = 0;
                foreach (var item in cons)
                {
                    idx1++;

                    var crect = Cv2.BoundingRect(item);

                    if (crect.Width >= 40 && crect.Width <= 60 && crect.Height > 50 && crect.Height < 90)
                    {
                        if ((crect.X < xlow || crect.X > xhigh) && crect.X > 5 && crect.Y >= 6)
                        {
                            rectlist.Add(crect);
                        }

                    }//end if
                }
            }//end foreach

            return rectlist;
        }

        private static List<double> GetCoordWidthPT1(Mat mat,Mat edged)
        {
            var ret = new List<List<double>>();
            var kaze = KAZE.Create();
            var kazeDescriptors = new Mat();
            KeyPoint[] kazeKeyPoints = null;
            kaze.DetectAndCompute(edged, null, out kazeKeyPoints, kazeDescriptors);

            var hl = 0.25 * mat.Height;
            var hh = 0.75 * mat.Height;
            var wl = 10;
            var wh = mat.Width - 10;
            var hptlist = new List<KeyPoint>();
            foreach (var pt in kazeKeyPoints)
            {
                if (pt.Pt.Y >= hl && pt.Pt.Y <= hh
                    && pt.Pt.X >= wl && pt.Pt.X <= wh)
                {
                    hptlist.Add(pt);
                }
            }

            //var wptlist = hptlist;

            var wptlist = new List<KeyPoint>();
            for (var idx = 15; idx < mat.Width;)
            {
                var ylist = new List<double>();

                var wlist = new List<KeyPoint>();
                foreach (var pt in hptlist)
                {
                    if (pt.Pt.X >= (idx - 15) && pt.Pt.X < idx)
                    {
                        wlist.Add(pt);
                        ylist.Add(pt.Pt.Y);
                    }
                }

                if (wlist.Count > 8 && (ylist.Max() - ylist.Min()) > 0.25 * mat.Height)
                { wptlist.AddRange(wlist); }
                idx = idx + 15;
            }

            var xlist = new List<double>();
            if (wptlist.Count() == 0)
            {
                return xlist;
            }

            foreach (var pt in wptlist)
            {
                xlist.Add(pt.Pt.X);
            }

            var xlength = xlist.Max() - xlist.Min();
            var coordlength = 0.336 * xlength;
            var xmin = xlist.Min() + coordlength;
            var xmax = xlist.Max() - coordlength;

            var xyptlist = new List<KeyPoint>();
            foreach (var pt in wptlist)
            {
                if (pt.Pt.X <= xmin || pt.Pt.X >= xmax)
                {
                    xyptlist.Add(pt);
                }
            }

            //var dstKaze = new Mat();
            //Cv2.DrawKeypoints(mat, xyptlist.ToArray(), dstKaze);

            //using (new Window("dstKaze", dstKaze))
            //{
            //    Cv2.WaitKey();
            //}

            xlist.Clear();
            foreach (var pt in xyptlist)
            {
                xlist.Add(pt.Pt.X);
            }

            return xlist;
        }

    }
}