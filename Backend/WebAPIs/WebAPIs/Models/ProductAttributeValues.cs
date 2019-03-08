using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIs.Models
{
    public class ProductAttributeValues
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [BindNever]
        public int ID { get; set; }

        [ForeignKey("Attribute")]
        public int AttributeID { get; set; }

        [ForeignKey("Product")]
        public int ProductID { get; set; }

        [Required]
        public string Value { get; set; }

        public virtual ProductModel Product{get;set;}

        public virtual ProductAttribute Attribute { get; set; }
    }
}
