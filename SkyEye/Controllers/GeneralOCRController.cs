using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SkyEye.Models;


namespace SkyEye.Controllers
{
    public class GeneralOCRController : ApiController
    {
        //http://wuxinpi.china.ads.finisar.com:9091/api/GeneralOCR/GetSNCoord?LotNum=S2004100792
        [HttpGet]
        public string GetSNCoord(string LotNum)
        {
            var client = new RestSharp.RestClient("http://localhost:9091/Main/RefreshLotCoord");
            var request = new RestSharp.RestRequest(RestSharp.Method.GET);
            var response = client.Execute(request);
            if (response.IsSuccessful)
            {}
            client.ClearHandlers();

            return GeneralOCRVM.GetSNCoord(LotNum);
        }


    }
}
