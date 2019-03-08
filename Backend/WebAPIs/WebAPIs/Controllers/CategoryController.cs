using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebAPIs.Data;
using WebAPIs.Models;

namespace WebAPIs.Controllers
{
    [Route("api/category/[action]")]
    public class CategoryController : Controller
    {
        private WebApisContext context;
        private readonly ClaimsPrincipal principal;
        Helper helper;

        public CategoryController(WebApisContext APIcontext, IPrincipal _principal)
        {
            context = APIcontext;
            principal = _principal as ClaimsPrincipal;
            helper = new Helper(_principal);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id != 0)
            {
                var detail = await context.Categories.Where(x => x.CategoryID == id && x.IsDeleted != true).FirstOrDefaultAsync();
                var image = context.Images.Where(x => x.ImageID == detail.ImageID).FirstOrDefault();
                if (detail != null)
                {
                    if (image != null)
                    {
                        detail.ImageContent = image.ImageContent;
                    }
                    return Ok(detail);
                }
                else
                {
                    return NotFound("Category does not exist.");
                }
            }
            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> InsertCategory()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var categoryFile = JsonConvert.DeserializeObject<CategoryModel>(Request.Form["category"]);
            CategoryModel category = new CategoryModel();
            Images images = new Images();
            if (Request.Form.Files.Count != 0)
            {
                IFormFile img = null;
                var image = Request.Form.Files;
                foreach (var i in image)
                {
                    img = image[0];
                }
                ImageService imageService = new ImageService();
                images.ImageName = img.FileName;
                images.ImageContent = imageService.Image(img);
                images.ImageExtenstion = Path.GetExtension(img.FileName);
                context.Images.Add(images);
                await context.SaveChangesAsync();
                category.ImageID = images.ImageID;
                category.ImageContent = images.ImageContent;
            }
            var categoryCheck = context.Categories.Where(x => x.CategoryName == categoryFile.CategoryName).ToList();
            if (categoryCheck.Count() != 0)
            {
                return Ok(new { message = "This category already exists" });
            }
            category.CategoryID = categoryFile.CategoryID;
            category.CategoryName = categoryFile.CategoryName;
            category.CategoryDescription = categoryFile.CategoryDescription;
            category.IsActive = categoryFile.IsActive;
            category.CreatedBy = helper.GetSpecificClaim("ID");
            category.CreatedDate = DateTime.Now;
            category.ParentCategory = categoryFile.ParentCategory;
            if (!category.ParentCategory)
            {
                category.ChildCategory = categoryFile.ChildCategory;
            }

            context.Categories.Add(category);
            var status = await context.SaveChangesAsync();
            CategoryViewModel viewModel = new CategoryViewModel()
            {
                CreatedUser = helper.GetSpecificClaim("Name")
            };
            return Ok(new { categoryObj = category });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut]
        public async Task<IActionResult> UpdateCategory()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Images images = new Images();
            var file = JsonConvert.DeserializeObject<CategoryModel>(Request.Form["category"]);
            var categoryCheck = context.Categories.Where(x => x.CategoryName == file.CategoryName && x.CategoryID != file.CategoryID && x.IsDeleted != true).Any();
            if (categoryCheck)
            {
                return Ok(new { message = "This category already exists" });
            }
            var category = context.Categories.Where(x => x.CategoryID == file.CategoryID && x.IsDeleted != true).FirstOrDefault();
            category.CategoryID = file.CategoryID;
            category.CategoryName = file.CategoryName;
            category.CategoryDescription = file.CategoryDescription;
            category.IsActive = file.IsActive;
            category.ModifiedBy = helper.GetSpecificClaim("ID");
            category.ModifiedDate = DateTime.Now;
            category.ParentCategory = file.ParentCategory;
            if (file.ImageID == null)
            {
                if (category.ImageID != null)
                {
                    var oldImage = await context.Images.Where(x => x.ImageID == category.ImageID).FirstOrDefaultAsync();
                    context.Images.Remove(oldImage);
                    await context.SaveChangesAsync();
                    category.ImageID = null;
                }
            }
            if (!category.ParentCategory)
            {
                category.ChildCategory = file.ChildCategory;
            }
            else
            {
                category.ChildCategory = null;
            }
            if (Request.Form.Files.Count != 0)
            {
                ImageService imageService = new ImageService();
                IFormFile img = null;
                if (category == null)
                {
                    return Ok(new { message = "Category does not exist." });
                }
                var image = Request.Form.Files;
                img = image[0];
                images.ImageName = img.FileName;
                images.ImageContent = imageService.Image(img);
                images.ImageExtenstion = Path.GetExtension(img.FileName);
                if (category.ImageID == null)
                {
                    context.Images.Add(images);
                    await context.SaveChangesAsync();
                    category.ImageID = images.ImageID;
                    category.ImageContent = images.ImageContent;
                }
                var categoryImage = await context.Images.Where(x => x.ImageID == category.ImageID).FirstOrDefaultAsync();
                categoryImage.ImageContent = images.ImageContent;
                categoryImage.ImageExtenstion = images.ImageExtenstion;
                categoryImage.ImageName = images.ImageName;
            }
            var status = await context.SaveChangesAsync();
            CategoryViewModel localmodel = new CategoryViewModel()
            {
                ModifiedUser = helper.GetSpecificClaim("Name")
            };
            return Ok(new { categoryObj = category });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Listing(DataHelperModel dataHelper, bool getAllParent, bool getAll)
        {
            var listQuery = from category in context.Categories
                            join createdUser in context.Login
                            on category.CreatedBy equals createdUser.UserID
                            into createdUserName
                            from createdUser in createdUserName.DefaultIfEmpty()
                            let createdByUser = createdUser.Username
                            join modifiedUser in context.Login
                            on category.ModifiedBy equals modifiedUser.UserID
                            into modifiedUserName
                            from modifiedUser in modifiedUserName.DefaultIfEmpty()
                            let modifiedByUser = modifiedUser.Username
                            join products in context.Products
                            on category.CategoryID equals products.CategoryID
                            into productCount
                            from productValueCount in productCount.DefaultIfEmpty()
                            where category.IsDeleted != true
                            orderby category.CreatedDate descending
                            group new { category, productValueCount, createdByUser, modifiedByUser } by
                            new { category, createdByUser, modifiedByUser } into categories                           
                            select new CategoryViewModel
                            {
                                Name = categories.Key.category.CategoryName,
                                CreatedBy = categories.Key.category.CreatedBy,
                                ID = categories.Key.category.CategoryID,
                                IsActive = categories.Key.category.IsActive,
                                CreatedDate = categories.Key.category.CreatedDate,
                                Description = categories.Key.category.CategoryDescription,
                                ModifiedBy = categories.Key.category.ModifiedBy,
                                ModifiedDate = categories.Key.category.ModifiedDate,
                                CreatedUser = categories.Key.createdByUser,
                                ModifiedUser = categories.Key.modifiedByUser,
                                Parent = categories.Key.category.ParentCategory,
                                Child = categories.Key.category.ChildCategory,
                                ImageContent = "",
                                AssociatedProducts = categories.Where(x => x.productValueCount != null ? x.category.CategoryID == x.category.CategoryID: false).Count()                                
                            };
            if (getAllParent == false)
            {
                if (dataHelper.Search != null)
                {
                    listQuery = listQuery.Where(x => x.Name.Contains(dataHelper.Search) || x.Description.Contains(dataHelper.Search));
                }
                var list = listQuery;
                list = DataSort.SortBy(list, dataHelper.SortColumn, dataHelper.SortOrder);
                var resultCount = list.Count();
                var pagedList = DataCount.Page(list, dataHelper.PageNumber, dataHelper.PageSize);
                var resultList = await pagedList.ToListAsync();
                ResultModel result = new ResultModel();
                result.CategoryResult = resultList;
                result.TotalCount = resultCount;
                if (resultList.Count == 0)
                {
                    return NotFound("No records present.");
                }
                return Ok(result);
            }
            else
            {
                if (!getAll)
                {
                    listQuery = listQuery.Where(x => x.Child == null).OrderBy(x => x.Name);
                    var categoryList = await listQuery.ToListAsync();
                    return Ok(categoryList);
                }
                else
                {
                    listQuery = listQuery.OrderBy(x => x.Name);
                    var categoryList = await listQuery.ToListAsync();
                    return Ok(categoryList);
                }
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            var category = await context.Categories.Where(x => x.CategoryID == Id && x.IsDeleted != true).FirstOrDefaultAsync();
            if (category == null)
            {
                return NotFound("Category does not exist.");
            }
            category.IsDeleted = true;
            var deletedCategory = await context.SaveChangesAsync();
            if (deletedCategory > 0)
            {
                var products = await context.Products.Where(x => x.CategoryID == Id && x.IsDeleted != true).ToListAsync();
                foreach (var pdt in products)
                {
                    pdt.IsDeleted = true;
                    var result = await context.SaveChangesAsync();
                    if (result > 0)
                    {
                        var productImages = await context.ProductImage.Where(x => x.ProductID == pdt.ProductID).ToListAsync();
                        if (productImages != null)
                        {
                            context.ProductImage.RemoveRange(productImages);
                        }
                        var productAttributeValues = await context.ProductAttributeValues.Where(x => x.ProductID == pdt.ProductID).ToListAsync();
                        if (productAttributeValues != null)
                        {
                            context.ProductAttributeValues.RemoveRange(productAttributeValues);
                        }
                        await context.SaveChangesAsync();
                    }
                }                
            }
            return Ok("Deleted successfully.");
        } 

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssociatedProducts(int id, DataHelperModel dataHelper)
        {
            if (id != 0)
            {
                var product = from products in context.Products
                              where products.CategoryID == id
&& products.IsDeleted != true
                              orderby products.ProductName
                              select new ProductViewModel { ProductName = products.ProductName, ProductID = products.ProductID, ShortDescription = products.ShortDescription, CategoryID = products.CategoryID, CategoryName = "", IsActive = products.IsActive, CreatedBy = products.CreatedBy, CreatedDate = products.CreatedDate, CreatedUser = "", Price = products.Price, QuantityInStock = products.QuantityInStock, VisibleEndDate = products.VisibleEndDate, AllowCustomerReviews = products.AllowCustomerReviews, DiscountPercent = products.DiscountPercent, VisibleStartDate = products.VisibleStartDate, IsDiscounted = products.IsDiscounted, LongDescription = products.LongDescription, MarkNew = products.MarkNew, ModelNumber = products.ModelNumber, ModifiedBy = products.ModifiedBy, ModifiedDate = products.ModifiedDate, ModifiedUser = "", OnHomePage = products.OnHomePage, ShipingEnabled = products.ShipingEnabled, ShippingCharges = products.ShippingCharges, Tax = products.Tax, TaxExempted = products.TaxExempted, QuantityType = products.QuantityType };
                if (product.Count() == 0)
                {
                    return NotFound("Products do not exist for the category.");
                }
                var list = product;
                list = DataSort.SortBy(list, dataHelper.SortColumn, dataHelper.SortOrder);
                var resultCount = list.Count();
                var pagedList = DataCount.Page(list, dataHelper.PageNumber, dataHelper.PageSize);
                var resultList = await pagedList.ToListAsync();
                ResultModel result = new ResultModel();
                result.ProductResult = resultList;
                result.TotalCount = resultCount;
                if (resultList.Count == 0)
                {
                    return NotFound("No records present.");
                }
                return Ok(result);
            }
            return Ok("ID entered is null.");
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetCategoriesForCustomer()
        {
            var categories = from category in context.Categories
                             where category.IsDeleted != true && category.ParentCategory == true && category.IsActive == true
                             orderby category.CreatedDate
                             select new CategoryViewModel { Name = category.CategoryName, CreatedBy = category.CreatedBy, ID = category.CategoryID, IsActive = category.IsActive, CreatedDate = category.CreatedDate, Description = category.CategoryDescription, ModifiedBy = category.ModifiedBy, ModifiedDate = category.ModifiedDate, CreatedUser = "", ModifiedUser = "", Parent = category.ParentCategory, Child = category.ChildCategory, ImageContent = "", AssociatedProducts = 0 };
            var categoryList = await categories.ToListAsync();
            if (categoryList.Count == 0)
            {
                return NotFound("Categories do not exist.");
            }
            ResultModel result = new ResultModel();
            result.CategoryResult = categoryList;
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChildCategoriesForCustomer(int id)
        {
            var childCategory = from category in context.Categories
                                where category.ChildCategory == id && category.IsDeleted != true
                                select new CategoryViewModel { Name = category.CategoryName, CreatedBy = category.CreatedBy, ID = category.CategoryID, IsActive = category.IsActive, CreatedDate = category.CreatedDate, Description = category.CategoryDescription, ModifiedBy = category.ModifiedBy, ModifiedDate = category.ModifiedDate, CreatedUser = "", ModifiedUser = "", Parent = category.ParentCategory, Child = category.ChildCategory, ImageContent = "", AssociatedProducts = 0 };

            await childCategory.ToListAsync();
            if (childCategory.Count() == 0)
            {
                return NoContent();
            }
            return Ok(childCategory);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetLatestCategoriesForCustomer()
        {
            var categories = from category in context.Categories
                             join image in context.Images
                             on category.ImageID equals image.ImageID
                             into categoryDetail
                             from image in categoryDetail.DefaultIfEmpty()
                             orderby category.CreatedDate descending
                             where category.IsActive == true && category.IsDeleted != true && category.ParentCategory == true
                             select new CategoryViewModel { Name = category.CategoryName, CreatedBy = category.CreatedBy, ID = category.CategoryID, IsActive = category.IsActive, CreatedDate = category.CreatedDate, Description = category.CategoryDescription, ModifiedBy = category.ModifiedBy, ModifiedDate = category.ModifiedDate, CreatedUser = "", ModifiedUser = "", Parent = category.ParentCategory, Child = category.ChildCategory, ImageContent = image.ImageContent, AssociatedProducts = 0 };
            var categoryList = await categories.Take(3).ToListAsync();
            if (categoryList.Count == 0)
            {
                return NotFound("Categories does not exist");
            }
            return Ok(categoryList);
        }
    }
}