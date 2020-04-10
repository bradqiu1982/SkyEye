﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SkyEye.Models
{
    public class GeneralOCRVM
    {
        public static void RefreshNewLotNum(Controller ctrl)
        {
            if (!CheckRefreshFile(ctrl))
            { return; }

            try
            {
                var ocrlist = new List<GeneralOCRVM>();

                var latesttime = GetLatestUpdateTime();
                var dict = new Dictionary<string, string>();
                dict.Add("@LoadTimestamp", latesttime);

                var sql = "select [Lot],[LoadTimestamp],[PC],[Product],[Path],[Employee] from [AIProjects].[dbo].[OGP_Ins_Online_Lot] where [LoadTimestamp] >= @LoadTimestamp";
                var dbret = DBUtility.ExeMeOCRSqlWithRes(sql, dict);
                foreach (var line in dbret)
                {
                    var tempvm = new GeneralOCRVM();
                    tempvm.LotNum = UT.O2S(line[0]);
                    tempvm.UploadTime = UT.T2S(line[1]);
                    tempvm.UploadMachine = UT.O2S(line[2]);
                    tempvm.Product = UT.O2S(line[3]);
                    tempvm.RawPath = UT.O2S(line[4]);
                    tempvm.Uploader = UT.O2S(line[5]);
                    ocrlist.Add(tempvm);
                }
                foreach (var ocr in ocrlist)
                {
                    ocr.StoreData();
                }
            }
            catch (Exception ex) { }

            CleanRefreshFile(ctrl);
        }

        private static bool CheckRefreshFile(Controller ctrl)
        {
            var file = ctrl.Server.MapPath("~/userfiles") + "\\Refeshing_" + DateTime.Now.ToString("yyyyMMdd");
            if (System.IO.File.Exists(file))
            { return false; }
            System.IO.File.WriteAllText(file, "HELLO");
            return true;
        }

        private static void CleanRefreshFile(Controller ctrl)
        {
            var file = ctrl.Server.MapPath("~/userfiles") + "\\Refeshing_" + DateTime.Now.ToString("yyyyMMdd");
            if (System.IO.File.Exists(file))
            {
                try
                { System.IO.File.Delete(file); }
                catch (Exception ex) { }
            }
        }

        private void StoreData()
        {
            var sql = "insert into GeneralOCRVM(OCRKey,LotNum,Product,UploadMachine,UploadTime,RawPath,Uploader) values(@OCRKey,@LotNum,@Product,@UploadMachine,@UploadTime,@RawPath,@Uploader)";
            var dict = new Dictionary<string, string>();
            dict.Add("@OCRKey", Guid.NewGuid().ToString("N"));
            dict.Add("@LotNum",LotNum);
            dict.Add("@Product", Product);
            dict.Add("@UploadMachine", UploadMachine);
            dict.Add("@UploadTime", UploadTime);
            dict.Add("@RawPath", RawPath);
            dict.Add("@Uploader", Uploader);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static string GetLatestUpdateTime()
        {
            var sql = "select top 1 UploadTime from GeneralOCRVM order by UploadTime desc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var ret = UT.O2T(line[0]).AddSeconds(1).ToString("yyyy-MM-dd HH:mm:ss");
                if (!string.IsNullOrEmpty(ret))
                { return ret; }
            }
            return "2020-04-10 00:00:00";
        }
        
        public static void ParseNewLot(Controller ctrl)
        {
            var unparsedlist = GetUnParsedData();
            foreach (var ocr in unparsedlist)
            {
                try
                {
                    var ret = ParseNewLot_(ocr.LotNum, ocr.RawPath, ctrl);
                    if (ret)
                    { CompleteParse(ocr.OCRKey); }
                }
                catch (Exception ex) { }
            }
        }

        private static bool ParseNewLot_(string lotnum,string path,Controller ctrl)
        {
            var imgpath = path;
            if (!imgpath.ToUpper().Contains("RAW"))
            { imgpath = imgpath + "\\RAW"; }

            OGPFatherImg.CleanWaferData(lotnum);
            KMode.CleanTrainCache(ctrl);

            var filelist = ExternalDataCollector.DirectoryEnumerateFiles(ctrl, imgpath);
            var samplepicture = new List<string>();
            foreach (var fs in filelist)
            {
                var fn = System.IO.Path.GetFileName(fs).ToUpper();
                if (fn.Contains(".BMP") || fn.Contains(".PNG") || fn.Contains(".JPG"))
                {
                    samplepicture.Add(fs);
                    if (samplepicture.Count > 1)
                    { break; }
                }
            }

            if (samplepicture.Count == 0)
            { return false; }

            var caprev = "";
            caprev = OGPFatherImg.GetPictureRev(samplepicture[0]);
            if (string.IsNullOrEmpty(caprev))
            {
                caprev = OGPFatherImg.GetPictureRev(samplepicture[1]);
                if (string.IsNullOrEmpty(caprev))
                { return false; }
            }

            foreach (var fs in filelist)
            {
                var fn = System.IO.Path.GetFileName(fs).ToUpper();
                if (fn.Contains(".BMP") || fn.Contains(".PNG") || fn.Contains(".JPG"))
                {
                   OGPFatherImg.Load200xImg(fs, lotnum , caprev, ctrl);
                }
            }

            return true;
        }

        private static void CompleteParse(string OCRKey)
        {
            var sql = "update GeneralOCRVM set Parsed='TRUE' where OCRKey = @OCRKey";
            var dict = new Dictionary<string, string>();
            dict.Add("@OCRKey", OCRKey);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void ConfirmOCR(string OCRKey,string confirmer)
        {
            var sql = "update GeneralOCRVM set Confirmer = @Confirmer,ConfirmTime = @ConfirmTime where OCRKey = @OCRKey";
            var dict = new Dictionary<string, string>();
            dict.Add("@OCRKey", OCRKey);
            dict.Add("@Confirmer", confirmer);
            dict.Add("@ConfirmTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static List<GeneralOCRVM> GetUnParsedData()
        {
            var ret = new List<GeneralOCRVM>();
            var sql = "select OCRKey,LotNum,RawPath from GeneralOCRVM where Parsed = ''";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new GeneralOCRVM();
                tempvm.OCRKey = UT.O2S(line[0]);
                tempvm.LotNum = UT.O2S(line[1]);
                tempvm.RawPath = UT.O2S(line[2]);
                ret.Add(tempvm);
            }
            return ret;
        }


        public static List<GeneralOCRVM> GetOCRVM(string starttime, string endtime, string machine,string product, string lotnum)
        {
            var ret = new List<GeneralOCRVM>();

            var sql = "";
            var dict = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(lotnum))
            {
                dict.Add("@LotNum", lotnum);
                sql = "select OCRKey,LotNum,Product,UploadMachine,UploadTime,RawPath,Confirmer,ConfirmTime,Parsed,Uploader from GeneralOCRVM where LotNum = @LotNum";
            }
            else if (!string.IsNullOrEmpty(starttime) && !string.IsNullOrEmpty(endtime) && !string.IsNullOrEmpty(machine))
            {
                dict.Add("@UploadMachine", machine);
                dict.Add("@starttime", starttime);
                dict.Add("@endtime", endtime);
                sql = "select OCRKey,LotNum,Product,UploadMachine,UploadTime,RawPath,Confirmer,ConfirmTime,Parsed,Uploader from GeneralOCRVM  where UploadMachine = @UploadMachine and UploadTime >= @starttime and UploadTime <= @endtime";
            }
            else if (!string.IsNullOrEmpty(starttime) && !string.IsNullOrEmpty(endtime) && !string.IsNullOrEmpty(product))
            {
                dict.Add("@Product", product);
                dict.Add("@starttime", starttime);
                dict.Add("@endtime", endtime);
                sql = "select OCRKey,LotNum,Product,UploadMachine,UploadTime,RawPath,Confirmer,ConfirmTime,Parsed,Uploader from GeneralOCRVM  where Product = @Product and UploadTime >= @starttime and UploadTime <= @endtime";
            }

            if (string.IsNullOrEmpty(sql))
            { return new List<GeneralOCRVM>(); }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new GeneralOCRVM();
                tempvm.OCRKey = UT.O2S(line[0]);
                tempvm.LotNum = UT.O2S(line[1]);
                tempvm.Product = UT.O2S(line[2]);
                tempvm.UploadMachine = UT.O2S(line[3]);
                tempvm.UploadTime = UT.T2S(line[4]);
                tempvm.RawPath = UT.O2S(line[5]);
                tempvm.Confirmer = UT.O2S(line[6]);
                tempvm.ConfirmTime = UT.T2S(line[7]);
                if (string.IsNullOrEmpty(tempvm.Confirmer)) { tempvm.ConfirmTime = ""; }

                tempvm.Parsed = UT.O2S(line[8]);
                tempvm.Uploader = UT.O2S(line[9]);
                ret.Add(tempvm);
            }

            foreach (var item in ret)
            {
                if (!string.IsNullOrEmpty(item.Parsed))
                {
                    var xylist = OGPSNXYVM.GetLocalOGPXYSNDict(item.LotNum);
                    if (xylist.Count > 0)
                    { item.XYList.AddRange(xylist.Values.ToList());}
                }
            }

            return ret;
        }

        private static Dictionary<string, bool> GetParamList(string param)
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select distinct " + param + " from GeneralOCRVM";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var val = UT.O2S(line[0]);
                if (!string.IsNullOrEmpty(val) && !ret.ContainsKey(val))
                { ret.Add(val, true); }
            }
            return ret;
        }

        public static Dictionary<string, bool> GetLotNumList()
        {
            return GetParamList("LotNum");
        }

        public static Dictionary<string, bool> GetProductList()
        {
            return GetParamList("Product");
        }

        public static Dictionary<string, bool> GetMachineList()
        {
            return GetParamList("UploadMachine");
        }

        public GeneralOCRVM()
        {
            OCRKey = "";
            LotNum = "";
            Product = "";
            UploadMachine = "";
            UploadTime = "1982-05-06 10:00:00";
            Uploader = "";
            RawPath = "";
            Confirmer = "";
            ConfirmTime = "";
            Parsed = "";
            XYList = new List<OGPSNXYVM>();
        }

        public string OCRKey { set; get; }
        public string LotNum { set; get; }
        public string Product { set; get; }
        public string UploadMachine { set; get; }
        public string UploadTime { set; get; }
        public string RawPath { set; get; }
        public string Confirmer { set; get; }
        public string ConfirmTime { set; get; }
        public List<OGPSNXYVM> XYList { set; get; }
        public string Parsed { set; get; }
        public string Uploader { set; get; }
        public string Yield { get {
                if (!string.IsNullOrEmpty(Confirmer) && XYList.Count > 0)
                {
                    var md = 0;
                    foreach (var item in XYList)
                    {
                        if (!string.IsNullOrEmpty(item.Modified))
                        { md += 1; }
                    }
                    var count = XYList.Count;
                    var yield = Math.Round((double)(count - md) / (double)count * 100.0, 1).ToString()+"%";
                    return yield;
                }
                return "";
            } }
    }
}