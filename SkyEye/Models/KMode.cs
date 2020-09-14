using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenCvSharp;
using System.Web.Mvc;
using System.Web.Caching;

namespace SkyEye.Models
{
    public class KMode
    {
        public static OpenCvSharp.ML.KNearest GetTrainedMode(string caprev,Controller ctrl)
        {
            var traindatas = GetTrainData(caprev, ctrl);
            var samplex = new Mat();
            var samples = new Mat();
            samplex.ConvertTo(samples, MatType.CV_32FC1);
            var responsarray = new List<int>();
            foreach (var item in traindatas)
            {
                var tcmresizex = Mat.ImDecode(Convert.FromBase64String(item.TrainingImg),ImreadModes.Grayscale);
                var tcmresize = new Mat();
                tcmresizex.ConvertTo(tcmresize, MatType.CV_32FC1);
                var stcm = tcmresize.Reshape(1, 1);
                samples.PushBack(stcm);
                responsarray.Add(item.ImgVal);
            }

            int[] rparray = responsarray.ToArray();
            var responx = new Mat(rparray.Length, 1, MatType.CV_32SC1, rparray);
            var respons = new Mat();
            responx.ConvertTo(respons, MatType.CV_32FC1);

            var kmode = OpenCvSharp.ML.KNearest.Create();
            kmode.Train(samples, OpenCvSharp.ML.SampleTypes.RowSample, respons);
            return kmode;
        }

        private static List<AITrainingData> GetTrainData(string caprev, Controller ctrl)
        {
            var obj = ctrl.HttpContext.Cache.Get(caprev + "_AIKEY");
            if (obj != null)
            { return (List<AITrainingData>)obj; }

            var traindatas = AITrainingData.GetTrainingData(caprev);
            if (traindatas.Count == 0)
            { traindatas = AITrainingData.GetTrainingData("OGP-rect5x1"); }

            if (traindatas.Count > 0)
            { ctrl.HttpContext.Cache.Insert(caprev + "_AIKEY", traindatas, null, DateTime.Now.AddHours(4), Cache.NoSlidingExpiration); }

            return traindatas;
        }

        public static void CleanTrainCache(Controller ctrl)
        {
            var mycache = ctrl.HttpContext.Cache;
            var citem = mycache.GetEnumerator();
            var ckeylist = new List<string>();
            while (citem.MoveNext())
            {
                var ckey = Convert.ToString(citem.Key);
                ckeylist.Add(ckey);
            }

            foreach (var ckey in ckeylist)
            {
                if (ckey.Contains("_AIKEY"))
                {
                    mycache.Remove(ckey);
                }
            }
        }

        //add SVM support
        public static OpenCvSharp.ML.SVM GetTrainedSMode(string caprev, Controller ctrl)
        {
            var traindatas = GetTrainData(caprev, ctrl);
            var samplex = new Mat();
            var samples = new Mat();
            samplex.ConvertTo(samples, MatType.CV_32FC1);
            var respmatx = new Mat();
            var respmat = new Mat();
            respmatx.ConvertTo(respmat, MatType.CV_32SC1);

            foreach (var item in traindatas)
            {
                var tcmresizex = Mat.ImDecode(Convert.FromBase64String(item.TrainingImg), ImreadModes.Grayscale);
                var tcmresize = new Mat();
                tcmresizex.ConvertTo(tcmresize, MatType.CV_32FC1);
                var stcm = tcmresize.Reshape(1, 1);
                samples.PushBack(stcm);
                respmat.PushBack(item.ImgVal);
            }

            var smode = OpenCvSharp.ML.SVM.Create();
            smode.Type = OpenCvSharp.ML.SVM.Types.CSvc;
            smode.KernelType = OpenCvSharp.ML.SVM.KernelTypes.Linear;
            smode.TermCriteria = TermCriteria.Both(5000, 0.000001);
            smode.Train(samples, OpenCvSharp.ML.SampleTypes.RowSample, respmat);
            return smode;
        }

        public static Mat GetOneHot(int val)
        {
            var res = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var idx = val - 48;
            if (idx >= 0 && idx <= 9)
            { res[idx] = 1; }
            return new Mat(1, 10, MatType.CV_32FC1, res);
        }

        public static OpenCvSharp.ML.ANN_MLP GetTrainedANNMode(string caprev)
        {
            var traindatas = AITrainingData.GetTrainingData(caprev); ;
            var samplex = new Mat();
            var samples = new Mat();
            samplex.ConvertTo(samples, MatType.CV_32FC1);
            var respmatx = new Mat();
            var respmat = new Mat();
            respmatx.ConvertTo(respmat, MatType.CV_32FC1);

            foreach (var item in traindatas)
            {
                var tcmresizex = Mat.ImDecode(Convert.FromBase64String(item.TrainingImg), ImreadModes.Grayscale);
                var tcmresize = new Mat();
                tcmresizex.ConvertTo(tcmresize, MatType.CV_32FC1);
                var stcm = tcmresize.Reshape(0, 1);
                samples.PushBack(stcm);
                respmat.PushBack(GetOneHot(item.ImgVal).Reshape(0, 1));
            }

            var smode = OpenCvSharp.ML.ANN_MLP.Create();

            var hide = (int)Math.Sqrt(50*50*10);
            var layarray = new int[] { 50*50, hide,hide,hide, 10 };
            var laysize = InputArray.Create(layarray);
            smode.SetLayerSizes(laysize);

            smode.BackpropWeightScale = 0.0001;

            smode.TermCriteria = new TermCriteria(CriteriaType.MaxIter, 200, 0.000001);
            smode.Train(samples, OpenCvSharp.ML.SampleTypes.RowSample, respmat);

            return smode;
        }

    }
}