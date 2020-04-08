using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenCvSharp;

namespace SkyEye.Models
{
    public class ImgOperateSmall5x1
    {

        //18,34,4.5,6.92,5000,50
        public static List<Rect> FindSmall5x1Rect(string imgpath, double whlow, double whhigh, double ratelow, double ratehigh, double area, double locationoffset)
        {
            Mat srcorgimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var detectsize = GetDetectPoint(srcorgimg);
            var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var srcenhance = new Mat();
            Cv2.DetailEnhance(srcrealimg, srcenhance);

            var denoisemat = new Mat();
            Cv2.FastNlMeansDenoisingColored(srcenhance, denoisemat, 10, 10, 7, 21);

            var srcgray = new Mat();
            Cv2.CvtColor(denoisemat, srcgray, ColorConversionCodes.BGR2GRAY);

            var blurred = new Mat();
            Cv2.GaussianBlur(srcgray, blurred, new Size(3, 3), 0);

            var frects = FindSmall5x1Rect_(blurred,srcgray,srcenhance, whlow, whhigh, ratelow, ratehigh, area,locationoffset,false);
            var trects = FindSmall5x1Rect_(blurred, srcgray, srcenhance, whlow, whhigh, ratelow, ratehigh, area, locationoffset, true);
            var rects = new List<Rect>();
            if (frects.Count > 0 && trects.Count > 0)
            {
                if (frects[0].Width * frects[0].Height > trects[0].Width * trects[0].Height)
                { rects.AddRange(frects); }
                else
                { rects.AddRange(trects); }
            }
            else if (frects.Count > 0)
            { rects.AddRange(frects); }
            else if (trects.Count > 0)
            { rects.AddRange(trects); }
            return rects;
        }

        //18,34,4.5,8,5000,50
        public static List<Mat> CutCharRect(string imgpath, double whlow, double whhigh, double ratelow, double ratehigh, double area, double locationoffset)
        {
            Mat srcorgimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            var detectsize = GetDetectPoint(srcorgimg);
            var srcrealimg = srcorgimg.SubMat((int)detectsize[1].Min(), (int)detectsize[1].Max(), (int)detectsize[0].Min(), (int)detectsize[0].Max());

            var srcenhance = new Mat();
            Cv2.DetailEnhance(srcrealimg, srcenhance);

            var denoisemat = new Mat();
            Cv2.FastNlMeansDenoisingColored(srcenhance, denoisemat, 10, 10, 7, 21);

            var srcgray = new Mat();
            Cv2.CvtColor(denoisemat, srcgray, ColorConversionCodes.BGR2GRAY);

            var blurred = new Mat();
            Cv2.GaussianBlur(srcgray, blurred, new Size(3, 3), 0);

            var frects = FindSmall5x1Rect_(blurred, srcgray, srcenhance, whlow, whhigh, ratelow, ratehigh, area, locationoffset, false);
            var trects = FindSmall5x1Rect_(blurred, srcgray, srcenhance, whlow, whhigh, ratelow, ratehigh, area, locationoffset, true);
            var rects = new List<Rect>();
            if (frects.Count > 0 && trects.Count > 0)
            {
                if (frects[0].Width * frects[0].Height > trects[0].Width * trects[0].Height)
                { rects.AddRange(frects); }
                else
                { rects.AddRange(trects); }
            }
            else if (frects.Count > 0)
            { rects.AddRange(frects); }
            else if (trects.Count > 0)
            { rects.AddRange(trects); }

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

            Cv2.DetailEnhance(coormat, coormat);

            var coormatresize = new Mat();
            Cv2.Resize(coormat, coormatresize, new Size(coormat.Cols * 4, coormat.Rows * 4), 0, 0, InterpolationFlags.Linear);

            var coorenhance = new Mat();
            Cv2.DetailEnhance(coormatresize, coorenhance);

            var coorgray = new Mat();
            var denoisemat2 = new Mat();
            Cv2.FastNlMeansDenoisingColored(coorenhance, denoisemat2, 10, 10, 7, 21);
            Cv2.CvtColor(denoisemat2, coorgray, ColorConversionCodes.BGR2GRAY);

            Cv2.GaussianBlur(coorgray, blurred, new Size(5, 5), 0);

            var edged = new Mat();
            Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

            var rectlist = GetSmall5x1CharRect(coorgray, blurred, edged, coorenhance);
            var ret = new List<Mat>();
            if (rectlist.Count == 0)
            { return ret; }

            Cv2.DetailEnhance(coormat, coorenhance);
            ret.Add(coorenhance);
            foreach (var rect in rectlist)
            {
                if (rect.X < 0 || rect.Y < 0
                    || ((rect.X + rect.Width) > edged.Width)
                    || ((rect.Y + rect.Height) > edged.Height))
                {
                    ret.Clear();
                    return ret;
                }
                var cmat = edged.SubMat(rect);
                ret.Add(cmat);
            }
            return ret;
        }


        private static List<Rect> GetSmall5x1CharRect(Mat coorgray, Mat blurred, Mat edged,Mat coorenhance)
        {

            var cbond = GetCoordHighPT(blurred, edged);
            var xlist = GetCoordWidthPT(coorenhance,edged);
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
                var y0list = new List<int>();
                var y1list = new List<int>();

                cbond.Sort(delegate (Rect o1, Rect o2)
                { return o1.X.CompareTo(o2.X); });

                var filteredbond = new List<Rect>();
                foreach (var item in cbond)
                {
                    if (filteredbond.Count == 0)
                    {
                        filteredbond.Add(item);
                        y0list.Add(item.Y);
                        y1list.Add(item.Height);
                    }
                    else
                    {
                        var bcnt = filteredbond.Count;
                        if (item.X - filteredbond[bcnt - 1].X > 28)
                        {
                            filteredbond.Add(item);
                            y0list.Add(item.Y);
                            y1list.Add(item.Height);
                        }
                    }
                }

                var y0 = (int)y0list.Average();
                var y1 = y1list.Max();

                if ((int)xcmax - 179 > 0) { ret.Add(new Rect((int)xcmax - 179, y0, 44, y1)); }
                else { ret.Add(new Rect(0, y0, 44, y1)); }

                ret.Add(new Rect((int)xcmax - 130, y0, 43, y1));
                ret.Add(new Rect((int)xcmax - 90, y0, 44, y1));
                ret.Add(new Rect((int)xcmax - 46, y0, 45, y1));

                ret.Add(new Rect((int)ycmin, y0, 45, y1));
                ret.Add(new Rect((int)ycmin + 52, y0, 45, y1));
                ret.Add(new Rect((int)ycmin + 90, y0, 45, y1));

                if (((int)ycmin + 135 + 46) >= (edged.Cols-1))
                { ret.Add(new Rect((int)ycmin + 135, y0, edged.Cols - (int)ycmin - 135, y1)); }
                else
                { ret.Add(new Rect((int)ycmin + 135, y0, 46, y1)); }


                var changedict = new Dictionary<int, bool>();

                for (var idx = 0; idx < 7; idx++)
                {
                    foreach (var item in filteredbond)
                    {
                        if ((item.X > ret[idx].X - 15) && (item.X < ret[idx].X + 15))
                        {
                            var currentrect = new Rect(item.X, ret[idx].Y, item.Width, ret[idx].Height);
                            if (!changedict.ContainsKey(idx))
                            {
                                ret[idx] = currentrect;
                                changedict.Add(idx, true);
                            }
                            break;
                        }
                    }
                }//end for

                for (var idx = 0; idx < 7; idx++)
                {
                    foreach (var item in filteredbond)
                    {
                        if ((item.X > ret[idx].X - 15) && (item.X < ret[idx].X + 15))
                        {
                            if ((idx >= 0 && idx <= 2) || (idx >= 4 && idx <= 6))
                            {
                                var nextrect = new Rect(item.X + item.Width + 1, ret[idx].Y
                                    ,((item.X + 2*item.Width + 1) < edged.Width)?item.Width:(edged.Width - item.X - item.Width - 1), ret[idx].Height);

                                if (!changedict.ContainsKey(idx+1))
                                {
                                    ret[idx + 1] = nextrect;
                                    changedict.Add(idx+1, true);
                                }
                            }

                            if ((idx >= 1 && idx <= 3) || (idx >= 5 && idx <= 7))
                            {
                                var nextrect = new Rect((item.X - item.Width - 1) > 0 ? (item.X - item.Width) : 0, ret[idx].Y, item.Width + 1, ret[idx].Height);
                                if (!changedict.ContainsKey(idx - 1))
                                {
                                    ret[idx - 1] = nextrect;
                                    changedict.Add(idx - 1, true);
                                }
                            }
                            break;
                        }
                    }
                }//end for
            }
            else
            {
                var y0 = 18;
                var y1 = 69;
                if (edged.Height < 88)
                { y1 = edged.Height - y0; }

                if ((int)xcmax - 179 > 0) { ret.Add(new Rect((int)xcmax - 179, y0, 44, y1)); }
                else { ret.Add(new Rect(0, y0, 44, y1)); }

                ret.Add(new Rect((int)xcmax - 130, y0, 43, y1));
                ret.Add(new Rect((int)xcmax - 90, y0, 44, y1));
                ret.Add(new Rect((int)xcmax - 46, y0, 45, y1));

                ret.Add(new Rect((int)ycmin, y0, 47, y1));
                ret.Add(new Rect((int)ycmin + 52, y0, 45, y1));
                ret.Add(new Rect((int)ycmin + 90, y0, 45, y1));

                if (((int)ycmin + 135 + 46) >= (edged.Cols - 1))
                {
                    ret.Add(new Rect((int)ycmin + 135, y0, edged.Cols - (int)ycmin - 135, y1));
                }
                else
                { ret.Add(new Rect((int)ycmin + 135, y0, 46, y1)); }

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

        private static List<double> GetCoordWidthPT(Mat mat,Mat edged)
        {
            var denoisemat = new Mat();
            Cv2.FastNlMeansDenoisingColored(mat, denoisemat, 10, 10, 7, 21);

            var ret = new List<List<double>>();
            var kaze = KAZE.Create();
            var kazeDescriptors = new Mat();
            KeyPoint[] kazeKeyPoints = null;
            kaze.DetectAndCompute(denoisemat, null, out kazeKeyPoints, kazeDescriptors);

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
            if (wptlist.Count == 0)
            { return xlist; }

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
            //Cv2.DrawKeypoints(denoisemat, xyptlist.ToArray(), dstKaze);

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
            var xlist = GetCoordWidthPT(outxymat, outxymat);
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

        //18,34,4.5,6.92,5000,50
        private static List<Rect> FindSmall5x1Rect_(Mat blurred,Mat srcgray,Mat srcenhance,double whlow,double whhigh,double ratelow,double ratehigh,double area,double locationoffset,bool cflag)
        {
            var ret = new List<Rect>();

            //var cflaglist = new List<bool>();
            //cflaglist.Add(false);
            //cflaglist.Add(true);

            //foreach (var cflag in cflaglist)
            //{
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
                        && whrate >= ratelow && whrate < ratehigh && a < area && rect.Y > locationoffset)
                    {
                        //var xymat = srcgray.SubMat(rect);
                        //using (new Window("xymat" + idx, xymat))
                        //{
                        //    Cv2.WaitKey();
                        //}

                        if ((rect.Width >= 18 && rect.Width <= 20) && ((rect.X + rect.Width + 2) <= edged.Width))
                        { rect = new Rect(rect.X - 2, rect.Y, rect.Width + 4, rect.Height); }

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
                    if (CheckVerticalCutMat(srcgray, srcenhance, ret[0]))
                    {
                        return ret;
                    }
                    else
                    { ret.Clear(); }
                }

                //foreach (var item in conslist)
                //{
                //    idx++;

                //    var rect = Cv2.BoundingRect(item);
                //    var whrate = (double)rect.Width / (double)rect.Height;
                //    var a = rect.Width * rect.Height;
                //    if (rect.Height >= whlow && rect.Height <= whhigh
                //        && whrate >= ratelow && whrate < ratehigh && a < area && rect.X > locationoffset)
                //    {
                //        //var xymat = srcgray.SubMat(rect);
                //        //using (new Window("xymat" + idx, xymat))
                //        //{
                //        //    Cv2.WaitKey();
                //        //}

                //        if ((rect.Height >= 18 && rect.Height <= 20) && ((rect.Y + rect.Height + 2) <= edged.Height))
                //        { rect = new Rect(rect.X, rect.Y - 2, rect.Width, rect.Height + 4); }

                //        if (ret.Count > 0)
                //        {
                //            if (a > ret[0].Width * ret[0].Height)
                //            {
                //                ret.Clear();
                //                ret.Add(rect);
                //            }
                //        }
                //        else
                //        { ret.Add(rect); }
                //    }
                //}

                //if (ret.Count > 0)
                //{
                //    return ret;
                //}
            //}

            return new List<Rect>();
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