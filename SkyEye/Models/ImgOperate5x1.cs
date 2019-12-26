using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenCvSharp;

namespace SkyEye.Models
{
    public class ImgOperate5x1
    {
        public static List<Rect> FindXYRect(string file, int heighlow, int heighhigh, double ratelow, double ratehigh,int areahigh)
        {
            var ret = new List<Rect>();
            Mat src = Cv2.ImRead(file, ImreadModes.Grayscale);

            var blurred = new Mat();
            Cv2.GaussianBlur(src, blurred, new Size(5, 5), 0);
            var edged = new Mat();
            Cv2.Canny(blurred, edged, 50, 200, 3);

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
            }

            src.Dispose();
            return ret;
        }

        private static List<int> GetPossibleXList(Mat edged, int heighlow, int heighhigh, int widthlow, int widthhigh)
        {
            var outmat = new Mat();
            var ids = OutputArray.Create(outmat);
            var cons = new Mat[] { };
            Cv2.FindContours(edged, out cons, ids, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            var cwlist = new List<int>();
            for (var idx = 0; idx < 6; idx++)
            {
                cwlist.Add(-1);
            }

            foreach (var item in cons)
            {
                var crect = Cv2.BoundingRect(item);
                if (crect.Height >= heighlow && crect.Height <= heighhigh
                    && crect.Width >= widthlow && crect.Width <= widthhigh)
                {
                    var crect_Width = crect.Width;
                    if (crect_Width <= 23)
                    { crect_Width = 25; }
                    if (crect_Width > 32)
                    { crect_Width = 25; }

                    if (crect.X > edged.Width / 2)
                    {
                        if (crect.X < (edged.Width - 17) && crect.X > (edged.Width - 37))
                        {
                            if (cwlist[5] == -1)
                            {
                                cwlist[5] = crect.X;
                                cwlist[4] = crect.X - crect_Width;
                                cwlist[3] = crect.X - 2 * crect_Width;
                            }
                            else
                            { cwlist[5] = crect.X; }
                        }
                        else if (crect.X < (edged.Width - 45) && crect.X > (edged.Width - 59))
                        {
                            if (cwlist[4] == -1)
                            {
                                cwlist[5] = crect.X + crect_Width;
                                cwlist[4] = crect.X;
                                cwlist[3] = crect.X - crect_Width;
                            }
                            else
                            { cwlist[4] = crect.X; cwlist[5] = crect.X + crect_Width; }
                        }
                    }
                    else
                    {
                        if (crect.X > 17 && crect.X < 37)
                        {
                            if (cwlist[0] == -1)
                            {
                                cwlist[0] = crect.X;
                                cwlist[1] = crect.X + crect_Width;
                                cwlist[2] = crect.X + 2 * crect_Width;
                            }
                            else
                            {
                                cwlist[0] = crect.X;
                                cwlist[1] = crect.X + crect_Width;
                            }
                        }
                        else if (crect.X > 45 && crect.X < 59)
                        {
                            if (cwlist[1] == -1)
                            {
                                cwlist[0] = crect.X - crect_Width;
                                cwlist[1] = crect.X;
                                cwlist[2] = crect.X + crect_Width;
                            }
                            else
                            {
                                cwlist[1] = crect.X;
                                cwlist[2] = crect.X + crect_Width;
                            }
                        }
                    }//end else
                }//end if
            }//end foreach

            foreach (var i in cwlist)
            {
                if (i == -1)
                {
                    cwlist.Clear();
                    return cwlist;
                }
            }

            return cwlist;
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

        public static List<Mat> CutCharRect(string imgpath, Rect xyrect, int heighlow, int heighhigh, int widthlow, int widthhigh)
        {
            var cmatlist = new List<Mat>();

            Mat src = Cv2.ImRead(imgpath, ImreadModes.Color);
            var xymat = src.SubMat(xyrect);
            var srcmidy = src.Height / 2;
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

            Cv2.Resize(xyenhgray, xyenhgray, new Size(xyenhgray.Width * 2, xyenhgray.Height * 2));

            var xyptlist = GetDetectPoint(xyenhgray);
            //var kaze = KAZE.Create();
            //var kazeDescriptors = new Mat();
            //KeyPoint[] kazeKeyPoints = null;
            //kaze.DetectAndCompute(xyenhgray, null, out kazeKeyPoints, kazeDescriptors);
            //var xlist = new List<double>();
            //var ylist = new List<double>();
            //foreach (var pt in kazeKeyPoints)
            //{
            //    xlist.Add(pt.Pt.X);
            //    ylist.Add(pt.Pt.Y);
            //}

            //using (new Window("xyenhgray", xyenhgray))
            //{
            //    Cv2.WaitKey();
            //}

            var xyenhgrayresize = xyenhgray.SubMat(Convert.ToInt32(xyptlist[1].Min()) + 7, Convert.ToInt32(xyptlist[1].Max() - 5)
                , Convert.ToInt32(xyptlist[0].Min()) + 8, Convert.ToInt32(xyptlist[0].Max()) - 6);

            //if (xyenhgrayresize.Width / xyenhgrayresize.Height > 4)
            {
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

                var possxlist = GetPossibleXList(edged, heighlow, heighhigh, widthlow, widthhigh);

                var ewidth = edged.Width;
                var eheight = edged.Height;

                cmatlist.Add(xyenhance);

                if (possxlist.Count > 0)
                {
                    cmatlist.Add(edged.SubMat(2, eheight - 2, 0, possxlist[0]));
                    cmatlist.Add(edged.SubMat(2, eheight - 2, possxlist[0], possxlist[1]));
                    cmatlist.Add(edged.SubMat(2, eheight - 2, possxlist[1], possxlist[2]));
                    cmatlist.Add(edged.SubMat(2, eheight - 2, possxlist[2] + 2, possxlist[2] + 31));

                    cmatlist.Add(edged.SubMat(2, eheight - 2, possxlist[3] - 31, possxlist[3] - 3));
                    cmatlist.Add(edged.SubMat(2, eheight - 2, possxlist[3] - 3, possxlist[4] - 1));
                    cmatlist.Add(edged.SubMat(2, eheight - 2, possxlist[4] - 1, possxlist[5]));
                    cmatlist.Add(edged.SubMat(2, eheight - 2, possxlist[5], ewidth));
                }
                else
                {
                    cmatlist.Add(edged.SubMat(1, eheight - 2, 2, 28));
                    cmatlist.Add(edged.SubMat(1, eheight - 2, 29, 55));
                    cmatlist.Add(edged.SubMat(1, eheight - 2, 56, 81));
                    cmatlist.Add(edged.SubMat(1, eheight - 2, 82, 106));

                    cmatlist.Add(edged.SubMat(1, eheight - 2, ewidth - 104, ewidth - 78));
                    cmatlist.Add(edged.SubMat(1, eheight - 2, ewidth - 78, ewidth - 52));
                    cmatlist.Add(edged.SubMat(1, eheight - 2, ewidth - 52, ewidth - 26));
                    cmatlist.Add(edged.SubMat(1, eheight - 2, ewidth - 26, ewidth));
                }

            }

            return cmatlist;
        }
    }
}