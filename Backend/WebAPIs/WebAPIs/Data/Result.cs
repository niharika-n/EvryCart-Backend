using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebAPIs.Models;

namespace WebAPIs.Data
{
    public interface IResult
    {
        bool Status { get; set; }
        HttpStatusCode StatusCode { get; set; }
        dynamic Body { get; set; }
        string Message { get; set; }
    }

    public class Result : IResult
    {
        public bool Status { get; set; } = false;
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public dynamic Body { get; set; } = null;
        public string Message { get; set; } = "";
    }


}
