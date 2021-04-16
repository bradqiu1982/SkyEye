using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenCvSharp;

namespace SkyEye.Models
{
    public class ImgOperateCircle2168
    {
        ////149,175,45.5,53,1.85,2.7,3.56
        //public static bool Detect2168Revision(string file,int wdlow,int wdhigh,double radlow,double radhigh, double xc, double ylc, double yhc)
        //{
        //    Mat srcorgimg = Cv2.ImRead(file, ImreadModes.Color);
        //    var detectsize = GetDetectPoint(srcorgimg);
        //    var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

        //    var srcgray = new Mat();
        //    Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);

        //    var circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, 40, 100);

        //    if (circles.Length == 0)
        //    { return false; }

        //    var circlesegment = circles[0];
        //    var imgmidy = srcgray.Height / 2;
        //    var rad = circlesegment.Radius;
        //    var x0 = circlesegment.Center.X - 1.85 * rad;
        //    var x1 = circlesegment.Center.X + 1.85 * rad;
        //    var y0 = circlesegment.Center.Y - 3.56 * rad;
        //    if (y0 < 0) { y0 = 0; }
        //    var y1 = circles[0].Center.Y - 2.7 * rad;

        //    if (circlesegment.Center.Y < imgmidy)
        //    {
        //        x0 = circlesegment.Center.X - 1.85 * rad;
        //        x1 = circlesegment.Center.X + 1.85 * rad;
        //        y0 = circles[0].Center.Y + 2.7 * rad;
        //        y1 = circlesegment.Center.Y + 3.56 * rad;
        //        if (y1 > srcgray.Height) { y1 = srcgray.Height; }
        //    }


        //    var coordinatemat = srcrealimg.SubMat((int)y0, (int)y1, (int)x0, (int)x1);

        //    if (circlesegment.Center.Y < imgmidy)
        //    {
        //        var center = new Point2f(coordinatemat.Width / 2, coordinatemat.Height / 2);
        //        var m = Cv2.GetRotationMatrix2D(center, 180, 1);
        //        var outxymat = new Mat();
        //        Cv2.WarpAffine(coordinatemat, outxymat, m, new Size(coordinatemat.Width, coordinatemat.Height));
        //        coordinatemat = outxymat;
        //    }

        //    var coordgray = new Mat();
        //    Cv2.CvtColor(coordinatemat, coordgray, ColorConversionCodes.BGR2GRAY);
        //    var coordblurred = new Mat();
        //    Cv2.GaussianBlur(coordgray, coordblurred, new Size(5, 5), 0);
        //    var coordedged = new Mat();
        //    Cv2.AdaptiveThreshold(coordblurred, coordedged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 7, 2);

        //    var coordwidth = CheckCoordWidth(coordedged);
        //    rad = circlesegment.Radius;

        //    if (coordwidth > wdlow && coordwidth < wdhigh
        //        && rad > radlow && rad < radhigh)
        //    { return true; }

        //    return false;
        //}

        //public static void Cutx(string imgpath)
        //{
        //    Mat src = Cv2.ImRead(imgpath, ImreadModes.Color);
        //    var detectsize = GetDetectPoint(src);
        //    var srcrealimg = src.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

        //    var xyenhance = new Mat();
        //    Cv2.DetailEnhance(srcrealimg, xyenhance);

        //    var denoisemat = new Mat();
        //    Cv2.FastNlMeansDenoisingColored(xyenhance, denoisemat, 10, 10, 7, 21);


        //    var xyenhgray = new Mat();
        //    Cv2.CvtColor(denoisemat, xyenhgray, ColorConversionCodes.BGR2GRAY);

        //    var blurred = new Mat();
        //    Cv2.GaussianBlur(xyenhgray, blurred, new Size(5, 5), 0);

        //    var edged = new Mat();
        //    Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);
        //    using (new Window("edged", edged))
        //    {
        //        Cv2.WaitKey();
        //    }

        //    var high = srcrealimg.Height;
        //    var minrad = (int)(high * 0.12) - 10;
        //    var maxrad = (int)(high * 0.155) + 10;

        //    var hl = 0.375 * high;
        //    var hh = 0.625 * high;

        //    ////edged.Rows / 8, 100, 70, 10, 800
        //    var circles = Cv2.HoughCircles(xyenhgray, HoughMethods.Gradient, 1, xyenhgray.Rows / 4, 100, 70, minrad, maxrad);
        //    foreach (var c in circles)
        //    {
        //        var centerh = (int)c.Center.Y;
        //        if (centerh >= hl && centerh <= hh)
        //        {
        //            Cv2.Circle(xyenhance, (int)c.Center.X, (int)c.Center.Y, (int)c.Radius, new Scalar(0, 255, 0), 3);
        //            using (new Window("srcimg", xyenhance))
        //            {
        //                Cv2.WaitKey();
        //            }
        //        }
        //    }
        //}

        ////38,64,50,100,1.85,2.7,3.56,40,60
        //public static List<Mat> CutCharRect(string file, int cwdlow, int cwdhigh, int chdlow, int chdhigh,double xc,double ylc,double yhc, double radlow, double radhigh)
        //{
        //    Mat srcorgimg = Cv2.ImRead(file, ImreadModes.Color);
        //    var detectsize = GetDetectPoint(srcorgimg);
        //    var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

        //    var srcgray = new Mat();
        //    Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);

        //    var circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, 40, 100);

        //    if (circles.Length == 0)
        //    { return new List<Mat>(); }

        //    var circlesegment = circles[0];
        //    var rad = circlesegment.Radius;
        //    if (rad < radlow || rad > radhigh)
        //    {
        //        circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, 40, 70);
        //        if (circles.Length == 0)
        //        { return new List<Mat>(); }
        //        circlesegment = circles[0];
        //        rad = circlesegment.Radius;
        //    }


        //    var imgmidy = srcgray.Height / 2;
        //    var x0 = circlesegment.Center.X - xc * rad;
        //    var x1 = circlesegment.Center.X + xc * rad;
        //    var y0 = circlesegment.Center.Y - yhc * rad;
        //    if (y0 < 0) { y0 = 0; }
        //    var y1 = circles[0].Center.Y - ylc * rad;

        //    if (circlesegment.Center.Y < imgmidy)
        //    {
        //        x0 = circlesegment.Center.X - xc * rad;
        //        x1 = circlesegment.Center.X + xc * rad;
        //        y0 = circles[0].Center.Y + ylc * rad;
        //        y1 = circlesegment.Center.Y + yhc * rad;
        //        if (y1 > srcgray.Height) { y1 = srcgray.Height; }
        //    }

        //    var coordinatemat = srcrealimg.SubMat((int)y0, (int)y1, (int)x0, (int)x1);
        //    if (circlesegment.Center.Y < imgmidy)
        //    {
        //        var center = new Point2f(coordinatemat.Width / 2, coordinatemat.Height / 2);
        //        var m = Cv2.GetRotationMatrix2D(center, 180, 1);
        //        var outxymat = new Mat();
        //        Cv2.WarpAffine(coordinatemat, outxymat, m, new Size(coordinatemat.Width, coordinatemat.Height));
        //        coordinatemat = outxymat;
        //    }

        //    var coordgray = new Mat();
        //    Cv2.CvtColor(coordinatemat, coordgray, ColorConversionCodes.BGR2GRAY);
        //    var coordblurred = new Mat();
        //    Cv2.GaussianBlur(coordgray, coordblurred, new Size(5, 5), 0);
        //    var coordedged = new Mat();
        //    Cv2.AdaptiveThreshold(coordblurred, coordedged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 7, 2);

        //    Cv2.Resize(coordedged, coordedged, new Size(coordedged.Width * 4, coordedged.Height * 4));

        //    var rectlist = GetPossibleXlistCircle2168(coordedged,cwdlow, cwdhigh, chdlow, chdhigh);

        //    if (rectlist.Count != 8)
        //    { return new List<Mat>(); }

        //    var cmatlist = new List<Mat>();
        //    cmatlist.Add(coordinatemat);

        //    foreach (var rect in rectlist)
        //    {
        //        cmatlist.Add(coordedged.SubMat(rect));
        //    }

        //    return cmatlist;
        //}


        //private static List<double> GetCoordWidthPT(Mat mat)
        //{
        //    var ret = new List<List<double>>();
        //    var kaze = KAZE.Create();
        //    var kazeDescriptors = new Mat();
        //    KeyPoint[] kazeKeyPoints = null;
        //    kaze.DetectAndCompute(mat, null, out kazeKeyPoints, kazeDescriptors);

        //    var hl = 0.25 * mat.Height;
        //    var hh = 0.75 * mat.Height;
        //    var wl = 10;
        //    var wh = mat.Width - 10;
        //    var hptlist = new List<KeyPoint>();
        //    foreach (var pt in kazeKeyPoints)
        //    {
        //        if (pt.Pt.Y >= hl && pt.Pt.Y <= hh
        //            && pt.Pt.X >= wl && pt.Pt.X <= wh)
        //        {
        //            hptlist.Add(pt);
        //        }
        //    }

        //    var wptlist = new List<KeyPoint>();
        //    for (var idx = 15; idx < mat.Width;)
        //    {
        //        var wlist = new List<KeyPoint>();
        //        foreach (var pt in hptlist)
        //        {
        //            if (pt.Pt.X >= (idx - 15) && pt.Pt.X < idx)
        //            {
        //                wlist.Add(pt);
        //            }
        //        }
        //        if (wlist.Count > 3)
        //        { wptlist.AddRange(wlist); }
        //        idx = idx + 15;
        //    }

        //    //var dstKaze = new Mat();
        //    //Cv2.DrawKeypoints(mat, wptlist.ToArray(), dstKaze);

        //    //using (new Window("dstKaze", dstKaze))
        //    //{
        //    //    Cv2.WaitKey();
        //    //}
        //    var xlist = new List<double>();
        //    foreach (var pt in wptlist)
        //    {
        //        xlist.Add(pt.Pt.X);
        //    }
        //    return xlist;
        //}

        ////38,64,50,100
        //private static List<Rect> GetPossibleXlistCircle2168(Mat edged,int wdlow,int wdhigh,int hdlow,int hdhigh)
        //{
        //    var outmat = new Mat();
        //    var ids = OutputArray.Create(outmat);
        //    var cons = new Mat[] { };
        //    Cv2.FindContours(edged, out cons, ids, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

        //    var cwlist = new List<int>();
        //    var y0list = new List<int>();
        //    var y1list = new List<int>();
        //    var wavglist = new List<int>();
        //    var rectlist = new List<Rect>();


        //    var idx1 = 0;
        //    foreach (var item in cons)
        //    {
        //        idx1++;

        //        var crect = Cv2.BoundingRect(item);
        //        if (crect.Width > wdlow && crect.Width < wdhigh && crect.Height > hdlow && crect.Height < hdhigh)
        //        {
        //            rectlist.Add(crect);

        //            cwlist.Add(crect.X);
        //            y0list.Add(crect.Y);
        //            wavglist.Add(crect.Width);
        //            y1list.Add(crect.Height);
        //        }
        //    }//end foreach

        //    if (rectlist.Count == 8)
        //    {
        //        rectlist.Sort(delegate (Rect r1, Rect r2) {
        //            return r1.X.CompareTo(r2.X);
        //        });
        //        return rectlist;
        //    }
        //    else
        //    {
        //        if (cwlist.Count == 0)
        //        { return new List<Rect>(); }
        //        cwlist.Sort();
        //        return GetGuessXlistCircle2168(edged, cwlist, y0list, y1list, wavglist);
        //    }
        //}

        //private static List<Rect> GetGuessXlistCircle2168(Mat edged, List<int> cwlist, List<int> y0list, List<int> y1list, List<int> wavglist)
        //{
        //    var rectlist = new List<Rect>();
        //    var xlist = GetCoordWidthPT(edged);
        //    var leftedge = xlist.Min();
        //    var rightedge = xlist.Max();

        //    var wavg = wavglist.Average() + 2;


        //    var assumexlist = new List<int>();
        //    for (var idx = 0; idx < 8; idx++)
        //    { assumexlist.Add(-1); }

        //    foreach (var val in cwlist)
        //    {
        //        if (val > (leftedge - 0.5 * wavg) && val < (leftedge + 0.5 * wavg))
        //        { assumexlist[0] = val; }
        //        if (val > (leftedge + 0.5 * wavg) && val < (leftedge + 1.5 * wavg))
        //        { assumexlist[1] = val; }
        //        if (val > (leftedge + 1.5 * wavg) && val < (leftedge + 2.5 * wavg))
        //        { assumexlist[2] = val; }
        //        if (val > (leftedge + 2.5 * wavg) && val < (leftedge + 3.5 * wavg))
        //        { assumexlist[3] = val; }

        //        if (val > (rightedge - 1.5 * wavg) && val < (rightedge - 0.5 * wavg))
        //        { assumexlist[7] = val; }
        //        if (val > (rightedge - 2.5 * wavg) && val < (rightedge - 1.5 * wavg))
        //        { assumexlist[6] = val; }
        //        if (val > (rightedge - 3.5 * wavg) && val < (rightedge - 2.5 * wavg))
        //        { assumexlist[5] = val; }
        //        if (val > (rightedge - 4.5 * wavg) && val < (rightedge - 3.5 * wavg))
        //        { assumexlist[4] = val; }
        //    }

        //    if (assumexlist[0] == -1)
        //    {
        //        if (assumexlist[1] != -1) { assumexlist[0] = assumexlist[1] - (int)wavg - 1; }
        //        else { assumexlist[0] = (int)leftedge - 2; }
        //    }
        //    if (assumexlist[1] == -1)
        //    {
        //        if (assumexlist[2] != -1) { assumexlist[1] = assumexlist[2] - (int)wavg - 1; }
        //        else { assumexlist[1] = assumexlist[0] + (int)wavg; }
        //    }
        //    if (assumexlist[2] == -1)
        //    {
        //        if (assumexlist[3] != -1)
        //        { assumexlist[2] = assumexlist[3] - (int)wavg - 1; }
        //        else
        //        { assumexlist[2] = assumexlist[1] + (int)wavg; }
        //    }
        //    if (assumexlist[3] == -1)
        //    { assumexlist[3] = assumexlist[2] + (int)wavg; }

        //    if (assumexlist[7] == -1)
        //    {
        //        if (assumexlist[6] != -1)
        //        { assumexlist[7] = assumexlist[6] + (int)wavg; }
        //        else
        //        { assumexlist[7] = (int)rightedge - (int)wavg - 1; }
        //    }
        //    if (assumexlist[6] == -1)
        //    {
        //        if (assumexlist[5] != -1)
        //        { assumexlist[6] = assumexlist[5] + (int)wavg; }
        //        else
        //        { assumexlist[6] = assumexlist[7] - (int)wavg - 1; }
        //    }
        //    if (assumexlist[5] == -1)
        //    {
        //        if (assumexlist[4] != -1)
        //        { assumexlist[5] = assumexlist[4] + (int)wavg; }
        //        else
        //        { assumexlist[5] = assumexlist[6] - (int)wavg - 1; }
        //    }
        //    if (assumexlist[4] == -1)
        //    { assumexlist[4] = assumexlist[5] - (int)wavg - 2; }

        //    var h0avg = (int)y0list.Average() - 1;
        //    var h1avg = (int)y1list.Average() + 1;

        //    rectlist.Clear();
        //    for (var idx = 0; idx < 8; idx++)
        //    {
        //        if (idx == 3)
        //        {
        //            rectlist.Add(new Rect(assumexlist[idx] - 1, h0avg, (int)wavg + 2, h1avg));
        //        }
        //        else if (idx == 7)
        //        { rectlist.Add(new Rect(assumexlist[idx] - 1, h0avg, (int)wavg + 2, h1avg)); }
        //        else
        //        { rectlist.Add(new Rect(assumexlist[idx] - 1, h0avg, assumexlist[idx + 1] - assumexlist[idx], h1avg)); }
        //    }

        //    return rectlist;
        //}


        //private static double CheckCoordWidth(Mat mat)
        //{
        //    var xlist = new List<double>();

        //    var ret = new List<List<double>>();
        //    var kaze = KAZE.Create();
        //    var kazeDescriptors = new Mat();
        //    KeyPoint[] kazeKeyPoints = null;
        //    kaze.DetectAndCompute(mat, null, out kazeKeyPoints, kazeDescriptors);

        //    var hl = 0.25 * mat.Height;
        //    var hh = 0.75 * mat.Height;
        //    var wl = 5;
        //    var wh = mat.Width - 5;
        //    var hptlist = new List<KeyPoint>();
        //    foreach (var pt in kazeKeyPoints)
        //    {
        //        if (pt.Pt.Y >= hl && pt.Pt.Y <= hh
        //            && pt.Pt.X >= wl && pt.Pt.X <= wh)
        //        {
        //            hptlist.Add(pt);
        //            xlist.Add(pt.Pt.X);
        //        }
        //    }

        //    //var dstKaze = new Mat();
        //    //Cv2.DrawKeypoints(mat, hptlist.ToArray(), dstKaze);

        //    //using (new Window("dstKaze", dstKaze))
        //    //{
        //    //    Cv2.WaitKey();
        //    //}

        //    return xlist.Max() - xlist.Min();
        //}

        public static string CheckRegion(Mat checkregion)
        {
            var xyenhance = new Mat();
            Cv2.DetailEnhance(checkregion, xyenhance);

            var denoisemat1 = new Mat();
            Cv2.FastNlMeansDenoisingColored(xyenhance, denoisemat1, 10, 10, 7, 21);
            xyenhance = denoisemat1;

            var xyenhance4x = new Mat();
            Cv2.Resize(xyenhance, xyenhance4x, new Size(xyenhance.Width * 4, xyenhance.Height * 4));
            Cv2.DetailEnhance(xyenhance4x, xyenhance4x);

            var xyenhgray = new Mat();
            var denoisemat = new Mat();
            Cv2.FastNlMeansDenoisingColored(xyenhance4x, denoisemat, 10, 10, 7, 21);
            Cv2.CvtColor(denoisemat, xyenhgray, ColorConversionCodes.BGR2GRAY);

            var blurred = new Mat();
            Cv2.GaussianBlur(xyenhgray, blurred, new Size(5, 5), 0);

            var edged = new Mat();
            Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

            for (var idx = 1; idx < edged.Height - 3; idx = idx + 3)
            {
                var snapmat = edged.SubMat(idx, idx + 3, 0, edged.Width);
                var cnt = snapmat.CountNonZero();
                if (cnt > 200)
                {
                    return "";
                }
            }

            return "OGP-circle2168";
        }

        public static string Detect2168Revision(string imgpath,bool fixangle)
        {
            Mat srccolor = Cv2.ImRead(imgpath, ImreadModes.Color);

            if (fixangle)
            {
                var angle = GetAngle2168(imgpath);
                if (angle >= 0.7 && angle <= 359.3)
                {
                    var center = new Point2f(srccolor.Width / 2, srccolor.Height / 2);
                    var m = Cv2.GetRotationMatrix2D(center, angle, 1);
                    var outxymat = new Mat();
                    Cv2.WarpAffine(srccolor, outxymat, m, new Size(srccolor.Width, srccolor.Height));
                    srccolor = outxymat;
                }
            }

            var detectsize = ImgPreOperate.GetImageBoundPointX(srccolor);
            var srcrealimg = srccolor.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var srcgray = new Mat();
            Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);
            var srcblurred = new Mat();
            Cv2.GaussianBlur(srcgray, srcblurred, new Size(5, 5), 0);
            var srcedged = new Mat();
            Cv2.AdaptiveThreshold(srcblurred, srcedged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

            var circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, 30, 70);

            var lowbond = srcrealimg.Height * 0.2;
            var upbond = srcrealimg.Height * 0.8;
            var midbond = srcrealimg.Height * 0.5;

            var middis = 0;
            var filtercircles = new List<CircleSegment>();
            foreach (var c in circles)
            {
                if (c.Center.Y > lowbond && c.Center.Y < upbond)
                {
                    if (middis == 0)
                    {
                        middis = Math.Abs((int)c.Center.Y - (int)midbond);
                        filtercircles.Add(c);
                    }
                    else
                    {
                        var tempdis = Math.Abs((int)c.Center.Y - (int)midbond);
                        if (tempdis < middis)
                        {
                            middis = tempdis;
                            filtercircles.Clear();
                            filtercircles.Add(c);
                        }
                    }
                }
            }

            if (filtercircles.Count > 0)
            {
                var CP = filtercircles[0];
                var hg = srcrealimg.Height;

                var lines = Cv2.HoughLinesP(srcedged, 1, Math.PI / 180.0, 50, 80, 5);
                var filterline = new List<LineSegmentPoint>();
                foreach (var line in lines)
                {
                    var degree = Math.Atan2((line.P2.Y - line.P1.Y), (line.P2.X - line.P1.X));
                    var d360 = (degree > 0 ? degree : (2 * Math.PI + degree)) * 360 / (2 * Math.PI);
                    var xlen = Math.Abs(line.P2.X - line.P1.X);
                    var ylen = CP.Center.Y - line.P1.Y;
                    if (CP.Center.Y < midbond)
                    { ylen = line.P1.Y - CP.Center.Y; }

                    if (xlen >= 166 && xlen < 240
                        && (d360 <= 4 || d360 >= 356)
                        && (ylen >= 170 && ylen <= 210))
                    {
                        filterline.Add(line);
                    }
                }

                if (filterline.Count > 0)
                {
                    var checkregion = new Mat();
                    var ylist = new List<int>();
                    foreach (var p in filterline)
                    { ylist.Add(p.P1.Y); ylist.Add(p.P2.Y); }
                    var boundy = (int)ylist.Average();
                    var midx = (int)CP.Center.X;

                    if (CP.Center.Y > midbond)
                    {
                        var colstart = midx - 15;
                        var colend = midx + 15;
                        var rowstart = boundy + 15;
                        var rowend = boundy + 40;
                        checkregion = srcrealimg.SubMat(rowstart, rowend, colstart, colend);
                    }
                    else
                    {
                        var colstart = midx - 15;
                        var colend = midx + 15;
                        var rowstart = boundy - 40;
                        var rowend = boundy - 15;
                        checkregion = srcrealimg.SubMat(rowstart, rowend, colstart, colend);
                    }

                    return CheckRegion(checkregion);
                }//end if
            }//end if

            return "";
        }

        public static List<Mat> CutCharRect(string imgpath,bool fixangle)
        {
            Mat srccolor = Cv2.ImRead(imgpath, ImreadModes.Color);

            if (fixangle)
            {
                var angle = GetAngle2168(imgpath);
                if (angle >= 0.7 && angle <= 359.3)
                {
                    var center = new Point2f(srccolor.Width / 2, srccolor.Height / 2);
                    var m = Cv2.GetRotationMatrix2D(center, angle, 1);
                    var outxymat = new Mat();
                    Cv2.WarpAffine(srccolor, outxymat, m, new Size(srccolor.Width, srccolor.Height));
                    srccolor = outxymat;
                }
            }

            var detectsize = ImgPreOperate.GetImageBoundPointX(srccolor);
            var srcrealimg = srccolor.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var srcgray = new Mat();
            Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);
            var srcblurred = new Mat();
            Cv2.GaussianBlur(srcgray, srcblurred, new Size(5, 5), 0);
            var srcedged = new Mat();
            Cv2.AdaptiveThreshold(srcblurred, srcedged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

            var circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, 30, 70);

            var lowbond = srcrealimg.Height * 0.2;
            var upbond = srcrealimg.Height * 0.8;
            var midbond = srcrealimg.Height * 0.5;

            var middis = 0;
            var filtercircles = new List<CircleSegment>();
            foreach (var c in circles)
            {
                if (c.Center.Y > lowbond && c.Center.Y < upbond)
                {
                    if (c.Center.Y > lowbond && c.Center.Y < upbond)
                    {
                        if (middis == 0)
                        {
                            middis = Math.Abs((int)c.Center.Y - (int)midbond);
                            filtercircles.Add(c);
                        }
                        else
                        {
                            var tempdis = Math.Abs((int)c.Center.Y - (int)midbond);
                            if (tempdis < middis)
                            {
                                middis = tempdis;
                                filtercircles.Clear();
                                filtercircles.Add(c);
                            }
                        }
                    }
                }
            }

            if (filtercircles.Count > 0)
            {
                var CP = filtercircles[0];
                var hg = srcrealimg.Height;

                var lines = Cv2.HoughLinesP(srcedged, 1, Math.PI / 180.0, 50, 80, 5);
                var filterline = new List<LineSegmentPoint>();
                foreach (var line in lines)
                {
                    var degree = Math.Atan2((line.P2.Y - line.P1.Y), (line.P2.X - line.P1.X));
                    var d360 = (degree > 0 ? degree : (2 * Math.PI + degree)) * 360 / (2 * Math.PI);
                    var xlen = Math.Abs(line.P2.X - line.P1.X);
                    var ylen = CP.Center.Y - line.P1.Y;
                    if (CP.Center.Y < midbond)
                    { ylen = line.P1.Y - CP.Center.Y; }

                    if (xlen > 150 && xlen < 240  //xlen >= 166 && xlen < 240
                        && (d360 <= 4 || d360 >= 356)
                        && (ylen >= 170 && ylen <= 210))
                    {
                        filterline.Add(line);
                    }
                }

                if (filterline.Count > 0)
                {
                    var yblist = new List<int>();
                    foreach (var p in filterline)
                    { yblist.Add(p.P1.Y); yblist.Add(p.P2.Y); }

                    var coormat = new Mat();
                    var boundy = (int)yblist.Average();
                    var midx = (int)CP.Center.X;
                    if (CP.Center.Y > midbond)
                    {
                        var colstart = midx - 120;
                        var colend = midx + 120;
                        var rowstart = boundy + 3;
                        var rowend = boundy + 58;

                        if (colstart < 0 || colend > srcrealimg.Width)
                        {
                            if (srcrealimg.Width - midx > midx)
                            {
                                colstart = 1;
                                colend = 2 * midx - 1;
                            }
                            else
                            {
                                colstart = 2 * midx - srcrealimg.Width + 1;
                                colend = srcrealimg.Width - 1;
                            }
                            rowstart = boundy + 3;
                            rowend = boundy + 46;
                        }

                        coormat = srcrealimg.SubMat(rowstart, rowend, colstart, colend);
                    }
                    else
                    {
                        var colstart = midx - 120;
                        var colend = midx + 120;
                        var rowstart = boundy - 58;
                        var rowend = boundy - 3;

                        if (colstart < 0 || colend > srcrealimg.Width)
                        {
                            if (srcrealimg.Width - midx > midx)
                            {
                                colstart = 1;
                                colend = 2 * midx - 1;
                            }
                            else
                            {
                                colstart = 2 * midx - srcrealimg.Width + 1;
                                colend = srcrealimg.Width - 1;
                            }
                            rowstart = boundy - 46;
                            rowend = boundy - 3;
                        }

                        coormat = srcrealimg.SubMat(rowstart, rowend, colstart, colend);
                        var outxymat = new Mat();
                        Cv2.Transpose(coormat, outxymat);
                        Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                        Cv2.Transpose(outxymat, outxymat);
                        Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                        coormat = outxymat;
                    }

                    //using (new Window("coormat", coormat))
                    //{
                    //    Cv2.WaitKey();
                    //}
                    var ylen = CP.Center.Y - boundy;
                    if (CP.Center.Y < midbond)
                    { ylen = boundy - CP.Center.Y; }

                    return Get2168MatList(coormat,(int)ylen);
                    //var idx = 0;
                    //foreach (var cm in charmatlist)
                    //{
                    //    if (idx == 0)
                    //    { idx++; continue; }

                    //    var tcm = new Mat();
                    //    cm.ConvertTo(tcm, MatType.CV_32FC1);
                    //    var tcmresize = new Mat();
                    //    Cv2.Resize(tcm, tcmresize, new Size(50, 50), 0, 0, InterpolationFlags.Linear);

                    //    using (new Window("cmresize1" + idx, tcmresize))
                    //    {
                    //        Cv2.WaitKey();
                    //    }

                    //    idx++;
                    //}//end foreach
                }//end line
            }//end circle

            return new List<Mat>();
        }

        private static List<Mat> Get2168MatList(Mat coordmat,int ylen)
        {
            var cmatlist = new List<Mat>();

            var sharpimg = new Mat();
            Cv2.GaussianBlur(coordmat, sharpimg, new Size(0, 0), 3);
            Cv2.AddWeighted(coordmat, 2.0, sharpimg, -0.4, 0, sharpimg);

            var xyenhance4x = new Mat();
            Cv2.DetailEnhance(sharpimg, sharpimg);
            Cv2.Resize(sharpimg, xyenhance4x, new Size(coordmat.Width * 4, coordmat.Height * 4));

            var xyenhgray = new Mat();
            var denoisemat = new Mat();
            //Cv2.FastNlMeansDenoisingColored(xyenhance4x, denoisemat, 10, 10, 7, 21);
            Cv2.MedianBlur(xyenhance4x, denoisemat, 9);
            Cv2.CvtColor(denoisemat, xyenhgray, ColorConversionCodes.BGR2GRAY);


            var blurred = new Mat();
            Cv2.GaussianBlur(xyenhgray, blurred, new Size(7, 7), 0);

            var edged = new Mat();
            Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);
                
            //using (new Window("edged", edged))
            //{
            //    Cv2.WaitKey();
            //}

            var rectlist = Get2168Rect(edged, xyenhance4x);

            cmatlist.Add(sharpimg);
            foreach (var rect in rectlist)
            {
                if (rect.X < 0 || rect.Y < 0
                || ((rect.X + rect.Width) > edged.Width)
                || ((rect.Y + rect.Height) > edged.Height))
                {
                    cmatlist.Clear();
                    return cmatlist;
                }

                cmatlist.Add(edged.SubMat(rect));
            }

            return cmatlist;
        }

        private static List<Mat> Get2168MatList1(Mat coordmat, int ylen)
        {
            var cmatlist = new List<Mat>();

            var xyenhance = coordmat;
            var xyenhance4x = new Mat();
            Cv2.Resize(xyenhance, xyenhance4x, new Size(xyenhance.Width * 4, xyenhance.Height * 4));
            Cv2.DetailEnhance(xyenhance4x, xyenhance4x);

            var lowspec = new Scalar(23, 0, 0);
            //var highspec = new Scalar(100, 67, 65);
            var highspec = new Scalar(143, 63, 56);
            var coordrgb = new Mat();
            Cv2.CvtColor(xyenhance4x, coordrgb, ColorConversionCodes.BGR2RGB);
            var edged = coordrgb.InRange(lowspec, highspec);

            //using (new Window("edged", edged))
            //{
            //    Cv2.WaitKey();
            //}

            var rectlist = Get2168Rect(edged, xyenhance4x);

            cmatlist.Add(xyenhance);
            foreach (var rect in rectlist)
            {
                if (rect.X < 0 || rect.Y < 0
                || ((rect.X + rect.Width) > edged.Width)
                || ((rect.Y + rect.Height) > edged.Height))
                {
                    cmatlist.Clear();
                    return cmatlist;
                }

                cmatlist.Add(edged.SubMat(rect));
            }

            return cmatlist;
        }

        private static List<Rect> Get2168Rect(Mat edged, Mat xyenhance4x)
        {
            var hl = GetHeighLow2168(edged);
            var hh = GetHeighHigh2168(edged);


            var dcl = hl;//(int)(hl + (hh - hl) * 0.1);
            var dch = hh;//(int)(hh - (hh - hl) * 0.1);
            var xxh = GetXXHigh2168(edged, dcl, dch);
            var yxl = GetYXLow2168(edged, dcl, dch);

            if (xxh == -1 || yxl == -1)
            {
                var xlist = GetCoordWidthPT2168(xyenhance4x, edged);
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
            else
            {
                //avoid contamination at coord center
                //var wml = edged.Width / 2;
                //var xdist = wml - xxh;
                //var ydist = yxl - wml;
                //if (Math.Abs(xdist - ydist) > 90)
                //{
                //    if (xdist > ydist)
                //    { yxl = wml + xdist; }
                //    else
                //    { xxh = wml - ydist; }
                //}
            }

            var rectlist = new List<Rect>();

            var xxlist = GetXSplitList2168(edged, xxh, hl, hh);
            var flist = (List<int>)xxlist[0];
            var slist = (List<int>)xxlist[1];
            var y = hl - 7;
            var h = hh - hl + 9;

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
            else if (slist.Count == 2)
            {
                var fntw = (int)flist.Average();
                var left = slist[1] - 2 * fntw - 10;
                if (left < 0) { left = 1; }
                rectlist.Add(new Rect(left, y, fntw + 4, h));
                rectlist.Add(new Rect(slist[1] - fntw - 6, y, fntw + 1, h));
                rectlist.Add(new Rect(slist[1] - 6, y, slist[0] - slist[1], h));
                rectlist.Add(new Rect(slist[0] - 3, y, xxh - slist[0] + 8, h));
            }
            else
            {
                if ((int)xxh - 226 > 0)
                { rectlist.Add(new Rect(xxh - 226, y, 56, h)); }
                else
                { rectlist.Add(new Rect(0, y, 56, h)); }
                rectlist.Add(new Rect(xxh - 174, y, 56, h));
                rectlist.Add(new Rect(xxh - 112, y, 56, h));
                rectlist.Add(new Rect(xxh - 54, y, 56, h));
            }

            var yxlist = GetYSplitList2168(edged, yxl, hl, hh);
            flist = (List<int>)yxlist[0];
            slist = (List<int>)yxlist[1];
            if (slist.Count == 4)
            {
                rectlist.Add(new Rect(yxl - 3, y, slist[0] - yxl + 6, h));
                rectlist.Add(new Rect(slist[0] + 3, y, slist[1] - slist[0] + 3, h));
                rectlist.Add(new Rect(slist[1] + 3, y, slist[2] - slist[1] + 3, h));
                rectlist.Add(new Rect(slist[2] + 3, y, slist[3] - slist[2] + 4, h));
            }
            else if (slist.Count == 3)
            {
                var fntw = (int)flist.Average();
                rectlist.Add(new Rect(yxl - 3, y, slist[0] - yxl + 6, h));
                rectlist.Add(new Rect(slist[0] + 5, y, slist[1] - slist[0] + 4, h));
                rectlist.Add(new Rect(slist[1] + 5, y, slist[2] - slist[1] + 4, h));
                var left = slist[2] + 5;
                if (left + fntw + 4 > edged.Width)
                { left = edged.Width - fntw - 4; }
                rectlist.Add(new Rect(left, y, fntw + 4, h));
            }
            else if (slist.Count == 2)
            {
                var fntw = (int)flist.Average();
                rectlist.Add(new Rect(yxl - 3, y, slist[0] - yxl + 6, h));
                rectlist.Add(new Rect(slist[0] + 5, y, slist[1] - slist[0] + 4, h));
                rectlist.Add(new Rect(slist[1] + 5, y, fntw + 4, h));
                var left = slist[1] + fntw + 12;
                if (left + fntw + 4 > edged.Width)
                { left = edged.Width - fntw - 4; }
                rectlist.Add(new Rect(left, y, fntw + 4, h));
            }
            else
            {
                rectlist.Add(new Rect(yxl - 2, y, 56, h));
                rectlist.Add(new Rect(yxl + 54, y, 56, h));
                rectlist.Add(new Rect(yxl + 113, y, 56, h));
                if ((yxl + 226) >= (edged.Cols - 1))
                { rectlist.Add(new Rect(yxl + 170, y, edged.Cols - yxl - 170, h)); }
                else
                { rectlist.Add(new Rect(yxl + 170, y, 54, h)); }
            }
            return rectlist;
        }

        private static List<double> GetCoordWidthPT2168(Mat mat, Mat edged)
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

            var dstKaze = new Mat();
            Cv2.DrawKeypoints(mat, xyptlist.ToArray(), dstKaze);

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

        private static int GetHeighLow2168(Mat edged)
        {
            var cheighxl = (int)(edged.Width * 0.15);
            var cheighxh = (int)(edged.Width * 0.33);
            var cheighyl = (int)(edged.Width * 0.66);
            var cheighyh = (int)(edged.Width * 0.84);

            var xhl = 0;
            var yhl = 0;
            var ymidx = (int)(edged.Height * 0.4);
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

        private static int GetHeighHigh2168(Mat edged)
        {
            var cheighxl = (int)(edged.Width * 0.15);
            var cheighxh = (int)(edged.Width * 0.33);
            var cheighyl = (int)(edged.Width * 0.66);
            var cheighyh = (int)(edged.Width * 0.84);

            var xhh = 0;
            var yhh = 0;
            var ymidx = (int)(edged.Height * 0.4);
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

        private static int GetXXHigh2168(Mat edged, int dcl, int dch)
        {
            var ret = -1;
            var tm = 0;
            var wml = (int)(edged.Width * 0.25);
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

        private static int GetYXLow2168(Mat edged, int dcl, int dch)
        {
            var ret = -1;
            var tm = 0;
            var wml = (int)(edged.Width * 0.5);
            var wmh = (int)(edged.Width * 0.75);

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

        private static int GetXDirectSplit2168(Mat edged, int start, int end, int dcl, int dch, int previous)
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

        private static int GetYDirectSplit2168(Mat edged, int start, int end, int dcl, int dch, int previous)
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

        private static List<object> GetXSplitList2168(Mat edged, int xxh, int hl, int hh)
        {
            var offset = 50;
            var ret = new List<object>();
            var flist = new List<int>();
            var slist = new List<int>();
            ret.Add(flist);
            ret.Add(slist);

            var fntw = (int)(edged.Width * 0.333 * 0.25);

            var spx1 = GetXDirectSplit2168(edged, xxh - 20, xxh - 20 - fntw, hl, hh, xxh);
            if (spx1 == -1) { return ret; }
            fntw = xxh - spx1 + 1;
            if (fntw >= 18 && fntw < 38)
            { spx1 = xxh - offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spx1);

            var spx2 = GetXDirectSplit2168(edged, spx1 - 28, spx1 - 28 - fntw, hl, hh, spx1);
            if (spx2 == -1) { return ret; }
            fntw = spx1 - spx2;
            if (fntw >= 18 && fntw < 38)
            { spx2 = spx1 - offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spx2);

            var spx3 = GetXDirectSplit2168(edged, spx2 - 28, spx2 - 28 - fntw, hl, hh, spx2);
            if (spx3 == -1) { return ret; }
            fntw = spx2 - spx3;
            if (fntw >= 18 && fntw < 38)
            { spx3 = spx2 - offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spx3);

            return ret;
        }
        private static List<object> GetYSplitList2168(Mat edged, int yxl, int hl, int hh)
        {
            var offset = 50;
            var ret = new List<object>();
            var flist = new List<int>();
            var slist = new List<int>();
            ret.Add(flist);
            ret.Add(slist);

            var fntw = (int)(edged.Width * 0.333 * 0.25);

            var spy1 = GetYDirectSplit2168(edged, yxl + 28, yxl + 28 + fntw, hl, hh, yxl);
            if (spy1 == -1) { return ret; }
            fntw = spy1 - yxl + 1;
            if (fntw >= 18 && fntw < 38)
            { spy1 = yxl + offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spy1);

            var spy2 = GetYDirectSplit2168(edged, spy1 + 28, spy1 + 28 + fntw, hl, hh, spy1);
            if (spy2 == -1) { return ret; }
            fntw = spy2 - spy1 + 1;
            if (fntw >= 18 && fntw < 38)
            { spy2 = spy1 + offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spy2);

            var spy3 = GetYDirectSplit2168(edged, spy2 + 28, spy2 + 28 + fntw, hl, hh, spy2);
            if (spy3 == -1) { return ret; }
            fntw = spy3 - spy2 + 1;
            if (fntw >= 18 && fntw < 38)
            { spy3 = spy2 + offset; fntw = offset; }
            flist.Add(fntw); slist.Add(spy3);

            var spy4 = GetYDirectSplit2168(edged, spy3 + 28, edged.Width - 10, hl, hh, spy3);
            if (spy4 == -1) { return ret; }
            fntw = spy4 - spy3 + 1;
            if (fntw < 40)
            { return ret; }
            flist.Add(fntw); slist.Add(spy4);

            return ret;
        }


        //private static int GetXDirectSplit2168(Mat edged, int start, int end, int dcl, int dch)
        //{
        //    var ret = -1;
        //    for (var idx = start; idx > end; idx = idx - 2)
        //    {
        //        var snapmat = edged.SubMat(dcl, dch, idx - 2, idx);
        //        var cnt = snapmat.CountNonZero();
        //        if (cnt < 2)
        //        {
        //            if (ret == -1)
        //            { ret = idx; }
        //            else
        //            { return ret; }
        //        }
        //        else
        //        { ret = -1; }
        //    }
        //    return -1;
        //}

        //private static int GetYDirectSplit2168(Mat edged, int start, int end, int dcl, int dch)
        //{
        //    var ret = -1;
        //    for (var idx = start; idx < end; idx = idx + 2)
        //    {
        //        var snapmat = edged.SubMat(dcl, dch, idx, idx + 2);
        //        var cnt = snapmat.CountNonZero();
        //        if (cnt < 2)
        //        {
        //            if (ret == -1)
        //            { ret = idx; }
        //            else
        //            { return ret; }
        //        }
        //        else
        //        { ret = -1; }
        //    }
        //    return -1;
        //}

        //private static List<object> GetXSplitList2168(Mat edged, int xxh, int hl, int hh)
        //{
        //    var offset = 50;
        //    var ret = new List<object>();
        //    var flist = new List<int>();
        //    var slist = new List<int>();
        //    ret.Add(flist);
        //    ret.Add(slist);

        //    var fntw = (int)(edged.Width * 0.333 * 0.25);

        //    var spx1 = GetXDirectSplit2168(edged, xxh - 20, xxh - 20 - fntw, hl, hh);
        //    if (spx1 == -1) { return ret; }
        //    fntw = xxh - spx1 + 1;
        //    if (fntw >= 18 && fntw < 38)
        //    { spx1 = xxh - offset; fntw = offset; }
        //    flist.Add(fntw); slist.Add(spx1);

        //    var spx2 = GetXDirectSplit2168(edged, spx1 - 28, spx1 - 28 - fntw, hl, hh);
        //    if (spx2 == -1) { return ret; }
        //    fntw = spx1 - spx2;
        //    if (fntw >= 18 && fntw < 38)
        //    { spx2 = spx1 - offset; fntw = offset; }
        //    flist.Add(fntw); slist.Add(spx2);

        //    var spx3 = GetXDirectSplit2168(edged, spx2 - 28, spx2 - 28 - fntw, hl, hh);
        //    if (spx3 == -1) { return ret; }
        //    fntw = spx2 - spx3;
        //    if (fntw >= 18 && fntw < 38)
        //    { spx3 = spx2 - offset; fntw = offset; }
        //    flist.Add(fntw); slist.Add(spx3);

        //    return ret;
        //}
        //private static List<object> GetYSplitList2168(Mat edged, int yxl, int hl, int hh)
        //{
        //    var offset = 50;
        //    var ret = new List<object>();
        //    var flist = new List<int>();
        //    var slist = new List<int>();
        //    ret.Add(flist);
        //    ret.Add(slist);

        //    var fntw = (int)(edged.Width * 0.333 * 0.25);

        //    var spy1 = GetYDirectSplit2168(edged, yxl + 28, yxl + 28 + fntw, hl, hh);
        //    if (spy1 == -1) { return ret; }
        //    fntw = spy1 - yxl + 1;
        //    if (fntw >= 18 && fntw < 38)
        //    { spy1 = yxl + offset; fntw = offset; }
        //    flist.Add(fntw); slist.Add(spy1);

        //    var spy2 = GetYDirectSplit2168(edged, spy1 + 28, spy1 + 28 + fntw, hl, hh);
        //    if (spy2 == -1) { return ret; }
        //    fntw = spy2 - spy1 + 1;
        //    if (fntw >= 18 && fntw < 38)
        //    { spy2 = spy1 + offset; fntw = offset; }
        //    flist.Add(fntw); slist.Add(spy2);

        //    var spy3 = GetYDirectSplit2168(edged, spy2 + 28, spy2 + 28 + fntw, hl, hh);
        //    if (spy3 == -1) { return ret; }
        //    fntw = spy3 - spy2 + 1;
        //    if (fntw >= 18 && fntw < 38)
        //    { spy3 = spy2 + offset; fntw = offset; }
        //    flist.Add(fntw); slist.Add(spy3);

        //    var spy4 = GetYDirectSplit2168(edged, spy3 + 28, edged.Width - 10, hl, hh);
        //    if (spy4 == -1) { return ret; }
        //    fntw = spy4 - spy3 + 1;
        //    if (fntw < 40)
        //    { return ret; }
        //    flist.Add(fntw); slist.Add(spy4);

        //    return ret;
        //}

        private static double GetAngle2168(string imgpath)
        {
            Mat srcimg = Cv2.ImRead(imgpath, ImreadModes.Color);

            var detectsize = ImgPreOperate.GetImageBoundPointX(srcimg);
            srcimg = srcimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var src = new Mat();
            Cv2.CvtColor(srcimg, src, ColorConversionCodes.BGR2GRAY);

            var blurred = new Mat();
            Cv2.GaussianBlur(src, blurred, new Size(5, 5), 0);
            var edged = new Mat();
            Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

            //using (new Window("edged", edged))
            //{
            //    Cv2.WaitKey();
            //}
            var hg = srcimg.Height;
            var lines = Cv2.HoughLinesP(edged, 1, Math.PI / 180.0, 50, 80, 5);
            foreach (var line in lines)
            {
                var degree = Math.Atan2((line.P2.Y - line.P1.Y), (line.P2.X - line.P1.X));
                var d360 = (degree > 0 ? degree : (2 * Math.PI + degree)) * 360 / (2 * Math.PI);
                if (d360 > 20 && d360 < 340)
                { continue; }

                if (d360 <= 4 || d360 >= 356)
                {

                    var xlen = line.P2.X - line.P1.X;
                    if (xlen > 180 && xlen < 240
                        && ((line.P1.Y > 30 && line.P1.Y < 100) || (line.P1.Y < hg - 30 && line.P1.Y > hg - 100)))
                    {
                        return d360;
                    }
                }
            }

            return 0;
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

        //    var hptlist = new List<KeyPoint>();
        //    for (var idx = 20; idx < mat.Height;)
        //    {
        //        var xwlist = new List<double>();
        //        var wlist = new List<KeyPoint>();
        //        foreach (var pt in wptlist)
        //        {
        //            if (pt.Pt.Y >= (idx - 20) && pt.Pt.Y < idx)
        //            {
        //                wlist.Add(pt);
        //                xwlist.Add(pt.Pt.X);
        //            }
        //        }

        //        if (wlist.Count >= 2 && (xwlist.Max() - xwlist.Min()) > 0.3 * mat.Width)
        //        { hptlist.AddRange(wlist); }
        //        idx = idx + 20;
        //    }

        //    var xlist = new List<double>();
        //    var ylist = new List<double>();
        //    foreach (var pt in hptlist)
        //    {
        //        xlist.Add(pt.Pt.X);
        //        ylist.Add(pt.Pt.Y);
        //    }
        //    ret.Add(xlist);
        //    ret.Add(ylist);

        //    //var dstKaze = new Mat();
        //    //Cv2.DrawKeypoints(mat, wptlist, dstKaze);

        //    //using (new Window("dstKazexx", dstKaze))
        //    //{
        //    //    Cv2.WaitKey();
        //    //}

        //    return ret;
        //}
    }
}