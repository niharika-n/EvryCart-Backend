using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIs.Data;
using WebAPIs.Models;

namespace WebAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly WebApisContext context;

        public DashboardController(WebApisContext APIcontext)
        {
            context = APIcontext;
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IResult> GetCount()
        {
            Result result = new Result();
            try
            {
                StatisticsModel statisticsModel = new StatisticsModel();
                var categoryQuery = from category in context.Categories
                                    where category.IsDeleted != true
                                    orderby category.CategoryID descending
                                    select new CategoryViewModel { Name = category.CategoryName, CreatedBy = category.CreatedBy, ID = category.CategoryID, IsActive = category.IsActive, CreatedDate = category.CreatedDate, Description = category.CategoryDescription, ModifiedBy = category.ModifiedBy, ModifiedDate = category.ModifiedDate, CreatedUser = "", ModifiedUser = "", Parent = category.ParentCategory, Child = category.ChildCategory, ImageContent = "", AssociatedProducts = 0 };
                var productQuery = from product in context.Products
                                   where product.IsDeleted != true
                                   orderby product.ProductID descending
                                   select new ProductViewModel { ProductName = product.ProductName, ProductID = product.ProductID, ShortDescription = product.ShortDescription, CategoryID = product.CategoryID, CategoryName = "", IsActive = product.IsActive, CreatedBy = product.CreatedBy, CreatedDate = product.CreatedDate, CreatedUser = "", Price = product.Price, QuantityInStock = product.QuantityInStock, VisibleEndDate = product.VisibleEndDate, AllowCustomerReviews = product.AllowCustomerReviews, DiscountPercent = product.DiscountPercent, VisibleStartDate = product.VisibleStartDate, IsDiscounted = product.IsDiscounted, LongDescription = product.LongDescription, MarkNew = product.MarkNew, ModelNumber = product.ModelNumber, ModifiedBy = product.ModifiedBy, ModifiedDate = product.ModifiedDate, ModifiedUser = "", OnHomePage = product.OnHomePage, ShipingEnabled = product.ShipingEnabled, ShippingCharges = product.ShippingCharges, Tax = product.Tax, TaxExempted = product.TaxExempted, QuantityType = product.QuantityType };
                statisticsModel.CategoryLastUpdated = context.Categories.Max(x => x.ModifiedDate > x.CreatedDate ? x.ModifiedDate : x.CreatedDate);
                statisticsModel.ProductLastUpdated = context.Products.Max(x => x.ModifiedDate > x.CreatedDate ? x.ModifiedDate : x.CreatedDate);
                statisticsModel.CategoryResult = await categoryQuery.Take(5).ToListAsync();
                statisticsModel.ProductResult = await productQuery.Take(5).ToListAsync();
                statisticsModel.CategoryCount = categoryQuery.ToList().Count;
                statisticsModel.ProductCount = productQuery.ToList().Count;
                result.Status = true;
                result.Body = statisticsModel;
                return result;
            }
            catch (Exception e)
            {
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.Body = e;

                return result;
            }
        }
    }
}