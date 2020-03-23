using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenCvSharp;

namespace SkyEye.Models
{
    public class ImgOperateCircle2168
    {
        //149,175,45.5,53,1.85,2.7,3.56
        public static bool Detect2168Revision(string file,int wdlow,int wdhigh,double radlow,double radhigh, double xc, double ylc, double yhc)
        {
            Mat srcorgimg = Cv2.ImRead(file, ImreadModes.Color);
            var detectsize = GetDetectPoint(srcorgimg);
            var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var srcgray = new Mat();
            Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);

            var circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, 40, 100);

            if (circles.Length == 0)
            { return false; }

            var circlesegment = circles[0];
            var imgmidy = srcgray.Height / 2;
            var rad = circlesegment.Radius;
            var x0 = circlesegment.Center.X - 1.85 * rad;
            var x1 = circlesegment.Center.X + 1.85 * rad;
            var y0 = circlesegment.Center.Y - 3.56 * rad;
            if (y0 < 0) { y0 = 0; }
            var y1 = circles[0].Center.Y - 2.7 * rad;

            if (circlesegment.Center.Y < imgmidy)
            {
                x0 = circlesegment.Center.X - 1.85 * rad;
                x1 = circlesegment.Center.X + 1.85 * rad;
                y0 = circles[0].Center.Y + 2.7 * rad;
                y1 = circlesegment.Center.Y + 3.56 * rad;
                if (y1 > srcgray.Height) { y1 = srcgray.Height; }
            }


            var coordinatemat = srcrealimg.SubMat((int)y0, (int)y1, (int)x0, (int)x1);

            if (circlesegment.Center.Y < imgmidy)
            {
                var center = new Point2f(coordinatemat.Width / 2, coordinatemat.Height / 2);
                var m = Cv2.GetRotationMatrix2D(center, 180, 1);
                var outxymat = new Mat();
                Cv2.WarpAffine(coordinatemat, outxymat, m, new Size(coordinatemat.Width, coordinatemat.Height));
                coordinatemat = outxymat;
            }

            var coordgray = new Mat();
            Cv2.CvtColor(coordinatemat, coordgray, ColorConversionCodes.BGR2GRAY);
            var coordblurred = new Mat();
            Cv2.GaussianBlur(coordgray, coordblurred, new Size(5, 5), 0);
            var coordedged = new Mat();
            Cv2.AdaptiveThreshold(coordblurred, coordedged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 7, 2);

            var coordwidth = CheckCoordWidth(coordedged);
            rad = circlesegment.Radius;

            if (coordwidth > wdlow && coordwidth < wdhigh
                && rad > radlow && rad < radhigh)
            { return true; }

            return false;
        }

        //38,64,50,100,1.85,2.7,3.56,40,60
        public static List<Mat> CutCharRect(string file, int cwdlow, int cwdhigh, int chdlow, int chdhigh,double xc,double ylc,double yhc, double radlow, double radhigh)
        {
            Mat srcorgimg = Cv2.ImRead(file, ImreadModes.Color);
            var detectsize = GetDetectPoint(srcorgimg);
            var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var srcgray = new Mat();
            Cv2.CvtColor(srcrealimg, srcgray, ColorConversionCodes.BGR2GRAY);

            var circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, 40, 100);

            if (circles.Length == 0)
            { return new List<Mat>(); }

            var circlesegment = circles[0];
            var rad = circlesegment.Radius;
            if (rad < radlow || rad > radhigh)
            {
                circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 4, 100, 80, 40, 70);
                if (circles.Length == 0)
                { return new List<Mat>(); }
                circlesegment = circles[0];
                rad = circlesegment.Radius;
            }


            var imgmidy = srcgray.Height / 2;
            var x0 = circlesegment.Center.X - xc * rad;
            var x1 = circlesegment.Center.X + xc * rad;
            var y0 = circlesegment.Center.Y - yhc * rad;
            if (y0 < 0) { y0 = 0; }
            var y1 = circles[0].Center.Y - ylc * rad;

            if (circlesegment.Center.Y < imgmidy)
            {
                x0 = circlesegment.Center.X - xc * rad;
                x1 = circlesegment.Center.X + xc * rad;
                y0 = circles[0].Center.Y + ylc * rad;
                y1 = circlesegment.Center.Y + yhc * rad;
                if (y1 > srcgray.Height) { y1 = srcgray.Height; }
            }

            var coordinatemat = srcrealimg.SubMat((int)y0, (int)y1, (int)x0, (int)x1);
            if (circlesegment.Center.Y < imgmidy)
            {
                var center = new Point2f(coordinatemat.Width / 2, coordinatemat.Height / 2);
                var m = Cv2.GetRotationMatrix2D(center, 180, 1);
                var outxymat = new Mat();
                Cv2.WarpAffine(coordinatemat, outxymat, m, new Size(coordinatemat.Width, coordinatemat.Height));
                coordinatemat = outxymat;
            }

            var coordgray = new Mat();
            Cv2.CvtColor(coordinatemat, coordgray, ColorConversionCodes.BGR2GRAY);
            var coordblurred = new Mat();
            Cv2.GaussianBlur(coordgray, coordblurred, new Size(5, 5), 0);
            var coordedged = new Mat();
            Cv2.AdaptiveThreshold(coordblurred, coordedged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 7, 2);

            Cv2.Resize(coordedged, coordedged, new Size(coordedged.Width * 4, coordedged.Height * 4));

            var rectlist = GetPossibleXlistCircle2168(coordedged,cwdlow, cwdhigh, chdlow, chdhigh);

            if (rectlist.Count != 8)
            { return new List<Mat>(); }

            var cmatlist = new List<Mat>();
            cmatlist.Add(coordinatemat);

            foreach (var rect in rectlist)
            {
                cmatlist.Add(coordedged.SubMat(rect));
            }

            return cmatlist;
        }


        private static List<double> GetCoordWidthPT(Mat mat)
        {
            var ret = new List<List<double>>();
            var kaze = KAZE.Create();
            var kazeDescriptors = new Mat();
            KeyPoint[] kazeKeyPoints = null;
            kaze.DetectAndCompute(mat, null, out kazeKeyPoints, kazeDescriptors);

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

            var wptlist = new List<KeyPoint>();
            for (var idx = 15; idx < mat.Width;)
            {
                var wlist = new List<KeyPoint>();
                foreach (var pt in hptlist)
                {
                    if (pt.Pt.X >= (idx - 15) && pt.Pt.X < idx)
                    {
                        wlist.Add(pt);
                    }
                }
                if (wlist.Count > 3)
                { wptlist.AddRange(wlist); }
                idx = idx + 15;
            }

            //var dstKaze = new Mat();
            //Cv2.DrawKeypoints(mat, wptlist.ToArray(), dstKaze);

            //using (new Window("dstKaze", dstKaze))
            //{
            //    Cv2.WaitKey();
            //}
            var xlist = new List<double>();
            foreach (var pt in wptlist)
            {
                xlist.Add(pt.Pt.X);
            }
            return xlist;
        }

        //38,64,50,100
        private static List<Rect> GetPossibleXlistCircle2168(Mat edged,int wdlow,int wdhigh,int hdlow,int hdhigh)
        {
            var outmat = new Mat();
            var ids = OutputArray.Create(outmat);
            var cons = new Mat[] { };
            Cv2.FindContours(edged, out cons, ids, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            var cwlist = new List<int>();
            var y0list = new List<int>();
            var y1list = new List<int>();
            var wavglist = new List<int>();
            var rectlist = new List<Rect>();


            var idx1 = 0;
            foreach (var item in cons)
            {
                idx1++;

                var crect = Cv2.BoundingRect(item);
                if (crect.Width > wdlow && crect.Width < wdhigh && crect.Height > hdlow && crect.Height < hdhigh)
                {
                    rectlist.Add(crect);

                    cwlist.Add(crect.X);
                    y0list.Add(crect.Y);
                    wavglist.Add(crect.Width);
                    y1list.Add(crect.Height);
                }
            }//end foreach

            if (rectlist.Count == 8)
            {
                rectlist.Sort(delegate (Rect r1, Rect r2) {
                    return r1.X.CompareTo(r2.X);
                });
                return rectlist;
            }
            else
            {
                if (cwlist.Count == 0)
                { return new List<Rect>(); }
                cwlist.Sort();
                return GetGuessXlistCircle2168(edged, cwlist, y0list, y1list, wavglist);
            }
        }

        private static List<Rect> GetGuessXlistCircle2168(Mat edged, List<int> cwlist, List<int> y0list, List<int> y1list, List<int> wavglist)
        {
            var rectlist = new List<Rect>();
            var xlist = GetCoordWidthPT(edged);
            var leftedge = xlist.Min();
            var rightedge = xlist.Max();

            var wavg = wavglist.Average() + 2;


            var assumexlist = new List<int>();
            for (var idx = 0; idx < 8; idx++)
            { assumexlist.Add(-1); }

            foreach (var val in cwlist)
            {
                if (val > (leftedge - 0.5 * wavg) && val < (leftedge + 0.5 * wavg))
                { assumexlist[0] = val; }
                if (val > (leftedge + 0.5 * wavg) && val < (leftedge + 1.5 * wavg))
                { assumexlist[1] = val; }
                if (val > (leftedge + 1.5 * wavg) && val < (leftedge + 2.5 * wavg))
                { assumexlist[2] = val; }
                if (val > (leftedge + 2.5 * wavg) && val < (leftedge + 3.5 * wavg))
                { assumexlist[3] = val; }

                if (val > (rightedge - 1.5 * wavg) && val < (rightedge - 0.5 * wavg))
                { assumexlist[7] = val; }
                if (val > (rightedge - 2.5 * wavg) && val < (rightedge - 1.5 * wavg))
                { assumexlist[6] = val; }
                if (val > (rightedge - 3.5 * wavg) && val < (rightedge - 2.5 * wavg))
                { assumexlist[5] = val; }
                if (val > (rightedge - 4.5 * wavg) && val < (rightedge - 3.5 * wavg))
                { assumexlist[4] = val; }
            }

            if (assumexlist[0] == -1)
            {
                if (assumexlist[1] != -1) { assumexlist[0] = assumexlist[1] - (int)wavg - 1; }
                else { assumexlist[0] = (int)leftedge - 2; }
            }
            if (assumexlist[1] == -1)
            {
                if (assumexlist[2] != -1) { assumexlist[1] = assumexlist[2] - (int)wavg - 1; }
                else { assumexlist[1] = assumexlist[0] + (int)wavg; }
            }
            if (assumexlist[2] == -1)
            {
                if (assumexlist[3] != -1)
                { assumexlist[2] = assumexlist[3] - (int)wavg - 1; }
                else
                { assumexlist[2] = assumexlist[1] + (int)wavg; }
            }
            if (assumexlist[3] == -1)
            { assumexlist[3] = assumexlist[2] + (int)wavg; }

            if (assumexlist[7] == -1)
            {
                if (assumexlist[6] != -1)
                { assumexlist[7] = assumexlist[6] + (int)wavg; }
                else
                { assumexlist[7] = (int)rightedge - (int)wavg - 1; }
            }
            if (assumexlist[6] == -1)
            {
                if (assumexlist[5] != -1)
                { assumexlist[6] = assumexlist[5] + (int)wavg; }
                else
                { assumexlist[6] = assumexlist[7] - (int)wavg - 1; }
            }
            if (assumexlist[5] == -1)
            {
                if (assumexlist[4] != -1)
                { assumexlist[5] = assumexlist[4] + (int)wavg; }
                else
                { assumexlist[5] = assumexlist[6] - (int)wavg - 1; }
            }
            if (assumexlist[4] == -1)
            { assumexlist[4] = assumexlist[5] - (int)wavg - 2; }

            var h0avg = (int)y0list.Average() - 1;
            var h1avg = (int)y1list.Average() + 1;

            rectlist.Clear();
            for (var idx = 0; idx < 8; idx++)
            {
                if (idx == 3)
                {
                    rectlist.Add(new Rect(assumexlist[idx] - 1, h0avg, (int)wavg + 2, h1avg));
                }
                else if (idx == 7)
                { rectlist.Add(new Rect(assumexlist[idx] - 1, h0avg, (int)wavg + 2, h1avg)); }
                else
                { rectlist.Add(new Rect(assumexlist[idx] - 1, h0avg, assumexlist[idx + 1] - assumexlist[idx], h1avg)); }
            }

            return rectlist;
        }


        private static double CheckCoordWidth(Mat mat)
        {
            var xlist = new List<double>();

            var ret = new List<List<double>>();
            var kaze = KAZE.Create();
            var kazeDescriptors = new Mat();
            KeyPoint[] kazeKeyPoints = null;
            kaze.DetectAndCompute(mat, null, out kazeKeyPoints, kazeDescriptors);

            var hl = 0.25 * mat.Height;
            var hh = 0.75 * mat.Height;
            var wl = 5;
            var wh = mat.Width - 5;
            var hptlist = new List<KeyPoint>();
            foreach (var pt in kazeKeyPoints)
            {
                if (pt.Pt.Y >= hl && pt.Pt.Y <= hh
                    && pt.Pt.X >= wl && pt.Pt.X <= wh)
                {
                    hptlist.Add(pt);
                    xlist.Add(pt.Pt.X);
                }
            }

            //var dstKaze = new Mat();
            //Cv2.DrawKeypoints(mat, hptlist.ToArray(), dstKaze);

            //using (new Window("dstKaze", dstKaze))
            //{
            //    Cv2.WaitKey();
            //}

            return xlist.Max() - xlist.Min();
        }

        private static List<List<double>> GetDetectPoint(Mat mat)
        {
            var ret = new List<List<double>>();
            var kaze = KAZE.Create();
            var kazeDescriptors = new Mat();
            KeyPoint[] kazeKeyPoints = null;
            kaze.DetectAndCompute(mat, null, out kazeKeyPoints, kazeDescriptors);
            var xlist = new List<double>();
            var ylist = new List<double>();
            foreach (var pt in kazeKeyPoints)
            {
                xlist.Add(pt.Pt.X);
                ylist.Add(pt.Pt.Y);
            }
            ret.Add(xlist);
            ret.Add(ylist);

            //var dstKaze = new Mat();
            //Cv2.DrawKeypoints(mat, kazeKeyPoints, dstKaze);

            //using (new Window("dstKaze", dstKaze))
            //{
            //    Cv2.WaitKey();
            //}

            return ret;
        }

    }
}