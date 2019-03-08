using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIs.Models
{
    public class StatisticsModel
    {
        public List<CategoryViewModel> CategoryResult { get; set; }

        public List<ProductViewModel> ProductResult { get; set; }

        public int CategoryCount { get; set; }

        public int ProductCount { get; set; }

        public DateTime? CategoryLastUpdated { get; set; }

        public DateTime? ProductLastUpdated { get; set; }
    }
}
