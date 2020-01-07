using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyEye.Models
{
    public class AITrainingData
    {
        //public static List<SonImg> GetCheckedImgVal(string caprev)
        //{
        //    var ret = new List<SonImg>();

        //    var dict = new Dictionary<string, string>();
        //    var sql = "";
        //    if (string.IsNullOrEmpty(caprev))
        //    {
        //        sql = @"select top 30000 s.ChildImg,s.ImgVal from [WAT].[dbo].[SonImg] (nolock) s 
        //                    where s.ImgChecked = 'TRUE' order by UpdateTime desc";
        //    }
        //    else
        //    {
        //        sql = @"select top 30000 s.ChildImg,s.ImgVal from [WAT].[dbo].[SonImg] (nolock) s
        //                  inner join [WAT].[dbo].[FatherImg] (nolock) f on f.MainImgKey = s.MainImgKey
        //                  where s.ImgChecked = 'TRUE' and f.CaptureRev = @CaptureRev order by UpdateTime desc";
        //        dict.Add("@CaptureRev", caprev);
        //    }
        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
        //    foreach (var line in dbret)
        //    {
        //        var tempvm = new SonImg();
        //        tempvm.ChildImg = UT.O2S(line[0]);
        //        tempvm.ImgVal = UT.O2I(line[1]);
        //        ret.Add(tempvm);
        //    }
        //    return ret;
        //}

        public static List<AITrainingData> GetTrainingData(string revision)
        {
            var ret = new List<AITrainingData>();

            var dict = new Dictionary<string, string>();
            var sql = "";
            if (string.IsNullOrEmpty(revision))
            {
                sql = @"select top 30000 TrainingImg,ImgVal from [WAT].[dbo].[AITrainingData] order by UpdateTime desc";
            }
            else
            {
                sql = @"select top 30000  TrainingImg,ImgVal from [WAT].[dbo].[AITrainingData] where Revision=@Revision order by UpdateTime desc";
                dict.Add("@Revision", revision);
            }
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new AITrainingData();
                tempvm.TrainingImg = UT.O2S(line[0]);
                tempvm.ImgVal = UT.O2I(line[1]);
                ret.Add(tempvm);
            }
            return ret;
        }

        public void StoreData()
        {
            var sql = @"insert into AITrainingData(ImgKey,TrainingImg,ImgVal,Revision,UpdateTime,WaferNum) 
                        values(@ImgKey,@TrainingImg,@ImgVal,@Revision,@UpdateTime,@WaferNum)";
            var dict = new Dictionary<string, string>();
            dict.Add("@ImgKey", ImgKey);
            dict.Add("@TrainingImg", TrainingImg);
            dict.Add("@ImgVal", ImgVal.ToString());
            dict.Add("@Revision", Revision);
            dict.Add("@UpdateTime", UpdateTime);
            dict.Add("@WaferNum", WaferNum);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public AITrainingData() {
            ImgKey = "";
            TrainingImg = "";
            ImgVal = -1;
            Revision = "";
            UpdateTime = "1982-05-06 10:00:00";
            WaferNum = "";
        }

        public string ImgKey { set; get; }
        public string TrainingImg { set; get; }
        public int ImgVal { set; get; }
        public string Revision { set; get; }
        public string UpdateTime { set; get; }
        public string WaferNum { set; get; }

    }
}