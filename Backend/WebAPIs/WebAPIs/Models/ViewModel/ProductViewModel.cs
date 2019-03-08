using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIs.Models
{
    public class ProductViewModel
    {
        public int ProductID { get; set; }

        public string ProductName { get; set; }

        public string ShortDescription { get; set; }

        public string LongDescription { get; set; }

        public int CategoryID { get; set; }

        public string CategoryName { get; set; }

        public bool IsActive { get; set; }

        public int Price { get; set; }

        public int QuantityInStock { get; set; }

        public string QuantityType { get; set; }

        public DateTime VisibleStartDate { get; set; }

        public DateTime VisibleEndDate { get; set; }

        public bool OnHomePage { get; set; }

        public bool AllowCustomerReviews { get; set; }

        public string ModelNumber { get; set; }

        public bool MarkNew { get; set; }

        public bool IsDiscounted { get; set; }

        public Nullable<int> DiscountPercent { get; set; }

        public Nullable<int> Tax { get; set; }

        public bool TaxExempted { get; set; }

        public bool ShipingEnabled { get; set; }

        public Nullable<int> ShippingCharges { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public Nullable<int> ModifiedBy { get; set; }

        public Nullable<DateTime> ModifiedDate { get; set; }

        public string CreatedUser { get; set; }

        public string ModifiedUser { get; set; }

    }
}
