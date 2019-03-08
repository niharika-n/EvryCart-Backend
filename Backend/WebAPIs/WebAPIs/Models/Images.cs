using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPIs.Models
{
    public class Images
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [BindNever]
        public int ImageID { get; set; }
        [Required]
        [BindNever]
        public string ImageName { get; set; }
        [Required]
        [BindNever]
        public string ImageExtenstion { get; set; }
        [Required]
        public string ImageContent { get; set; }

        public int ReferenceID { get; set; }
    }
}
