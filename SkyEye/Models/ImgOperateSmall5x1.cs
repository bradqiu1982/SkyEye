using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenCvSharp;

namespace SkyEye.Models
{
    public class ImgOperateSmall5x1
    {

        //18,30,4.5,6.92,4500
        public static List<Rect> FindSmall5x1Rect(string imgpath, double whlow, double whhigh, double ratelow, double ratehigh, double area)
        {
            Mat srcorgimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var detectsize = GetDetectPoint(srcorgimg);
            var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var srcenhance = new Mat();
            Cv2.DetailEnhance(srcrealimg, srcenhance);

            var srcgray = new Mat();
            Cv2.CvtColor(srcenhance, srcgray, ColorConversionCodes.BGR2GRAY);

            var blurred = new Mat();
            Cv2.GaussianBlur(srcgray, blurred, new Size(3, 3), 0);

            return FindSmall5x1Rect_(blurred,srcgray,srcenhance, whlow, whhigh, ratelow, ratehigh, area);
        }

        //18,30,4.5,6.92,4500
        public static List<Mat> CutCharRect(string imgpath, double whlow, double whhigh, double ratelow, double ratehigh, double area)
        {
            Mat srcorgimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var detectsize = GetDetectPoint(srcorgimg);
            var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var srcenhance = new Mat();
            Cv2.DetailEnhance(srcrealimg, srcenhance);

            var srcgray = new Mat();
            Cv2.CvtColor(srcenhance, srcgray, ColorConversionCodes.BGR2GRAY);

            var blurred = new Mat();
            Cv2.GaussianBlur(srcgray, blurred, new Size(3, 3), 0);

            var rects = FindSmall5x1Rect_(blurred,srcgray,srcenhance, whlow, whhigh, ratelow, ratehigh, area);
            if (rects.Count == 0)
            { return new List<Mat>(); }

            var coormat = srcenhance.SubMat(rects[0]);
            if (rects[0].Height > rects[0].Width)
            {
                var outxymat = new Mat();
                Cv2.Transpose(coormat, outxymat);
                Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                Cv2.Transpose(outxymat, outxymat);
                Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                Cv2.Transpose(outxymat, outxymat);
                Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                coormat = outxymat;
            }

            var coormatresize = new Mat();
            Cv2.Resize(coormat, coormatresize, new Size(coormat.Cols * 4, coormat.Rows * 4), 0, 0, InterpolationFlags.Linear);

            var coorenhance = new Mat();
            Cv2.DetailEnhance(coormatresize, coorenhance);

            var coorgray = new Mat();
            Cv2.CvtColor(coorenhance, coorgray, ColorConversionCodes.BGR2GRAY);

            Cv2.GaussianBlur(coorgray, blurred, new Size(5, 5), 0);

            var edged = new Mat();
            Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 20);

            var rectlist = GetSmall5x1CharRect(coorgray, blurred, edged);
            var ret = new List<Mat>();
            if (rectlist.Count == 0)
            { return ret; }

            Cv2.DetailEnhance(coormat, coorenhance);
            ret.Add(coorenhance);
            foreach (var rect in rectlist)
            {
                var cmat = edged.SubMat(rect);
                ret.Add(cmat);
            }
            return ret;
        }


        private static List<Rect> GetSmall5x1CharRect(Mat coorgray, Mat blurred, Mat edged)
        {

            var cbond = GetCoordHighPT(blurred, edged);
            var xlist = GetCoordWidthPT(coorgray);
            if (xlist.Count == 0 || (xlist.Max() -xlist.Min()) < 400)
            { return new List<Rect>(); }

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

            if (cbond.Count > 0)
            {
                var y0 = cbond[0].Y;
                var y1 = cbond[0].Height;

                if ((int)xcmax - 175 > 0) { ret.Add(new Rect((int)xcmax - 175, y0, 44, y1)); }
                else { ret.Add(new Rect(0, y0, 44, y1)); }

                ret.Add(new Rect((int)xcmax - 132, y0, 43, y1));
                ret.Add(new Rect((int)xcmax - 90, y0, 44, y1));
                ret.Add(new Rect((int)xcmax - 46, y0, 45, y1));

                ret.Add(new Rect((int)ycmin, y0, 47, y1));
                ret.Add(new Rect((int)ycmin + 45, y0, 45, y1));
                ret.Add(new Rect((int)ycmin + 87, y0, 45, y1));

                if (((int)ycmin + 133 + 46) >= (edged.Cols-1))
                { ret.Add(new Rect((int)ycmin + 133, y0, edged.Cols - (int)ycmin - 135, y1)); }
                else
                { ret.Add(new Rect((int)ycmin + 133, y0, 46, y1)); }

                //sort cbond
                cbond.Sort(delegate (Rect o1, Rect o2)
                { return o1.X.CompareTo(o2.X); });

                var filteredbond = new List<Rect>();
                foreach (var item in cbond)
                {
                    if (filteredbond.Count == 0)
                    {
                        filteredbond.Add(item);
                    }
                    else
                    {
                        var bcnt = filteredbond.Count;
                        if (item.X - filteredbond[bcnt - 1].X > 28)
                        {
                            filteredbond.Add(item);
                        }
                    }
                }//end foreach

                for (var idx = 0; idx < 7; idx++)
                {
                    foreach (var item in filteredbond)
                    {
                        if ((item.X > ret[idx].X - 15) && (item.X < ret[idx].X + 15))
                        {
                            var currentrect = new Rect(item.X, ret[idx].Y, item.Width, ret[idx].Height);
                            ret[idx] = currentrect;

                            if ((idx >= 0 && idx <= 2) || (idx >= 4 && idx <= 6))
                            {
                                var nextrect = new Rect(item.X + item.Width + 1, ret[idx].Y, item.Width, ret[idx].Height);
                                ret[idx + 1] = nextrect;
                            }

                            if ((idx >= 1 && idx <= 3) || (idx >= 5 && idx <= 7))
                            {
                                var nextrect = new Rect((item.X - item.Width - 1) > 0 ? (item.X - item.Width) : 0, ret[idx].Y, item.Width + 1, ret[idx].Height);
                                ret[idx - 1] = nextrect;
                            }
                            break;
                        }
                    }
                }//end for
            }
            else
            {
                if ((int)xcmax - 175 > 0) { ret.Add(new Rect((int)xcmax - 175, 9, 44, 69)); }
                else { ret.Add(new Rect(0, 9, 44, 69)); }

                ret.Add(new Rect((int)xcmax - 130, 9, 43, 69));
                ret.Add(new Rect((int)xcmax - 90, 9, 44, 69));
                ret.Add(new Rect((int)xcmax - 46, 9, 45, 69));

                ret.Add(new Rect((int)ycmin, 9, 47, 69));
                ret.Add(new Rect((int)ycmin + 45, 9, 45, 69));
                ret.Add(new Rect((int)ycmin + 87, 9, 45, 69));

                if (((int)ycmin + 133 + 46) >= (edged.Cols-1))
                { ret.Add(new Rect((int)ycmin + 133, 9, edged.Cols - (int)ycmin - 135, 69)); }
                else
                { ret.Add(new Rect((int)ycmin + 133, 9, 46, 69)); }
            }

            return ret;
        }

        private static List<Rect> GetCoordHighPT(Mat blurred, Mat edged)
        {
            var rectlist = new List<Rect>();

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

                    if (crect.Width > 36 && crect.Width <= 60 && crect.Height > 54 && crect.Height <= 65 && crect.Y > 8)
                    {
                        //Cv2.Rectangle(coorenhance, crect, new Scalar(0, 255, 0));
                        //using (new Window("xyenhance4", coorenhance))
                        //{
                        //    Cv2.WaitKey();
                        //}
                        rectlist.Add(crect);
                    }
                }
            }//end foreach

            return rectlist;
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

                if (wlist.Count > 3 && (ylist.Max() - ylist.Min()) > 0.25 * mat.Height)
                { wptlist.AddRange(wlist); }
                idx = idx + 15;
            }

            var xlist = new List<double>();
            if (wptlist.Count == 0)
            { return xlist; }

            foreach (var pt in wptlist)
            {
                xlist.Add(pt.Pt.X);
            }

            var xlength = xlist.Max() - xlist.Min();
            var coordlength = 0.335 * xlength;
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
            if (xyptlist.Count == 0)
            { return xlist; }

            xlist.Clear();
            foreach (var pt in xyptlist)
            {
                xlist.Add(pt.Pt.X);
            }

            return xlist;
        }

        private static bool CheckVerticalCutMat(Mat srcgray,Mat srcenhance, Rect rect)
        {
            var xymat = srcenhance.SubMat(rect);
            var outxymat = new Mat();
            Cv2.Transpose(xymat, outxymat);
            Cv2.Flip(outxymat, outxymat, FlipMode.Y);
            Cv2.Resize(outxymat, outxymat, new Size(outxymat.Width * 4, outxymat.Height * 4));
            Cv2.DetailEnhance(outxymat, outxymat);
            var xlist = GetCoordWidthPT(outxymat);
            if (xlist.Count > 0)
            {
                var xmax = xlist.Max();
                var xmin = xlist.Min();
                if (xmax - xmin >= 400)
                {
                    return true;
                }
            }
            return false;
        }

        //18,30,4.5,6.92,4500
        private static List<Rect> FindSmall5x1Rect_(Mat blurred,Mat srcgray,Mat srcenhance,double whlow,double whhigh,double ratelow,double ratehigh,double area)
        {
            var cflaglist = new List<bool>();
            cflaglist.Add(false);
            cflaglist.Add(true);
            var ret = new List<Rect>();

            foreach (var cflag in cflaglist)
            {
                var edged = new Mat();
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



                var idx = 0;
                foreach (var item in conslist)
                {
                    idx++;

                    var rect = Cv2.BoundingRect(item);
                    var whrate = (double)rect.Height / (double)rect.Width;
                    var a = rect.Width * rect.Height;
                    if (rect.Width >= whlow && rect.Width <= whhigh
                        && whrate >= ratelow && whrate < ratehigh && a < area)
                    {
                        //var xymat = srcgray.SubMat(rect);
                        //using (new Window("xymat" + idx, xymat))
                        //{
                        //    Cv2.WaitKey();
                        //}

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
                }

                if (ret.Count > 0)
                {
                    if (CheckVerticalCutMat(srcgray,srcenhance, ret[0]))
                    {
                        return ret;
                    }
                }

                foreach (var item in conslist)
                {
                    idx++;

                    var rect = Cv2.BoundingRect(item);
                    var whrate = (double)rect.Width / (double)rect.Height;
                    var a = rect.Width * rect.Height;
                    if (rect.Height >= whlow && rect.Height <= whhigh
                        && whrate >= ratelow && whrate < ratehigh && a < area)
                    {
                        //var xymat = srcgray.SubMat(rect);
                        //using (new Window("xymat" + idx, xymat))
                        //{
                        //    Cv2.WaitKey();
                        //}

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
                }

                if (ret.Count > 0)
                { return ret; }
            }

            return ret;
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