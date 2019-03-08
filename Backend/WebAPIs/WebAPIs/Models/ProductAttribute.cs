using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIs.Models
{
    public class ProductAttribute
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [BindNever]
        public int AttributeID { get; set; }
        [Required]
        public string AttributeName { get; set; }
        [Required]
        [BindNever]
        public int CreatedBy { get; set; }
        [Required]
        [BindNever]
        public DateTime CreatedDate { get; set; }
        [BindNever]
        public int? ModifiedBy { get; set; }
        [BindNever]
        public DateTime? ModifiedDate { get; set; }
    }
}
