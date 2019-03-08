using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIs.Models
{
    public class CategoryModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [BindNever]
        public int CategoryID { get; set; }
        [Required]
        public string CategoryName { get; set; }
        [Required]
        public string CategoryDescription { get; set; }
        [Required]
        public bool IsActive { get; set; }
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
        [Required]
        [NotMapped]
        public string ImageContent { get; set; }

        public int? ImageID { get; set; }
        [Required]
        public bool ParentCategory { get; set; }

        public int? ChildCategory { get; set; }
        [BindNever]
        public bool IsDeleted { get; set; } = false;
    }
    public class CategoryValidator : AbstractValidator<CategoryModel>
    {
        public CategoryValidator()
        {
            RuleFor(x => x.CategoryName).NotNull();
            RuleFor(x => x.CategoryDescription).NotNull();
            RuleFor(x => x.IsActive).NotNull();
        }
    }

}
