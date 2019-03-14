using System;
using System.IO;
using System.Linq;
using System.Net;
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
        public async Task<IResult> Detail(int? id)
        {
            Result result = new Result();
            try
            {
                if (id != 0)
                {
                    var detail = await context.Categories.Where(x => x.CategoryID == id && x.IsDeleted != true).FirstOrDefaultAsync();
                    if (detail != null)
                    {
                        var image = context.Images.Where(x => x.ImageID == detail.ImageID).FirstOrDefault();
                        if (image != null)
                        {
                            detail.ImageContent = image.ImageContent;
                        }
                        result.Status = true;
                        result.Body = detail;

                        return result;
                    }
                    else
                    {
                        result.Message = "Category does not exist.";
                        return result;
                    }
                }
                result.Message = "Category ID is not correct.";

                return result;
            }
            catch (Exception e)
            {
                result.Body = e;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return result;
            }
        }


        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IResult> InsertCategory()
        {
            Result result = new Result();
            try
            {
                if (!ModelState.IsValid)
                {
                    result.StatusCode = HttpStatusCode.BadRequest;
                    return result;
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
                    result.Message = "This category already exists.";
                    return result;
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
                result.Status = true;
                result.Body = new { categoryObj = category };
                return result;
            }
            catch (Exception e)
            {
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.Body = e;

                return result;
            }
        }


        [Authorize(Policy = "AdminOnly")]
        [HttpPut]
        public async Task<IResult> UpdateCategory()
        {
            Result result = new Result();
            try
            {
                if (!ModelState.IsValid)
                {
                    result.StatusCode = HttpStatusCode.BadRequest;
                    return result;
                }
                Images images = new Images();
                var file = JsonConvert.DeserializeObject<CategoryModel>(Request.Form["category"]);
                var categoryCheck = context.Categories.Where(x => x.CategoryName == file.CategoryName && x.CategoryID != file.CategoryID && x.IsDeleted != true).Any();
                if (categoryCheck)
                {
                    result.Message = "This category already exists.";
                    return result;
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
                        result.Message = "Category does not exist.";
                        return result;
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
                result.Status = true;
                result.Body = new { categoryObj = category };
                return result;
            }
            catch (Exception e)
            {
                result.Body = e;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return result;
            }
        }


        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IResult> Listing(DataHelperModel dataHelper, bool getAllParent, bool getAll)
        {
            Result result = new Result();
            try
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
                                    AssociatedProducts = categories.Where(x => x.productValueCount != null ? x.category.CategoryID == x.category.CategoryID : false).Count()
                                };
                if (getAllParent != true)
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
                    ResultModel resultModel = new ResultModel();
                    resultModel.CategoryResult = resultList;
                    resultModel.TotalCount = resultCount;
                    if (resultList.Count == 0)
                    {
                        result.Message = "No records present.";
                        return result;
                    }
                    result.Status = true;
                    result.Body = resultModel;
                    return result;
                }
                else
                {
                    if (!getAll)
                    {
                        listQuery = listQuery.Where(x => x.Child == null).OrderBy(x => x.Name);
                        var categoryList = await listQuery.ToListAsync();
                        result.Body = categoryList;
                        result.Status = true;
                        return result;
                    }
                    else
                    {
                        listQuery = listQuery.OrderBy(x => x.Name);
                        var categoryList = await listQuery.ToListAsync();
                        result.Body = categoryList;
                        result.Status = true;
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.Body = e;
                return result;
            }
        }


        [Authorize(Policy = "AdminOnly")]
        [HttpDelete]
        public async Task<IResult> Delete(int Id)
        {
            Result result = new Result();
            try
            {
                var category = await context.Categories.Where(x => x.CategoryID == Id && x.IsDeleted != true).FirstOrDefaultAsync();
                if (category == null)
                {
                    result.Message = "Category does not exist.";
                    return result;
                }
                category.IsDeleted = true;
                var deletedCategory = await context.SaveChangesAsync();
                if (deletedCategory > 0)
                {
                    var products = await context.Products.Where(x => x.CategoryID == Id && x.IsDeleted != true).ToListAsync();
                    foreach (var pdt in products)
                    {
                        pdt.IsDeleted = true;
                        var deleteCheck = await context.SaveChangesAsync();
                        if (deleteCheck > 0)
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
                result.Message = "Deleted successfully.";
                result.Status = true;

                return result;
            }
            catch (Exception e)
            {
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.Body = e;

                return result;
            }
        }


        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        public async Task<IResult> GetAssociatedProducts(int id, DataHelperModel dataHelper)
        {
            Result result = new Result();
            try
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
                        result.Message = "Products do not exist for the category.";
                        return result;
                    }
                    var list = product;
                    list = DataSort.SortBy(list, dataHelper.SortColumn, dataHelper.SortOrder);
                    var resultCount = list.Count();
                    var pagedList = DataCount.Page(list, dataHelper.PageNumber, dataHelper.PageSize);
                    var resultList = await pagedList.ToListAsync();
                    ResultModel resultModel = new ResultModel();
                    resultModel.ProductResult = resultList;
                    resultModel.TotalCount = resultCount;
                    if (resultList.Count == 0)
                    {
                        result.Message = "No records present.";
                        return result;
                    }
                    result.Status = true;
                    result.Body = resultModel;

                    return result;
                }
                result.Message = "ID entered is null.";
                return result;
            }
            catch (Exception e)
            {
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.Body = e;

                return result;
            }
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IResult> GetCategoriesForCustomer()
        {
            Result result = new Result();
            try
            {
                var categories = from category in context.Categories
                                 where category.IsDeleted != true && category.ParentCategory == true && category.IsActive == true
                                 orderby category.CreatedDate
                                 select new CategoryViewModel { Name = category.CategoryName, CreatedBy = category.CreatedBy, ID = category.CategoryID, IsActive = category.IsActive, CreatedDate = category.CreatedDate, Description = category.CategoryDescription, ModifiedBy = category.ModifiedBy, ModifiedDate = category.ModifiedDate, CreatedUser = "", ModifiedUser = "", Parent = category.ParentCategory, Child = category.ChildCategory, ImageContent = "", AssociatedProducts = 0 };
                var categoryList = await categories.ToListAsync();
                if (categoryList.Count == 0)
                {
                    result.Message = "Categories do not exist.";
                    return result;
                }
                ResultModel resultModel = new ResultModel();
                resultModel.CategoryResult = categoryList;

                result.Status = true;
                result.Body = resultModel;

                return result;
            }
            catch (Exception e)
            {
                result.Body = e;
                result.StatusCode = HttpStatusCode.BadRequest;

                return result;
            }
        }


        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IResult> GetChildCategoriesForCustomer(int id)
        {
            Result result = new Result();
            try
            {
                var childCategory = from category in context.Categories
                                    where category.ChildCategory == id && category.IsDeleted != true
                                    select new CategoryViewModel { Name = category.CategoryName, CreatedBy = category.CreatedBy, ID = category.CategoryID, IsActive = category.IsActive, CreatedDate = category.CreatedDate, Description = category.CategoryDescription, ModifiedBy = category.ModifiedBy, ModifiedDate = category.ModifiedDate, CreatedUser = "", ModifiedUser = "", Parent = category.ParentCategory, Child = category.ChildCategory, ImageContent = "", AssociatedProducts = 0 };

                await childCategory.ToListAsync();
                if (childCategory.Count() == 0)
                {
                    result.Message = "Categories do not exist.";
                    return result;
                }
                result.Status = true;
                result.Body = childCategory;

                return result;
            }
            catch (Exception e)
            {
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.Body = e;

                return result;
            }
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IResult> GetLatestCategoriesForCustomer()
        {
            Result result = new Result();
            try
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
                    result.Message = "Categories does not exist";
                    return result;
                }
                result.Body = categoryList;
                result.Status = true;
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