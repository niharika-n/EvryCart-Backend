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
        Status Status { get; set; }
        HttpStatusCode StatusCode { get; set; }
        dynamic Body { get; set; }
        string Message { get; set; }
        Operation Operation { get; set; }
    }

    public class Result : IResult
    {
        public Status Status { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public dynamic Body { get; set; } = null;
        public string Message { get; set; } = "";
        public Operation Operation { get; set; }
    }


}
