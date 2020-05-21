using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyEye.Models
{
    public class WaterMark
    {
        public static Mat GetWaterMarkSample()
        {
            var blank = new Mat(new Size(240, 60), MatType.CV_32FC3, new Scalar(255, 255, 255));
            //var xblank = new Mat();
            //Cv2.CvtColor(blank, xblank, ColorConversionCodes.GRAY2RGB);
            Cv2.PutText(blank, "HELLO WORLD", new Point(6, 40), HersheyFonts.HersheySimplex, 1, new Scalar(0, 0, 255), 2, LineTypes.Link8);
            Cv2.PutText(blank, "SkyEye", new Point(205, 52), HersheyFonts.HersheySimplex, 0.3, new Scalar(0, 0, 0), 1, LineTypes.Link8);
            //using (new Window("blank", blank))
            //{
            //    Cv2.WaitKey();
            //}


            //blank = new Mat(new Size(240, 60), MatType.CV_32FC3, new Scalar(255, 255, 255));
            ////var xblank = new Mat();
            ////Cv2.CvtColor(blank, xblank, ColorConversionCodes.GRAY2RGB);
            //Cv2.PutText(blank, "HELLO WORLD", new Point(6, 40), HersheyFonts.HersheySimplex, 1, new Scalar(255, 0, 0), 2, LineTypes.Link8);
            //Cv2.PutText(blank, "SkyEye", new Point(205, 52), HersheyFonts.HersheySimplex, 0.3, new Scalar(0, 0, 0), 1, LineTypes.Link8);
            //using (new Window("blank1", blank))
            //{
            //    Cv2.WaitKey();
            //}

            return blank;
        }
    }
}