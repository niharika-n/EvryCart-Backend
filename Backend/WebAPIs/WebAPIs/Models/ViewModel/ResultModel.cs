using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIs.Models
{
    public class ResultModel
    {
        public List<CategoryViewModel> CategoryResult { get; set; }

        public List<ProductViewModel> ProductResult { get; set; }

        public List<ProductImageViewModel> ProductImageResult { get; set; }

        public List<ProductAttributeViewModel> ProductAttributeResult { get; set; }

        public List<ProductAttributeValueViewModel> ProductAttributeValueResult { get; set; }

        public int TotalCount { get; set; }
    }
}
