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
            }//end foreach

            return ret;
        }

        public static List<Rect> FindXYRect(string file, int heighlow, int heighhigh, double ratelow, double ratehigh,int areahigh, bool fixangle = false)
        {
            var ret = new List<Rect>();
            Mat src = Cv2.ImRead(file, ImreadModes.Grayscale);
            if (fixangle)
            {
                var angle = ImgPreOperate.GetAngle(file);
                if (angle >= 0.7 && angle <= 359.3)
                { src = ImgPreOperate.GetFixedAngleImg(src,angle); }
            }

            var denoisemat = new Mat();
            Cv2.FastNlMeansDenoising(src, denoisemat, 10, 7, 21);

            var blurred = new Mat();
            Cv2.GaussianBlur(denoisemat, blurred, new Size(5, 5), 0);

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

            //var cannyflags = new List<bool>();
            //cannyflags.Add(false);
            //cannyflags.Add(true);
            //foreach (var cflag in cannyflags)
            //{
            //    var edged = new Mat();
            //    Cv2.Canny(blurred, edged, 50, 200, 3, cflag);

            //    //using (new Window("edged", edged))
            //    //{
            //    //    Cv2.WaitKey();
            //    //}

            //    var outmat = new Mat();
            //    var ids = OutputArray.Create(outmat);
            //    var cons = new Mat[] { };
            //    Cv2.FindContours(edged, out cons, ids, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            //    var conslist = cons.ToList();

            //    foreach (var item in conslist)
            //    {
            //        var rect = Cv2.BoundingRect(item);
            //        var a = rect.Width * rect.Height;
            //        var whrate = (double)rect.Width / (double)rect.Height;
            //        if (rect.Height >= heighlow && rect.Height <= heighhigh
            //            && whrate > ratelow && whrate < ratehigh && a < areahigh)
            //        {
            //            if (ret.Count > 0)
            //            {
            //                if (a > ret[0].Width * ret[0].Height)
            //                {
            //                    ret.Clear();
            //                    ret.Add(rect);
            //                }
            //            }
            //            else
            //            { ret.Add(rect); }
            //        }
            //    }//end foreach

            //    if (ret.Count > 0)
            //    { break; }
            //}//end forea

            src.Dispose();
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

        private static List<int> UniqX(List<int> xlist, int imgw, int avgw, int widthlow, int widthhigh)
        {
            var ret = new List<int>();
            if (xlist.Count < 3) { return ret; }

            ret.Add(xlist[0]);
            for (var idx = 1; idx < xlist.Count; idx++)
            {
                var delta = xlist[idx] - xlist[idx - 1];
                if (delta > widthlow)
                { ret.Add(xlist[idx]); }
            }

            if (ret.Count == 8)
            { return ret; }
            else
            {
                var passiblexlist = new List<int>();
                for (var idx = 0; idx < 8; idx++)
                { passiblexlist.Add(-1); }

                var xhigh = (int)(imgw * 0.666);
                var step = avgw + 2;

                var leftready = false;
                var rightready = false;

                foreach (var x in ret)
                {
                    if (x < (int)(step) && !leftready)
                    {
                        passiblexlist[0] = x;
                        passiblexlist[1] = passiblexlist[0] + avgw + 4;
                        passiblexlist[2] = passiblexlist[1] + avgw + 6;
                        passiblexlist[3] = passiblexlist[2] + avgw + 8;
                        leftready = true;
                    }
                    else if (x > (3 * step + 8) && x < (4 * step+20) && !leftready)
                    {
                        passiblexlist[3] = x;
                        passiblexlist[2] = passiblexlist[3] - avgw - 5;
                        passiblexlist[1] = passiblexlist[2] - avgw - 6; 
                        passiblexlist[0] = passiblexlist[1] - avgw - 8;
                        leftready = true;
                    }
                    else if (x > (step + 8) && (x < 2 * step - 10) && !leftready)
                    {
                        passiblexlist[1] = x;
                        passiblexlist[2] = passiblexlist[1] + avgw + 5;
                        passiblexlist[3] = passiblexlist[2] + avgw + 8;
                        if (passiblexlist[0] == -1)
                        { passiblexlist[0] = passiblexlist[1] - avgw - 5; }
                    }
                    else if (x > (2 * step + 8) && x < 3 * step && !leftready)
                    {
                        passiblexlist[2] = x;
                        passiblexlist[3] = passiblexlist[2] + avgw + 5;
                        if (passiblexlist[1] == -1)
                        { passiblexlist[1] = passiblexlist[2] - avgw - 5; }
                        if (passiblexlist[0] == -1)
                        { passiblexlist[0] = passiblexlist[1] - avgw - 8; }
                    }
                    
                    else if ((int)Math.Abs(x - xhigh) < (int)(step) && !rightready)
                    {
                        passiblexlist[4] = x;
                        passiblexlist[5] = passiblexlist[4] + avgw + 4;
                        passiblexlist[6] = passiblexlist[5] + avgw + 6;
                        passiblexlist[7] = passiblexlist[6] + avgw + 8;
                        rightready = true;
                    }
                    else if (x > imgw - step * 1.5 && !rightready)
                    {
                        passiblexlist[7] = x;
                         passiblexlist[6] = passiblexlist[7] - avgw - 5;
                        passiblexlist[5] = passiblexlist[6] - avgw - 6;
                        passiblexlist[4] = passiblexlist[5] - avgw - 8;
                        rightready = true;
                    }
                    else if (x > imgw - step * 3 && x < imgw - step * 2 && !rightready)
                    {
                        passiblexlist[6] = x;
                        passiblexlist[7] = passiblexlist[6] + avgw + 5;
                        if (passiblexlist[5] == -1)
                        { passiblexlist[5] = passiblexlist[6] - avgw - 5; }
                        if (passiblexlist[4] == -1)
                        { passiblexlist[4] = passiblexlist[5] - avgw - 6; }
                    }
                    else if ((int)Math.Abs(x - xhigh) > (int)(1.1 * step) && (int)Math.Abs(x - xhigh) < (int)(2.1 * step) && !rightready)
                    {
                        passiblexlist[5] = x;
                        passiblexlist[6] = passiblexlist[5] + avgw + 5;
                        passiblexlist[7] = passiblexlist[6] + avgw + 6;
                        if (passiblexlist[4] == -1)
                        { passiblexlist[4] = passiblexlist[5] - avgw - 5; }
                    }
                    
                }//end foreach

                for (var idx = 1; idx < passiblexlist.Count; idx++)
                {
                    var delta = passiblexlist[idx] - passiblexlist[idx - 1];
                    if (delta < widthlow)
                    {
                        passiblexlist.Clear();
                        return passiblexlist;
                    }

                    if (idx != 4 && delta > widthhigh)
                    {
                        passiblexlist.Clear();
                        return passiblexlist;
                    }
                }

                foreach (var x in passiblexlist)
                {
                    if (x < 0)
                    {
                        passiblexlist.Clear();
                        return passiblexlist;
                    }
                }

                if (Math.Abs(passiblexlist[7] - imgw) <= 4)
                {
                    passiblexlist[7] = imgw - avgw - 4;
                    passiblexlist[6] = passiblexlist[7] - avgw - 5;
                    passiblexlist[5] = passiblexlist[6] - avgw - 6;
                    passiblexlist[4] = passiblexlist[5] - avgw - 8;
                }

                return passiblexlist;
            }
        }

        private static List<int> GetPossibleXList(Mat edged, int heighlow, int heighhigh, int widthlow, int widthhigh)
        {
            var outmat = new Mat();
            var ids = OutputArray.Create(outmat);
            var cons = new Mat[] { };
            Cv2.FindContours(edged, out cons, ids, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            var xlow = (int)(edged.Width * 0.333 -20);
            var xhigh = (int)(edged.Width * 0.666 - 10);

            var cwlist = new List<int>();
            var y0list = new List<int>();
            var y1list = new List<int>();
            var wavglist = new List<int>();
            var backxlist = new List<KeyValuePair<int, int>>();

            var idx1 = 0;
            foreach (var item in cons)
            {
                idx1++;

                var crect = Cv2.BoundingRect(item);

                if (crect.Width > (int)(widthlow*0.6) && crect.Width < widthlow
                    && crect.Height > heighlow && crect.Height < heighhigh)
                {
                    //Cv2.Rectangle(xyenhance4, crect, new Scalar(0, 255, 0));
                    //using (new Window("xyenhance4" + idx1, xyenhance4))
                    //{
                    //    Cv2.WaitKey();
                    //}
                    if ((crect.X < xlow || crect.X > xhigh) && crect.Y > 5)
                    {
                        backxlist.Add(new KeyValuePair<int, int>(crect.X, crect.Width));
                    }//end if
                }

                if (crect.Width >= widthlow && crect.Width <= widthhigh 
                    && crect.Height > heighlow && crect.Height < heighhigh)
                {
                    //var mat = edged.SubMat(crect);
                    //using (new Window("edged" + idx1, mat))
                    //{
                    //    Cv2.WaitKey();
                    //}
                    if ((crect.X < xlow || crect.X > xhigh) && crect.X > 5 && crect.Y >= 4)
                    {
                        wavglist.Add(crect.Width);
                        cwlist.Add(crect.X);
                        y0list.Add(crect.Y);
                        y1list.Add(crect.Height);
                    }

                }//end if
            }//end foreach


            cwlist.Sort();
            var wavg = 0;
            if (wavglist.Count > 0)
            { wavg = (int)wavglist.Average(); }

            var retlist = UniqX(cwlist, edged.Width, wavg, widthlow, widthhigh);
            if (retlist.Count == 0)
            {
                var ncwlist = new List<int>();
                ncwlist.AddRange(cwlist);
                foreach (var kv in backxlist)
                {
                    var bx = kv.Key - (wavg - kv.Value) / 2 - 2;
                    if (bx > 0)
                    { ncwlist.Add(bx); }
                }
                ncwlist.Sort();
                retlist = UniqX(ncwlist, edged.Width, wavg, widthlow, widthhigh);
            }

            if (retlist.Count == 8)
            {
                retlist.Add((int)y0list.Average());
                retlist.Add((int)y1list.Max()+2);
            }
            return retlist;
        }

        public static List<Mat> CutCharRect_X(string imgpath, Rect xyrect, int heighlow, int heighhigh, int widthlow, int widthhigh)
        {
            var cmatlist = new List<Mat>();

            Mat src = Cv2.ImRead(imgpath, ImreadModes.Color);
            var xymat = src.SubMat(xyrect);

            var availableimgpt = GetDetectPoint(src);
            //var srcmidy = src.Height / 2;
            var srcmidy = (availableimgpt[1].Max() + availableimgpt[1].Min())/2;

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

                var possxlist = GetPossibleXList(edged, heighlow, heighhigh, widthlow, widthhigh);
                if (possxlist.Count > 0)
                {
                    try
                    {
                        var ewidth1 = possxlist[3] * 2 - possxlist[2] + 3;
                        var ewidth2 = possxlist[7] * 2 - possxlist[6];
                        if (ewidth2 > edged.Width)
                        { ewidth2 = edged.Width; }

                        var eheight0 = possxlist[8];
                        var eheight1 = possxlist[8] + possxlist[9];
                        if (eheight1 > edged.Height)
                        { eheight1 = edged.Height; }

                        cmatlist.Add(xyenhance);

                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[0] > 0 ? possxlist[0] : 0, possxlist[1]));
                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[1], possxlist[2]));
                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[2], possxlist[3]));
                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[3], ewidth1));

                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[4], possxlist[5]));
                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[5], possxlist[6]));
                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[6], possxlist[7]));
                        cmatlist.Add(edged.SubMat(eheight0, eheight1, possxlist[7], ewidth2));
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

        public static List<Mat> CutCharRect(string imgpath, Rect xyrect, int heighlow, int heighhigh, int widthlow, int widthhigh, bool fixangle = false)
        {
            var cmatlist = new List<Mat>();

            Mat src = Cv2.ImRead(imgpath, ImreadModes.Color);
            if (fixangle)
            {
                var angle = ImgPreOperate.GetAngle(imgpath);
                if (angle >= 0.7 && angle <= 359.3)
                { src = ImgPreOperate.GetFixedAngleImg(src, angle); }
            }

            var xymat = src.SubMat(xyrect);

            var availableimgpt = GetDetectPoint(src);
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

            var crectlist = Get5x1Rect(blurred, edged, xyenhance4x);

            if (crectlist.Count > 0)
            {
                cmatlist.Add(xyenhance);
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

            var availableimgpt = GetDetectPoint(src);
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
                    ret.Add(new Rect((int)xcmax - 226, y0, 48, y1));
                }
                else
                {
                    ret.Add(new Rect(0, y0, 48, y1));
                }

                ret.Add(new Rect((int)xcmax - 164, y0, 48, y1));
                ret.Add(new Rect((int)xcmax - 110, y0, 48, y1));
                ret.Add(new Rect((int)xcmax - 55, y0, 48, y1));

                ret.Add(new Rect((int)ycmin - 2, y0, 48, y1));
                ret.Add(new Rect((int)ycmin + 53, y0, 48, y1));
                ret.Add(new Rect((int)ycmin + 110, y0, 48, y1));

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
                        if ((item.X > ret[idx].X - 24) && (item.X < ret[idx].X + 24))
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

                            ret[idx] = currentrect;

                            if (!changedict.ContainsKey(idx))
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
                        if ((item.X > ret[idx].X - 24) && (item.X < ret[idx].X + 24))
                        {
                            if ((idx >= 0 && idx <= 2) || (idx >= 4 && idx <= 6))
                            {
                                var nextrect = new Rect(item.X + item.Width + 2, ret[idx].Y, item.Width + 4, ret[idx].Height);
                                if (idx + 1 == 7)
                                {
                                    nextrect = new Rect(item.X + item.Width + 4, ret[idx].Y, (edged.Width - item.X - 2 * item.Width - 8) > 0 ? (item.Width + 4) : (edged.Width - item.X - item.Width - 4), ret[idx].Height);
                                }

                                if (!changedict.ContainsKey(idx + 1))
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

                                if (!changedict.ContainsKey(idx - 1))
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
                    ret.Add(new Rect((int)xcmax - 226, y0, 48, y1));
                }
                else
                {
                    ret.Add(new Rect(0, y0, 48, y1));
                }

                ret.Add(new Rect((int)xcmax - 164, y0, 48, y1));
                ret.Add(new Rect((int)xcmax - 110, y0, 48, y1));
                ret.Add(new Rect((int)xcmax - 55, y0, 48, y1));

                ret.Add(new Rect((int)ycmin - 2, y0, 48, y1));
                ret.Add(new Rect((int)ycmin + 53, y0, 48, y1));
                ret.Add(new Rect((int)ycmin + 110, y0, 48, y1));

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