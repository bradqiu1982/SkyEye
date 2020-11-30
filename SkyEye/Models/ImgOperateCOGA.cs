using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyEye.Models
{
    public class ImgOperateCOGA
    {
        public void RunCOGA(string imgpath, List<double> ratelist)
        {
            Mat srcrealimg = Cv2.ImRead(imgpath, ImreadModes.Color);
            //using (new Window("original", srcrealimg))
            //{
            //    Cv2.WaitKey();
            //}

            Cv2.DetailEnhance(srcrealimg, srcrealimg);
            var denoisemat = new Mat();
            Cv2.FastNlMeansDenoisingColored(srcrealimg, denoisemat, 10, 10, 7, 21);


            var srcgray = new Mat();
            Cv2.CvtColor(denoisemat, srcgray, ColorConversionCodes.BGR2GRAY);

            var circles = Cv2.HoughCircles(srcgray, HoughMethods.Gradient, 1, srcgray.Rows / 8, 100, 80);

            var Cwl = (int)(srcgray.Width * 3);
            var Cwh = (int)(srcgray.Width * 0.7);

            var Ce0 = 15;
            var Ce1 = srcgray.Width - 15;

            var Chl = (int)(srcgray.Height * 0.333);
            var Chh = (int)(srcgray.Height * 0.666);

            var filtercircle = new List<CircleSegment>();

            foreach (var ccl in circles)
            {
                //var ccl = circles[0];
                if ((ccl.Center.X < Cwl || ccl.Center.X > Cwh)
                    && (ccl.Center.X > Ce0 && ccl.Center.X < Ce1)
                    && (ccl.Center.Y > Chl && ccl.Center.Y < Chh))
                {
                    //Cv2.Circle(denoisemat, (int)ccl.Center.X, (int)ccl.Center.Y, (int)ccl.Radius, new Scalar(0, 255, 0), 3);
                    filtercircle.Add(ccl);
                }
            }

            if (filtercircle.Count > 0)
            {
                var blurred = new Mat();
                Cv2.GaussianBlur(srcgray, blurred, new Size(3, 3), 0);

                //var edged = new Mat();
                //Cv2.Canny(blurred, edged, 50, 200, 3, false);

                var edged = new Mat();
                Cv2.AdaptiveThreshold(blurred, edged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);

                //var edged = new Mat();
                //Cv2.Threshold(blurred, edged, 50, 200, ThresholdTypes.BinaryInv);

                //using (new Window("edged", edged))
                //{
                //    Cv2.WaitKey();
                //}


                var linedetc = OpenCvSharp.XImgProc.FastLineDetector.Create(60);
                var lines = linedetc.Detect(edged);
                var cc = filtercircle[0].Center;

                var lyl = cc.Y - 75;
                var lyh = cc.Y + 75;
                var filterlines = new List<Vec4f>();

                var vlines = new List<Vec4f>();
                var hlines = new List<Vec4f>();

                foreach (var l in lines)
                {
                    if (l.Item1 > lyl && l.Item1 < lyh && l.Item3 > lyl && l.Item3 < lyh
                        && Math.Abs(l.Item0 - cc.X) < srcgray.Width / 2 && Math.Abs(l.Item2 - cc.X) < srcgray.Width / 2)
                    {
                        filterlines.Add(l);
                        if (Math.Abs(l.Item0 - l.Item2) > 30)
                        { hlines.Add(l); }
                        else
                        { vlines.Add(l); }
                    }
                }

                linedetc.DrawSegments(srcgray, filterlines, true);

                //using (new Window("srcgray", srcgray))
                //{
                //    Cv2.WaitKey();
                //}

                if (vlines.Count > 0 && hlines.Count > 0)
                {
                    //vlines.Sort(delegate (Vec4f obj1,Vec4f obj2) {
                    //   return obj1.Item0.CompareTo(obj2.Item0);
                    //});

                    //hlines.Sort(delegate (Vec4f obj1, Vec4f obj2) {
                    //    return obj1.Item1.CompareTo(obj2.Item1);
                    //});

                    var regionmidx = (int)((hlines[0].Item0 + hlines[0].Item2) / 2);
                    var regionmidy = (int)cc.Y;

                    var upper = -1;
                    for (var idx = regionmidy; idx > 0; idx--)
                    {
                        foreach (var l in hlines)
                        {
                            if (l.Item1 < regionmidy && upper == -1 && l.Item1 >= idx)
                            {
                                upper = (int)l.Item1;
                                break;
                            }
                        }
                        if (upper != -1) { break; }
                    }//end for

                    var botm = -1;
                    for (var idx = regionmidy; idx < srcgray.Height; idx++)
                    {
                        foreach (var l in hlines)
                        {
                            if (l.Item1 > regionmidy && botm == -1 && l.Item1 <= idx)
                            {
                                botm = (int)l.Item1;
                                break;
                            }
                        }
                        if (botm != -1) { break; }
                    }//end for

                    var left = -1;
                    for (var idx = regionmidx; idx > 0; idx--)
                    {
                        foreach (var l in vlines)
                        {
                            if (l.Item0 < regionmidx && left == -1 && l.Item0 >= idx)
                            {
                                left = (int)l.Item0;
                                break;
                            }
                        }
                        if (left != -1) { break; }
                    }//end for

                    var right = -1;
                    for (var idx = regionmidx; idx < srcgray.Width; idx++)
                    {
                        foreach (var l in vlines)
                        {
                            if (l.Item0 > regionmidx && right == -1 && l.Item0 <= idx)
                            {
                                right = (int)l.Item0;
                                break;
                            }
                        }
                        if (right != -1) { break; }
                    }//end for

                    if (upper != -1 && botm != -1)
                    {

                        //AVG 105,  >90  <120
                        ratelist.Add(botm - upper);

                        if (left != -1 && right == -1)
                        {
                            right = left + 150;
                        }
                        else if (left == -1 && right != -1)
                        {
                            left = right - 150;
                        }
                        else if (left == -1 && right == -1)
                        {
                            left = regionmidx - 75;
                            right = regionmidx + 75;
                        }

                        var coordmat = srcrealimg.SubMat(upper, botm, left, right);
                        if (cc.X < regionmidx)
                        {
                            var outxymat = new Mat();
                            Cv2.Transpose(coordmat, outxymat);
                            Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                            Cv2.Transpose(outxymat, outxymat);
                            Cv2.Flip(outxymat, outxymat, FlipMode.Y);
                            coordmat = outxymat;
                        }

                        //using (new Window("coordmat", coordmat))
                        //{
                        //    Cv2.WaitKey();
                        //}

                        Cv2.DetailEnhance(coordmat, coordmat);

                        var xyenhance4x = new Mat();
                        Cv2.Resize(coordmat, xyenhance4x, new Size(coordmat.Width * 1.6, coordmat.Height * 1.6));


                        Cv2.DetailEnhance(xyenhance4x, xyenhance4x);

                        var xyenhgray = new Mat();
                        var denoisemat1 = new Mat();
                        Cv2.FastNlMeansDenoisingColored(xyenhance4x, denoisemat1, 10, 10, 7, 21);
                        Cv2.CvtColor(denoisemat1, xyenhgray, ColorConversionCodes.BGR2GRAY);


                        var blurred1 = new Mat();
                        Cv2.GaussianBlur(xyenhgray, blurred1, new Size(13, 13), 3.2);

                        var coordedged = new Mat();
                        Cv2.AdaptiveThreshold(blurred1, coordedged, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 17, 15);


                        //using (new Window("edged1", coordedged))
                        //{
                        //    Cv2.WaitKey();
                        //}

                        var xl = (int)(coordedged.Width * 0.35);
                        var xh = (int)(coordedged.Width * 0.65);
                        var xmidy = (int)(coordedged.Height * 0.25);

                        var xupper = GetCoordUpper(coordedged, xl, xh, xmidy, 12);
                        var xmaxy = (int)(coordedged.Height * 0.5 + 20);
                        var xbotm = GetCoordBotm(coordedged, xl, xh, xmidy, xmaxy);

                        var ymidy = (int)(coordedged.Height * 0.75);
                        var yupper = GetCoordUpper(coordedged, xl, xh, ymidy, xbotm);
                        var ybotm = GetCoordBotm(coordedged, xl, xh, ymidy, coordedged.Height - 12);


                        var xposlist = new List<int>();

                        try
                        {
                            var xc = 26;
                            var step = (int)(coordedged.Width * 0.22);
                            var coordstart = GetFirstPos(coordedged, 14, step, xupper, xbotm, yupper, ybotm);
                            if (coordstart != -1)
                            {
                                xposlist.Add(coordstart);
                                var xpos1 = GetXPos(coordedged, coordstart + xc, coordstart + step + xc, xupper, xbotm, yupper, ybotm);
                                if (xpos1 != -1)
                                {
                                    xposlist.Add(xpos1);
                                    var xpos2 = GetXPos(coordedged, xpos1 + xc, xpos1 + step + xc, xupper, xbotm, yupper, ybotm);
                                    if (xpos2 != -1)
                                    {
                                        xposlist.Add(xpos2);
                                        var xpos3 = GetXPos(coordedged, xpos2 + xc, xpos2 + step + xc, xupper, xbotm, yupper, ybotm);
                                        if (xpos3 != -1)
                                        { xposlist.Add(xpos3); }
                                    }//end xpos2
                                }//end xpos1
                            }//end coordstart
                        }
                        catch (Exception ex) { xposlist.Clear(); }


                        var cmlist = new List<Mat>();

                        if (xposlist.Count > 0)
                        {
                            if (xposlist.Count == 4)
                            {
                                var fntlist = new List<int>();
                                fntlist.Add(xposlist[1] - xposlist[0]);
                                fntlist.Add(xposlist[2] - xposlist[1]);
                                fntlist.Add(xposlist[3] - xposlist[2]);
                                var fntwd = (int)fntlist.Average() - 2;
                                var xend = xposlist[3] + fntwd;
                                if (coordedged.Width < xend)
                                { xend = coordedged.Width - 3; }

                                var x1 = coordedged.SubMat(xupper, xbotm, xposlist[0], xposlist[1]);
                                var x2 = coordedged.SubMat(xupper, xbotm, xposlist[1], xposlist[2]);
                                var x3 = coordedged.SubMat(xupper, xbotm, xposlist[2], xposlist[3]);
                                var x4 = coordedged.SubMat(xupper, xbotm, xposlist[3], xend);

                                var y1 = coordedged.SubMat(yupper, ybotm, xposlist[0], xposlist[1]);
                                var y2 = coordedged.SubMat(yupper, ybotm, xposlist[1], xposlist[2]);
                                var y3 = coordedged.SubMat(yupper, ybotm, xposlist[2], xposlist[3]);
                                var y4 = coordedged.SubMat(yupper, ybotm, xposlist[3], xend);

                                cmlist.Add(x1); cmlist.Add(x2); cmlist.Add(x3); cmlist.Add(x4);
                                cmlist.Add(y1); cmlist.Add(y2); cmlist.Add(y3); cmlist.Add(y4);
                            }
                            else if (xposlist.Count == 3)
                            {
                                var fntlist = new List<int>();
                                fntlist.Add(xposlist[1] - xposlist[0]);
                                fntlist.Add(xposlist[2] - xposlist[1]);
                                var fntwd = (int)fntlist.Average();

                                var xend = xposlist[2] + 2 * fntwd;
                                if ((coordedged.Width - 2) < xend)
                                { xend = coordedged.Width - 3; }

                                var x1 = coordedged.SubMat(xupper, xbotm, xposlist[0], xposlist[1]);
                                var x2 = coordedged.SubMat(xupper, xbotm, xposlist[1], xposlist[2]);
                                var x3 = coordedged.SubMat(xupper, xbotm, xposlist[2], xposlist[2] + fntwd);
                                var x4 = coordedged.SubMat(xupper, xbotm, xposlist[2] + fntwd + 1, xend);

                                var y1 = coordedged.SubMat(yupper, ybotm, xposlist[0], xposlist[1]);
                                var y2 = coordedged.SubMat(yupper, ybotm, xposlist[1], xposlist[2]);
                                var y3 = coordedged.SubMat(yupper, ybotm, xposlist[2], xposlist[2] + fntwd);
                                var y4 = coordedged.SubMat(yupper, ybotm, xposlist[2] + fntwd + 1, xend);

                                cmlist.Add(x1); cmlist.Add(x2); cmlist.Add(x3); cmlist.Add(x4);
                                cmlist.Add(y1); cmlist.Add(y2); cmlist.Add(y3); cmlist.Add(y4);
                            }
                            else if (xposlist.Count == 2)
                            {
                                var fntlist = new List<int>();
                                fntlist.Add(xposlist[1] - xposlist[0]);
                                var fntwd = (int)fntlist.Average() + 1;
                                var xend = xposlist[1] + 3 * fntwd;
                                if ((coordedged.Width - 2) < xend)
                                { xend = coordedged.Width - 3; }

                                var x1 = coordedged.SubMat(xupper, xbotm, xposlist[0], xposlist[1]);
                                var x2 = coordedged.SubMat(xupper, xbotm, xposlist[1], xposlist[1] + fntwd);
                                var x3 = coordedged.SubMat(xupper, xbotm, xposlist[1] + fntwd + 1, xposlist[1] + 2 * fntwd);
                                var x4 = coordedged.SubMat(xupper, xbotm, xposlist[1] + 2 * fntwd + 1, xend);

                                var y1 = coordedged.SubMat(yupper, ybotm, xposlist[0], xposlist[1]);
                                var y2 = coordedged.SubMat(yupper, ybotm, xposlist[1], xposlist[1] + fntwd);
                                var y3 = coordedged.SubMat(yupper, ybotm, xposlist[1] + fntwd + 1, xposlist[1] + 2 * fntwd);
                                var y4 = coordedged.SubMat(yupper, ybotm, xposlist[1] + 2 * fntwd + 1, xend);

                                cmlist.Add(x1); cmlist.Add(x2); cmlist.Add(x3); cmlist.Add(x4);
                                cmlist.Add(y1); cmlist.Add(y2); cmlist.Add(y3); cmlist.Add(y4);
                            }
                            else
                            {
                                var xc = xposlist[0];
                                var xstep = (int)((coordedged.Width - xc) * 0.21);
                                var x1 = coordedged.SubMat(xupper, xbotm, xc, xstep + xc);
                                var x2 = coordedged.SubMat(xupper, xbotm, xstep + xc, 2 * xstep + xc);
                                var x3 = coordedged.SubMat(xupper, xbotm, 2 * xstep + xc, 3 * xstep + xc);
                                var x4 = coordedged.SubMat(xupper, xbotm, 3 * xstep + xc, 4 * xstep + xc);

                                var y1 = coordedged.SubMat(yupper, ybotm, xc, xstep + xc);
                                var y2 = coordedged.SubMat(yupper, ybotm, xstep + xc, 2 * xstep + xc);
                                var y3 = coordedged.SubMat(yupper, ybotm, 2 * xstep + xc, 3 * xstep + xc);
                                var y4 = coordedged.SubMat(yupper, ybotm, 3 * xstep + xc, 4 * xstep + xc);

                                cmlist.Add(x1); cmlist.Add(x2); cmlist.Add(x3); cmlist.Add(x4);
                                cmlist.Add(y1); cmlist.Add(y2); cmlist.Add(y3); cmlist.Add(y4);
                            }
                        }
                        else
                        {
                            var xc = 25;
                            var xstep = (int)((coordedged.Width - xc) * 0.21);
                            var x1 = coordedged.SubMat(xupper, xbotm, xc, xstep + xc);
                            var x2 = coordedged.SubMat(xupper, xbotm, xstep + xc, 2 * xstep + xc);
                            var x3 = coordedged.SubMat(xupper, xbotm, 2 * xstep + xc, 3 * xstep + xc);
                            var x4 = coordedged.SubMat(xupper, xbotm, 3 * xstep + xc, 4 * xstep + xc);

                            var y1 = coordedged.SubMat(yupper, ybotm, xc, xstep + xc);
                            var y2 = coordedged.SubMat(yupper, ybotm, xstep + xc, 2 * xstep + xc);
                            var y3 = coordedged.SubMat(yupper, ybotm, 2 * xstep + xc, 3 * xstep + xc);
                            var y4 = coordedged.SubMat(yupper, ybotm, 3 * xstep + xc, 4 * xstep + xc);

                            cmlist.Add(x1); cmlist.Add(x2); cmlist.Add(x3); cmlist.Add(x4);
                            cmlist.Add(y1); cmlist.Add(y2); cmlist.Add(y3); cmlist.Add(y4);
                        }

                        //var idx = 1;
                        //foreach (var cm in cmlist)
                        //{
                        //    using (new Window("char "+idx, cm))
                        //    {
                        //        Cv2.WaitKey();
                        //    }
                        //    idx++;
                        //}

                    }//get coord box

                }//vline,hline

            }

        }

        public int CheckMatBlack(Mat edge, int startx, int endx, int upper, int botm)
        {
            var times = 0;
            for (var idx = startx; idx < endx;)
            {
                var snapmat = edge.SubMat(upper + 2, botm - 2, idx, idx + 2);
                var cnt = snapmat.CountNonZero();
                if (cnt < 2 && times >= 5)
                { return idx + 2; }
                idx = idx + 2;
                times++;
            }

            return -1;
        }

        public int GetXPos(Mat edge, int startx, int endx, int xupper, int xbotm, int yupper, int ybotm)
        {
            var xstart = CheckMatBlack(edge, startx, endx, xupper, xbotm);
            var ystart = CheckMatBlack(edge, startx, endx, yupper, ybotm);
            if (xstart != -1 && ystart != -1)
            {
                if (xstart > ystart)
                { return ystart; }
                else
                { return xstart; }
            }
            else if (xstart != -1)
            { return xstart; }
            else if (ystart != -1)
            { return ystart; }
            else
            { return -1; }
        }


        public int CheckMatWhite(Mat edge, int startx, int endx, int upper, int botm)
        {
            var times = 0;
            for (var idx = startx; idx < endx;)
            {
                var snapmat = edge.SubMat(upper + 2, botm - 2, idx, idx + 2);
                var cnt = snapmat.CountNonZero();
                if (cnt >= 10 && times >= 3)
                { return idx - 2; }
                idx = idx + 2;
                times++;
            }

            return -1;
        }

        public int GetFirstPos(Mat edge, int startx, int endx, int xupper, int xbotm, int yupper, int ybotm)
        {
            var xstart = CheckMatWhite(edge, startx, endx, xupper, xbotm);
            var ystart = CheckMatWhite(edge, startx, endx, yupper, ybotm);
            if (xstart != -1 && ystart != -1)
            {
                if (xstart > ystart)
                { return xstart; }
                else
                { return ystart; }
            }
            else if (xstart != -1)
            { return xstart; }
            else if (ystart != -1)
            { return ystart; }
            else
            { return -1; }
        }

        public int GetCoordUpper(Mat edge, int xl, int xh, int midy, int miny)
        {
            for (var idx = midy; idx > miny;)
            {
                var snapmat = edge.SubMat(idx - 2, idx, xl, xh);
                var cnt = snapmat.CountNonZero();
                if (cnt < 3)
                { return idx - 2; }
                idx = idx - 3;
            }

            return miny;
        }

        public int GetCoordBotm(Mat edge, int xl, int xh, int midy, int maxy)
        {
            for (var idx = midy; idx < maxy;)
            {
                var snapmat = edge.SubMat(idx, idx + 2, xl, xh);
                var cnt = snapmat.CountNonZero();
                if (cnt < 3)
                { return idx + 2; }
                idx = idx + 3;
            }

            return maxy;
        }

    }
}