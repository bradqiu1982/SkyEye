using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

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
        public static List<ObjDetectItem> PYOBJDect(string imgpath,string imgtype)
        {
            var ret = new List<ObjDetectItem>();
            var pathobj = new
            {
                imgpath = imgpath,
                imgtype = imgtype
            };

            var reqstr = Newtonsoft.Json.JsonConvert.SerializeObject(pathobj);
            var response = PythonRESTFun("http://localhost:5000/SingleOBJDetect", reqstr);
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

        public static string WriteRawImg(Mat rawimg, Controller ctrl)
        {
            try
            {
                var fn = GetUniqKey() + ".jpg";
                string datestring = DateTime.Now.ToString("yyyyMMdd");
                string imgdir = ctrl.Server.MapPath("~/userfiles") + "\\images\\" + datestring + "\\";
                if (!Directory.Exists(imgdir))
                { Directory.CreateDirectory(imgdir); }
                var wholefn = imgdir + fn;
                Cv2.ImWrite(wholefn, rawimg);
                return wholefn;
            }
            catch (Exception ex) { }

            return string.Empty;
        }

        private static string GetUniqKey()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}