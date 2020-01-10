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
            Mat src = Cv2.ImRead(file, ImreadModes.Grayscale);

            var blurred = new Mat();
            Cv2.GaussianBlur(src, blurred, new Size(5, 5), 0);
            var edged = new Mat();
            Cv2.Canny(blurred, edged, 50, 200, 3,true);

            //using (new Window("edged", edged))
            //{
            //    Cv2.WaitKey();
            //}

            var outmat = new Mat();
            var ids = OutputArray.Create(outmat);
            var cons = new Mat[] { };
            Cv2.FindContours(edged, out cons, ids, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            var conslist = cons.ToList();
            //conslist.Sort(delegate (Mat obj1, Mat obj2)
            //{
            //    return Cv2.ContourArea(obj2).CompareTo(Cv2.ContourArea(obj1));
            //});

            var idx = 0;
            foreach (var item in conslist)
            {
                idx++;

                var rect = Cv2.BoundingRect(item);
                var whrate = (float)rect.Width / (float)rect.Height;
                var a = rect.Width * rect.Height;

                if (whrate > ratelow && whrate < ratehigh
                    && rect.Height > heighlow && rect.Height < heighhigh)
                {
                    //var xymat = src.SubMat(rect);
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

            src.Dispose();
            return ret;
        }

        private static bool XUniformity(List<int> xlist, int widthlow, int widthhigh)
        {
            if (xlist.Count == 4)
            {
                for (var idx = 1; idx < xlist.Count; idx++)
                {
                    var delta = xlist[idx] - xlist[idx - 1];
                    if (delta >= widthlow && delta < (widthhigh + 10))
                    { }
                    else
                    { return false; }
                }
                return true;
            }
            else
            { return false; }
        }

        private static List<int> GetPossibleXList(Mat edged, int widthlow, int widthhigh)
        {
            var outmat = new Mat();
            var ids = OutputArray.Create(outmat);
            var cons = new Mat[] { };
            Cv2.FindContours(edged, out cons, ids, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            var cwlist = new List<int>();
            var y0list = new List<int>();
            var y1list = new List<int>();

            var idx1 = 0;
            foreach (var item in cons)
            {
                idx1++;
                var crect = Cv2.BoundingRect(item);
                if (crect.Width >= widthlow && crect.Width <= widthhigh)
                {
                    if (crect.Y > (edged.Height / 2 - 30))
                    {
                        y1list.Add(crect.Y);
                        cwlist.Add(crect.X);
                    }
                    else
                    { y0list.Add(crect.Y); }
                }//end if
            }

            cwlist.Sort();

            if (XUniformity(cwlist, widthlow, widthhigh))
            {}
            else
            {
                cwlist.Clear();
                y0list.Clear();
                y1list.Clear();

                for (var idx = 0; idx < 4; idx++)
                {
                    cwlist.Add(-1);
                }

                idx1 = 0;
                foreach (var item in cons)
                {
                    idx1++;
                    var crect = Cv2.BoundingRect(item);
                    if (crect.Width >= widthlow && crect.Width <= widthhigh)
                    {
                        if (crect.Y > (edged.Height / 2 - 30))
                        {
                            y1list.Add(crect.Y);
                            if (crect.X < 45)
                            {
                                cwlist[0] = crect.X;
                                cwlist[1] = crect.X + crect.Width;
                            }
                            else if (crect.X >= 50 && crect.X < 100)
                            {
                                cwlist[1] = crect.X;
                                cwlist[2] = crect.X + crect.Width;
                            }
                            else if (crect.X > 105 && crect.X < 160)
                            {
                                cwlist[2] = crect.X;
                                cwlist[3] = crect.X + crect.Width;
                            }
                            else if (crect.X > 165 && crect.X < 210)
                            { cwlist[3] = crect.X; }
                        }
                        else
                        { y0list.Add(crect.Y); }

                        //var mat = edged.SubMat(crect);
                        //    using (new Window("edged" + idx1, mat))
                        //    {
                        //        Cv2.WaitKey();
                        //    }
                    }//end if
                }//end foreach
            }

            if (cwlist[3] == -1)
            { cwlist[3] = edged.Width - 60; }
            if (cwlist[2] == -1)
            { cwlist[2] = cwlist[3] - 60; }
            if (cwlist[1] == -1)
            { cwlist[1] = cwlist[2] - 60; }
            if (cwlist[0] == -1)
            { cwlist[0] = 5; }

            if (y0list.Count > 0)
            { cwlist.Add((int)y0list.Average()); }
            else
            { cwlist.Add(0); }

            if (y1list.Count > 0)
            { cwlist.Add((int)y1list.Average()); }
            else
            { cwlist.Add((int)(edged.Height * 0.55)); }

            return cwlist;
        }

        public static List<Mat> CutCharRect(string imgpath, Rect xyrect, int widthlow, int widthhigh)
        {
            var cmatlist = new List<Mat>();

            Mat src = Cv2.ImRead(imgpath, ImreadModes.Color);
            var xymat = src.SubMat(xyrect);
            var srcmidy = src.Height / 2;
            if (xyrect.Y < srcmidy)
            {
                var center = new Point2f(xymat.Width / 2, xymat.Height / 2);
                var m = Cv2.GetRotationMatrix2D(center, 180, 1);
                var outxymat = new Mat();
                Cv2.WarpAffine(xymat, outxymat, m, new Size(xymat.Width, xymat.Height));
                xymat = outxymat;
            }

            //using (new Window("xymat", xymat))
            //{
            //    Cv2.WaitKey();
            //}

            var newxymat = xymat.SubMat(0, xymat.Rows - 20, (int)(xymat.Cols * 0.6), xymat.Cols - 12);

            //using (new Window("newxymat", newxymat))
            //{
            //    Cv2.WaitKey();
            //}

            var xyenhance = new Mat();
            Cv2.DetailEnhance(newxymat, xyenhance);

            var xyenhgray = new Mat();
            Cv2.CvtColor(xyenhance, xyenhgray, ColorConversionCodes.BGR2GRAY);

            Cv2.Resize(xyenhgray, xyenhgray, new Size(xyenhgray.Width * 2, xyenhgray.Height * 2));

            var xyptlist = GetDetectPoint(xyenhgray);

            var xyenhgrayresize = xyenhgray.SubMat(Convert.ToInt32(xyptlist[1].Min()) + 6, Convert.ToInt32(xyptlist[1].Max())
                , Convert.ToInt32(xyptlist[0].Min()) + 4, Convert.ToInt32(xyptlist[0].Max()));

            Cv2.Resize(xyenhgrayresize, xyenhgrayresize, new Size(xyenhgrayresize.Width * 2, xyenhgrayresize.Height * 2));


                var blurred = new Mat();
                Cv2.GaussianBlur(xyenhgrayresize, blurred, new Size(5, 5), 0);

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

            var xlist = GetPossibleXList(edged, widthlow, widthhigh);

            cmatlist.Add(xyenhance);
            var y0 = xlist[4];
            var eheight = xlist[5]; //(int)(edged.Height * 0.55);
            cmatlist.Add(edged.SubMat(y0, eheight, xlist[0], xlist[1]));
            cmatlist.Add(edged.SubMat(y0, eheight, xlist[1], xlist[2]));
            cmatlist.Add(edged.SubMat(y0, eheight, xlist[2], xlist[3]));
            cmatlist.Add(edged.SubMat(y0, eheight, xlist[3], edged.Width));
            cmatlist.Add(edged.SubMat(eheight, edged.Height, xlist[0], xlist[1]));
            cmatlist.Add(edged.SubMat(eheight, edged.Height, xlist[1], xlist[2]));
            cmatlist.Add(edged.SubMat(eheight, edged.Height, xlist[2], xlist[3]));
            cmatlist.Add(edged.SubMat(eheight, edged.Height, xlist[3], edged.Width));

            return cmatlist;
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