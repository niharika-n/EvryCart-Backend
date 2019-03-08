using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIs.Models
{
    public class ProductAttributeViewModel
    {
        public int AttributeID { get; set; }

        public string AttributeName { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string CreatedUser { get; set; }

        public string ModifiedUser { get; set; }

        public Nullable<int> AssociatedProductValues { get; set; }
    }
}
