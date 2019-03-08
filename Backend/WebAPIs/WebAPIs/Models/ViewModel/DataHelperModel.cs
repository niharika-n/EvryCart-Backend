using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIs.Models
{
    public class DataHelperModel
    {
        public string Search { get; set; }

        public int PageNumber { get; set; } = 1;

        public bool SortOrder { get; set; } = true;

        public string SortColumn { get; set; }

        public int PageSize { get; set; } = 5;

    }
}
