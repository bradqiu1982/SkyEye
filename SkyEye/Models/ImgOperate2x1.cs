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


        public static List<Mat> CutCharRect(string imgpath, Rect xyrect)
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

            //if (xyenhgrayresize.Width / xyenhgrayresize.Height > 3)
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

                var xylist2 = GetDetectPoint(edged);
                var width = xylist2[0].Max() - xylist2[0].Min();

                cmatlist.Add(xyenhance);
                if ((edged.Width - width) > 20)
                {
                    var w = width / 4;
                    var zeropoint = xylist2[0].Min();
                    var w1 = (int)(zeropoint + w);
                    var w2 = (int)(zeropoint + 2 * w);
                    var w3 = (int)(zeropoint + 3 * w);

                    var ewidth = edged.Width;
                    var eheight = (int)(edged.Height * 0.55);

                    cmatlist.Add(edged.SubMat(0, eheight, 0, w1));
                    cmatlist.Add(edged.SubMat(0, eheight, w1, w2));
                    cmatlist.Add(edged.SubMat(0, eheight, w2, w3));
                    cmatlist.Add(edged.SubMat(0, eheight, w3, edged.Width));

                    cmatlist.Add(edged.SubMat(eheight, edged.Height, 0, w1));
                    cmatlist.Add(edged.SubMat(eheight, edged.Height, w1, w2));
                    cmatlist.Add(edged.SubMat(eheight, edged.Height, w2, w3));
                    cmatlist.Add(edged.SubMat(eheight, edged.Height, w3, edged.Width));
                }
                else
                {
                    var ewidth = edged.Width / 4;
                    var eheight = (int)(edged.Height * 0.55);

                    cmatlist.Add(edged.SubMat(0, eheight, 0, ewidth));
                    cmatlist.Add(edged.SubMat(0, eheight, ewidth, 2 * ewidth));
                    cmatlist.Add(edged.SubMat(0, eheight, 2 * ewidth, 3 * ewidth));
                    cmatlist.Add(edged.SubMat(0, eheight, 3 * ewidth, edged.Width));

                    cmatlist.Add(edged.SubMat(eheight, edged.Height, 0, ewidth));
                    cmatlist.Add(edged.SubMat(eheight, edged.Height, ewidth, 2 * ewidth));
                    cmatlist.Add(edged.SubMat(eheight, edged.Height, 2 * ewidth, 3 * ewidth));
                    cmatlist.Add(edged.SubMat(eheight, edged.Height, 3 * ewidth, edged.Width));
                }

            }

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