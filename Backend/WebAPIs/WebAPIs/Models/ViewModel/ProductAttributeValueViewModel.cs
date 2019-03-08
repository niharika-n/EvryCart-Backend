using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIs.Models
{
    public class ProductAttributeValueViewModel
    {
        public int ID { get; set; }

        public int AttributeID { get; set; }

        public string AttributeName { get; set; }

        public int ProductID { get; set; }

        public string Value { get; set; }

    }
}
