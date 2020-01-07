using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyEye.Models
{
    public class SonImg
    {

        public void StoreData()
        {
            var sql = @"insert into WAT.dbo.SonImg(MainImgKey,ChildImgKey,ChildCat,ChildImg,ImgVal,ImgOrder,UpdateTime) 
                    values(@MainImgKey,@ChildImgKey,@ChildCat,@ChildImg,@ImgVal,@ImgOrder,@UpdateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@MainImgKey", MainImgKey);
            dict.Add("@ChildImgKey", ChildImgKey);
            dict.Add("@ChildCat", ChildCat);
            dict.Add("@ChildImg", ChildImg);
            dict.Add("@ImgVal", ImgVal.ToString());
            dict.Add("@ImgOrder", ImgOrder.ToString());
            dict.Add("@UpdateTime",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void UpdateImgVal(string imgkey,int val)
        {
            var sql = "update WAT.dbo.SonImg set ImgVal=@ImgVal where ChildImgKey=@ChildImgKey";
            var dict = new Dictionary<string, string>();
            dict.Add("@ChildImgKey", imgkey);
            dict.Add("@ImgVal", val.ToString());
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void UpdateImgVal(string MainImgKey, int idx, int val)
        {
            var sql = "update WAT.dbo.SonImg set ImgVal=@ImgVal where MainImgKey=@MainImgKey and ImgOrder=@ImgOrder";
            var dict = new Dictionary<string, string>();
            dict.Add("@MainImgKey", MainImgKey);
            dict.Add("@ImgOrder", idx.ToString());
            dict.Add("@ImgVal", val.ToString());
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public SonImg()
        {
            MainImgKey = "";
            ChildImgKey = "";
            ChildCat = "";
            ChildImg = "";
            ImgVal = -1;
            ImgOrder = 0;
            ImgChecked = "FALSE";
            UpdateTime = "";
        }

        public string MainImgKey { set; get; }
        public string ChildImgKey { set; get; }
        public string ChildCat { set; get; }
        public string ChildImg { set; get; }
        public int ImgVal { set; get; }
        public string ImgValStr { get {
                if (ImgVal == -1)
                {
                    return string.Empty;
                }
                else
                {
                    return Convert.ToString((char)ImgVal);
                }
            } }

        public int ImgOrder { set; get; }
        public string ImgChecked { set; get; }
        public string UpdateTime { set; get; }
    }
}