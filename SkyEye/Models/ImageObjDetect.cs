using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace SkyEye.Models
{
    public class ObjDetectItem
    {
        public static List<ObjDetectItem> Parse(string json)
        {
            return (List<ObjDetectItem>)Newtonsoft.Json.JsonConvert.DeserializeObject(json, (new List<ObjDetectItem>()).GetType());
        }

        public ObjDetectItem()
        {
            imgname = "";
            score = 0.0;
            left = 0.0;
            right = 0.0;
            top = 0.0;
            botm = 0.0;
        }

        public string imgname { set; get; }
        public double score { set; get; }
        public double left { set; get; }
        public double right { set; get; }
        public double top { set; get; }
        public double botm { set; get; }
    }

    public class ImageObjDetect
    {
        public static List<ObjDetectItem> PYOBJDect()
        {
            var ret = new List<ObjDetectItem>();
            var imgpath = @"\\wux-engsys01\PlanningForCast\VCSEL5\F5X1-TEST";
            var imgtype = "F5X1";
            var pathobj = new
            {
                imgpath = imgpath
                ,
                imgtype = imgtype
            };

            var reqstr = Newtonsoft.Json.JsonConvert.SerializeObject(pathobj);
            var response = PythonRESTFun("http://localhost:5000/OBJDetect", reqstr);
            if (!string.IsNullOrEmpty(response))
            {
                ret = ObjDetectItem.Parse(response);
            }
            return ret;
        }

        private static string PythonRESTFun(string url, string reqstr)
        {
            string webResponse = string.Empty;
            try
            {
                Uri uri = new Uri(url);
                WebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(reqstr);
                }

                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        webResponse = streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return webResponse;
        }
    }
}