using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIs.Data
{
    public class WebApisContext : DbContext
    {
        public WebApisContext(DbContextOptions<WebApisContext> options)
            : base(options)
        {
        }

        
        public DbSet<Models.ProductModel> Products { get; set; }

        public DbSet<Models.CategoryModel> Categories { get; set; }

        public DbSet<Models.UserModel> Login { get; set; }

        public DbSet<Models.Images> Images { get; set; }

        public DbSet<Models.ProductAttribute> ProductAttributes { get; set; }

        public DbSet<Models.ProductImage> ProductImage { get; set; }

        public DbSet<Models.ProductAttributeValues> ProductAttributeValues { get; set; } 

        public DbSet<Models.UserRoleModel> UserRoleTable { get; set; }

        public DbSet<Models.PasswordResetModel> PasswordResetTable { get; set; }

        public DbSet<Models.ContentModel> ContentTable { get; set; }

        public DbSet<Models.AssignedRolesModel> AssignedRolesTable { get; set; }
    }
}
