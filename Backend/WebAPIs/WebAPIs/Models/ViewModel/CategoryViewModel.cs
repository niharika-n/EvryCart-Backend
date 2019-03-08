using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIs.Models
{
    public class CategoryViewModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int ID { get; set; }

        public bool IsActive { get; set; }

        public string CreatedUser { get; set; }

        public string ModifiedUser { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public Nullable<int> ModifiedBy { get; set; }

        public Nullable<DateTime> ModifiedDate { get; set; }

        public bool Parent { get; set; }

        public Nullable<int> Child { get; set; }

        public string ImageContent { get; set; }

        public Nullable<int> AssociatedProducts { get; set; }
    }
}
