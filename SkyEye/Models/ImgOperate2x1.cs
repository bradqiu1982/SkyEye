using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenCvSharp;

namespace SkyEye.Models
{
    public class ImgOperate2x1
    {
        public static List<Rect> FindXYRect(string file, int heighlow, int heighhigh, double ratelow, double ratehigh)
        {
            var ret = new List<Rect>();

            Mat srcorgimg = Cv2.ImRead(file, ImreadModes.Color);
            var detectsize = GetDetectPoint(srcorgimg);
            var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var srcgray = new Mat();
            Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);

            var blurred = new Mat();
            Cv2.GaussianBlur(srcgray, blurred, new Size(5, 5), 0);

            var cannyflags = new List<bool>();
            cannyflags.Add(true);
            cannyflags.Add(false);
            foreach (var cflag in cannyflags)
            {
                var edged = new Mat();
                Cv2.Canny(blurred, edged, 50, 200, 3, cflag);

                var outmat = new Mat();
                var ids = OutputArray.Create(outmat);
                var cons = new Mat[] { };
                Cv2.FindContours(edged, out cons, ids, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
                var conslist = cons.ToList();

                foreach (var item in conslist)
                {
                    var rect = Cv2.BoundingRect(item);
                    var whrate = (float)rect.Width / (float)rect.Height;
                    var a = rect.Width * rect.Height;

                    if (whrate > ratelow && whrate < ratehigh
                        && rect.Height > heighlow && rect.Height < heighhigh)
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
                }//end foreach

                if (ret.Count > 0)
                { break; }

            }//end foreach

            return ret;
        }

        //40,56,65
        public static List<Mat> CutCharRect(string imgpath, Rect xyrect, int widthlow, int widthhigh, int heightlow)
        {
            var cmatlist = new List<Mat>();

            Mat srcorgimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var detectsize = GetDetectPoint(srcorgimg);
            var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var xymat = srcrealimg.SubMat(xyrect);
            var srcmidy = (detectsize[1].Max() + detectsize[1].Min()) / 2;

            if ((xyrect.Y + (xyrect.Height / 2)) < srcmidy)
            {
                var outxymat = new Mat();
                Cv2.Transpose(xymat, outxymat);
                Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                Cv2.Transpose(outxymat, outxymat);
                Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                xymat = outxymat;
            }

            var newxymat = xymat.SubMat(4, xymat.Rows - 20, (int)(xymat.Cols * 0.63), xymat.Cols - 12);

            var xyenhance = new Mat();
            Cv2.DetailEnhance(newxymat, xyenhance);

            var xyenhance4x = new Mat();
            Cv2.Resize(xyenhance, xyenhance4x, new Size(xyenhance.Width * 4, xyenhance.Height * 4));
            Cv2.DetailEnhance(xyenhance4x, xyenhance4x);

            var denoisemat2 = new Mat();
            Cv2.FastNlMeansDenoisingColored(xyenhance4x, denoisemat2, 10, 10, 7, 21);

            var xyenhgray = new Mat();
            Cv2.CvtColor(denoisemat2, xyenhgray, ColorConversionCodes.BGR2GRAY);


            var blurred = new Mat();
            Cv2.GaussianBlur(xyenhgray, blurred, new Size(5, 5), 0);

            var edged = new Mat();
            Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

            var xypts = GetCoordPT2X1(edged);
            var xmin = xypts[0].Min();
            if (xmin < 6)
            {
                newxymat = xymat.SubMat(3, xymat.Rows - 20, (int)(xymat.Cols * 0.63-2.5), xymat.Cols - 12);

                xyenhance = new Mat();
                Cv2.DetailEnhance(newxymat, xyenhance);
                xyenhance4x = new Mat();
                Cv2.Resize(xyenhance, xyenhance4x, new Size(xyenhance.Width * 4, xyenhance.Height * 4));
                Cv2.DetailEnhance(xyenhance4x, xyenhance4x);

                denoisemat2 = new Mat();
                Cv2.FastNlMeansDenoisingColored(xyenhance4x, denoisemat2, 10, 10, 7, 21);
                xyenhgray = new Mat();
                Cv2.CvtColor(denoisemat2, xyenhgray, ColorConversionCodes.BGR2GRAY);

                blurred = new Mat();
                Cv2.GaussianBlur(xyenhgray, blurred, new Size(5, 5), 0);
                edged = new Mat();
                Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);
            }

            var rectlist = GetCharRect2X1(xyenhance4x, edged,  widthlow,  widthhigh, heightlow);
            cmatlist.Add(xyenhance);
            foreach (var r in rectlist)
            { cmatlist.Add(edged.SubMat(r)); }

            return cmatlist;
        }

        private static List<Rect> GetCharRect2X1(Mat xyenhance4x, Mat edged, int widthlow, int widthhigh, int heightlow)
        {
            var xypts = GetCoordPT2X1(edged);
            var cbox = GetCoorBond2x1(xyenhance4x, edged,  widthlow,  widthhigh, heightlow);

            var y0list = new List<int>();
            var y1list = new List<int>();
            var x0list = new List<int>();
            foreach (var bx in cbox)
            {
                if (bx.Y < 65)
                { y0list.Add(bx.Y); }
                else
                { y1list.Add(bx.Y); }
                x0list.Add(bx.X);
            }

            var y0 = (int)xypts[1].Min() - 2;
            if (y0list.Count > 0)
            { y0 = y0list.Min() - 2; }
            var h0 = 82;
            if (y0 < 0) { y0 = 0; }

            var y1 = y0 + 100;
            if (y0list.Count == 0 && y1list.Count > 0)
            { y1 = y1list.Min() - 2; }
            var h1 = 82;
            if ((y1 + h1) > edged.Height)
            { h1 = edged.Height - y1 - 1; }

            var x0 = (int)xypts[0].Min() - 2;
            if (x0list.Count > 0)
            {
                var xmin = x0list.Min();
                if (xmin > (x0 - 15) && xmin < (x0 + 15))
                { x0 = xmin - 2; }
            }
            if (x0 < 0)
            { x0 = 0; }

            var x1 = x0 + 54;
            var x2 = x0 + 106;
            var x3 = x0 + 160;
            var wd = 54;

            var rectlist = new List<Rect>();
            rectlist.Add(new Rect(x0, y0, wd + 1, h0));
            rectlist.Add(new Rect(x1, y0, wd, h0));
            rectlist.Add(new Rect(x2, y0, wd, h0));
            if (x3 + wd > edged.Width)
            {
                var wd1 = edged.Width - x3 - 1;
                rectlist.Add(new Rect(x3, y0, wd1, h0));
            }
            else
            { rectlist.Add(new Rect(x3, y0, wd, h0)); }

            rectlist.Add(new Rect(x0, y1, wd + 1, h1));
            rectlist.Add(new Rect(x1, y1, wd, h1));
            rectlist.Add(new Rect(x2, y1, wd, h1));
            if (x3 + wd > edged.Width)
            {
                var wd1 = edged.Width - x3 - 1;
                rectlist.Add(new Rect(x3, y1, wd1, h1));
            }
            else
            { rectlist.Add(new Rect(x3, y1, wd, h1)); }

            return rectlist;
        }

        private static List<Rect> GetCoorBond2x1(Mat xyenhance4x, Mat edged, int widthlow, int widthhigh,int heightlow)
        {
            var ret = new List<Rect>();

            var outmat = new Mat();
            var ids = OutputArray.Create(outmat);
            var cons = new Mat[] { };
            Cv2.FindContours(edged, out cons, ids, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            foreach (var item in cons)
            {
                var crect = Cv2.BoundingRect(item);
                if (crect.Width >= widthlow && crect.Width <= widthhigh && crect.Height > heightlow)
                {
                    ret.Add(crect);
                }//end if
            }

            return ret;
        }

        private static List<List<double>> GetCoordPT2X1(Mat edged)
        {
            var ret = new List<List<double>>();

            var kaze = KAZE.Create();
            var kazeDescriptors = new Mat();
            KeyPoint[] kazeKeyPoints = null;
            kaze.DetectAndCompute(edged, null, out kazeKeyPoints, kazeDescriptors);

            var allpt = kazeKeyPoints.ToList();

            var wptlist = new List<KeyPoint>();
            for (var idx = 15; idx < edged.Width;)
            {
                var ylist = new List<double>();
                var wlist = new List<KeyPoint>();
                foreach (var pt in allpt)
                {
                    if (pt.Pt.X >= (idx - 15) && pt.Pt.X < idx)
                    {
                        wlist.Add(pt);
                        ylist.Add(pt.Pt.Y);
                    }
                }

                if (wlist.Count > 16 && (ylist.Max() - ylist.Min()) > 0.6 * edged.Height)
                { wptlist.AddRange(wlist); }
                idx = idx + 15;
            }


            var hptlist = new List<KeyPoint>();
            for (var idx = 15; idx < edged.Height;)
            {
                var xlist = new List<double>();
                var hlist = new List<KeyPoint>();
                foreach (var pt in wptlist)
                {
                    if (pt.Pt.Y >= (idx - 15) && pt.Pt.Y < idx)
                    {
                        hlist.Add(pt);
                        xlist.Add(pt.Pt.X);
                    }
                }

                if (hlist.Count > 16 && (xlist.Max() - xlist.Min()) > 0.65 * edged.Width)
                { hptlist.AddRange(hlist); }
                idx = idx + 15;
            }

            if (hptlist.Count() == 0)
            {
                return ret;
            }

            //var dstKaze = new Mat();
            //Cv2.DrawKeypoints(edged, hptlist.ToArray(), dstKaze);

            //using (new Window("dstKaze", dstKaze))
            //{
            //    Cv2.WaitKey();
            //}

            var xxlist = new List<double>();
            var yylist = new List<double>();
            foreach (var pt in hptlist)
            {
                xxlist.Add(pt.Pt.X);
                yylist.Add(pt.Pt.Y);
            }
            ret.Add(xxlist);
            ret.Add(yylist);

            return ret;
        }


        private static List<List<double>> GetDetectPoint(Mat mat)
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

        //private static bool XUniformity(List<int> xlist, int widthlow, int widthhigh)
        //{
        //    if (xlist.Count == 4)
        //    {
        //        for (var idx = 1; idx < xlist.Count; idx++)
        //        {
        //            var delta = xlist[idx] - xlist[idx - 1];
        //            if (delta >= widthlow && delta < (widthhigh + 10))
        //            { }
        //            else
        //            { return false; }
        //        }
        //        return true;
        //    }
        //    else
        //    { return false; }
        //}

        //private static List<int> GetPossibleXList(Mat edged, int widthlow, int widthhigh)
        //{
        //    var outmat = new Mat();
        //    var ids = OutputArray.Create(outmat);
        //    var cons = new Mat[] { };
        //    Cv2.FindContours(edged, out cons, ids, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

        //    var cwlist = new List<int>();
        //    var y0list = new List<int>();
        //    var y1list = new List<int>();

        //    var idx1 = 0;
        //    foreach (var item in cons)
        //    {
        //        idx1++;
        //        var crect = Cv2.BoundingRect(item);
        //        if (crect.Width >= widthlow && crect.Width <= widthhigh)
        //        {
        //            if (crect.Y > (edged.Height / 2 - 30))
        //            {
        //                y1list.Add(crect.Y);
        //                cwlist.Add(crect.X);
        //            }
        //            else
        //            { y0list.Add(crect.Y); }
        //        }//end if
        //    }

        //    cwlist.Sort();

        //    if (XUniformity(cwlist, widthlow, widthhigh))
        //    { }
        //    else
        //    {
        //        cwlist.Clear();
        //        y0list.Clear();
        //        y1list.Clear();

        //        for (var idx = 0; idx < 4; idx++)
        //        {
        //            cwlist.Add(-1);
        //        }

        //        idx1 = 0;
        //        foreach (var item in cons)
        //        {
        //            idx1++;
        //            var crect = Cv2.BoundingRect(item);
        //            if (crect.Width >= widthlow && crect.Width <= widthhigh)
        //            {
        //                if (crect.Y > (edged.Height / 2 - 30))
        //                {
        //                    y1list.Add(crect.Y);
        //                    if (crect.X < 45)
        //                    {
        //                        cwlist[0] = crect.X;
        //                        cwlist[1] = crect.X + crect.Width;
        //                    }
        //                    else if (crect.X >= 50 && crect.X < 100)
        //                    {
        //                        cwlist[1] = crect.X;
        //                        cwlist[2] = crect.X + crect.Width;
        //                    }
        //                    else if (crect.X > 105 && crect.X < 160)
        //                    {
        //                        cwlist[2] = crect.X;
        //                        cwlist[3] = crect.X + crect.Width;
        //                    }
        //                    else if (crect.X > 165 && crect.X < 210)
        //                    { cwlist[3] = crect.X; }
        //                }
        //                else
        //                { y0list.Add(crect.Y); }

        //                //var mat = edged.SubMat(crect);
        //                //    using (new Window("edged" + idx1, mat))
        //                //    {
        //                //        Cv2.WaitKey();
        //                //    }
        //            }//end if
        //        }//end foreach
        //    }

        //    if (cwlist[3] == -1)
        //    { cwlist[3] = edged.Width - 60; }
        //    if (cwlist[2] == -1)
        //    { cwlist[2] = cwlist[3] - 60; }
        //    if (cwlist[1] == -1)
        //    { cwlist[1] = cwlist[2] - 60; }
        //    if (cwlist[0] == -1)
        //    { cwlist[0] = 5; }

        //    if (y0list.Count > 0)
        //    { cwlist.Add((int)y0list.Average()); }
        //    else
        //    { cwlist.Add(0); }

        //    if (y1list.Count > 0)
        //    { cwlist.Add((int)y1list.Average()); }
        //    else
        //    { cwlist.Add((int)(edged.Height * 0.55)); }

        //    return cwlist;
        //}

        //private static List<List<double>> GetDetectPoint(Mat mat)
        //{
        //    var ret = new List<List<double>>();
        //    var kaze = KAZE.Create();
        //    var kazeDescriptors = new Mat();
        //    KeyPoint[] kazeKeyPoints = null;
        //    kaze.DetectAndCompute(mat, null, out kazeKeyPoints, kazeDescriptors);
        //    var xlist = new List<double>();
        //    var ylist = new List<double>();
        //    foreach (var pt in kazeKeyPoints)
        //    {
        //        xlist.Add(pt.Pt.X);
        //        ylist.Add(pt.Pt.Y);
        //    }
        //    ret.Add(xlist);
        //    ret.Add(ylist);

        //    //var dstKaze = new Mat();
        //    //Cv2.DrawKeypoints(mat, kazeKeyPoints, dstKaze);

        //    //using (new Window("dstKaze", dstKaze))
        //    //{
        //    //    Cv2.WaitKey();
        //    //}

        //    return ret;
        //}

    }
}