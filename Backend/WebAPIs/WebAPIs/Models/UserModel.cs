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
    /// <summary>
    /// Class containing parameters for users.
    /// </summary>
    public class UserModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [BindNever]
        public int UserID { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        public string EmailID { get; set; }
        
        public string Username { get; set; }
        
        public string Password { get; set; }

        public string ImageContent { get; set; }       
    }

    public class UserValidator : AbstractValidator<UserModel>
    {
        public UserValidator()
        {
            RuleFor(x => x.Username).NotNull();
            RuleFor(x => x.FirstName).NotNull();
            RuleFor(x => x.LastName).NotNull();
            RuleFor(x => x.Password).Length(5, 10).NotNull();
        }
    }
}
