using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Category controller.
    /// </summary>    
    [Route("api/category")]
    [ApiController]
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


        /// <summary>
        /// Selected category detail.
        /// </summary>
        /// <param name="id">Id of caegory.</param>
        /// <returns>Detail of selected category.</returns>
        [HttpGet("detail/{id}")]
        [ProducesResponseType(typeof(CategoryViewModel), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> Detail(int? id)
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
            try
            {
                if (id != 0)
                {
                    var categoryViewModel = from category in context.Categories
                                            where category.CategoryID == id
                                            && category.IsDeleted != true
                                            select new CategoryViewModel { Name = category.CategoryName, CreatedBy = category.CreatedBy, ID = category.CategoryID, IsActive = category.IsActive, CreatedDate = category.CreatedDate, Description = category.CategoryDescription, ModifiedBy = category.ModifiedBy, ModifiedDate = category.ModifiedDate, CreatedUser = "", ModifiedUser = "", Parent = category.ParentCategory, Child = category.ChildCategory, ImageContent = null, AssociatedProducts = 0 };
                    var categoryObj = await categoryViewModel.FirstOrDefaultAsync();

                    var imageDetail = await context.Categories.Where(x => x.CategoryID == id && x.IsDeleted != true).Select(x => x.ImageID).FirstOrDefaultAsync();
                    if (categoryObj != null)
                    {
                        var image = context.Images.Where(x => x.ImageID == imageDetail).FirstOrDefault();
                        if (image != null)
                        {
                            categoryObj.ImageContent = image.ImageContent;
                        }
                        result.StatusCode = HttpStatusCode.OK;
                        result.Status = Status.Success;
                        result.Body = categoryObj;
                        return StatusCode((int)result.StatusCode, result);
                    }
                    else
                    {
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "Category does not exist.";
                        return StatusCode((int)result.StatusCode, result);
                    }
                }
                result.StatusCode = HttpStatusCode.BadRequest;
                result.Status = Status.Fail;
                result.Message = "Category ID is not correct.";
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception e)
            {
                result.Status = Status.Error;
                result.Message = e.Message;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return StatusCode((int)result.StatusCode, result);
            }
        }


        /// <summary>
        /// Insert category.
        /// </summary>
        /// <returns>
        /// Status of category interested.
        /// </returns>
        [HttpPost("insertcategory")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> InsertCategory()
        {
            var result = new Result
            {
                Operation = Operation.Create,
                Status = Status.Success
            };
            try
            {
                if (!ModelState.IsValid)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    return StatusCode((int)result.StatusCode, result);
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
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "This category already exists.";
                    return StatusCode((int)result.StatusCode, result);
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
                result.StatusCode = HttpStatusCode.OK;
                result.Status = Status.Success;
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception e)
            {
                result.Status = Status.Error;
                result.Message = e.Message;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return StatusCode((int)result.StatusCode, result);
            }
        }


        /// <summary>
        /// Update category.
        /// </summary>
        /// <returns>
        /// Status of category updated.
        /// </returns>
        [HttpPut("updatecategory")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> UpdateCategory()
        {
            var result = new Result
            {
                Operation = Operation.Update,
                Status = Status.Success
            };
            try
            {
                if (!ModelState.IsValid)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    return StatusCode((int)result.StatusCode, result);
                }
                Images images = new Images();
                var file = JsonConvert.DeserializeObject<CategoryModel>(Request.Form["category"]);
                var categoryCheck = context.Categories.Where(x => x.CategoryName == file.CategoryName && x.CategoryID != file.CategoryID && x.IsDeleted != true).Any();
                if (categoryCheck)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "This category already exists.";
                    return StatusCode((int)result.StatusCode, result);
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
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "Category does not exist.";
                        return StatusCode((int)result.StatusCode, result);
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
                result.StatusCode = HttpStatusCode.OK;
                result.Status = Status.Success;
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception e)
            {
                result.Status = Status.Error;
                result.Message = e.Message;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return StatusCode((int)result.StatusCode, result);
            }
        }


        /// <summary>
        /// Category list.
        /// </summary>
        /// <param name="dataHelper">DataHelper object of paging and sorting list.</param>
        /// <param name="getAllParent">Check to select to all parent categories.</param>
        /// <param name="getAll">Check to select all categories.</param>
        /// <returns>
        /// Returns list of category.
        /// </returns>       
        [HttpGet("listing")]
        [ProducesResponseType(typeof(List<CategoryViewModel>), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>>Listing([FromQuery] DataHelperModel dataHelper, bool getAllParent, bool getAll)
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
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
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "No records present.";
                        return StatusCode((int)result.StatusCode, result);
                    }
                    result.Status = Status.Success;
                    result.StatusCode = HttpStatusCode.OK;
                    result.Body = resultModel;
                    return StatusCode((int)result.StatusCode, result);
                }
                else
                {
                    if (!getAll)
                    {
                        listQuery = listQuery.Where(x => x.Child == null).OrderBy(x => x.Name);
                        var categoryList = await listQuery.ToListAsync();
                        result.Body = categoryList;
                        result.Status = Status.Success;
                        result.StatusCode = HttpStatusCode.OK;
                        return StatusCode((int)result.StatusCode, result);
                    }
                    else
                    {
                        listQuery = listQuery.OrderBy(x => x.Name);
                        var categoryList = await listQuery.ToListAsync();
                        result.StatusCode = HttpStatusCode.OK;
                        result.Body = categoryList;
                        result.Status = Status.Success;
                        return StatusCode((int)result.StatusCode, result);
                    }
                }
            }
            catch (Exception e)
            {
                result.Status = Status.Error;
                result.Message = e.Message;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return StatusCode((int)result.StatusCode, result);
            }
        }


        /// <summary>
        /// Deletes category.
        /// </summary>
        /// <param name="Id">Id of selected product.</param>
        /// <returns>
        /// Status with success message.
        /// </returns>
        [HttpDelete("delete")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> Delete(int Id)
        {
            var result = new Result
            {
                Operation = Operation.Delete,
                Status = Status.Success
            };
            try
            {
                var category = await context.Categories.Where(x => x.CategoryID == Id && x.IsDeleted != true).FirstOrDefaultAsync();
                if (category == null)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Category does not exist.";
                    return StatusCode((int)result.StatusCode, result);
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
                result.Status = Status.Success;
                result.StatusCode = HttpStatusCode.OK;
                result.Message = "Deleted successfully.";
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception e)
            {
                result.Status = Status.Error;
                result.Message = e.Message;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return StatusCode((int)result.StatusCode, result);
            }
        }


        /// <summary>
        /// Product list.
        /// </summary>
        /// <param name="id">Id of selected category.</param>
        /// <param name="dataHelper">Datahelper object for paging and sorting list.</param>
        /// <returns>
        /// Returns list of products for selected category.
        /// </returns>
        [HttpGet("getassociatedproducts/{id}")]
        [ProducesResponseType(typeof(List<ProductViewModel>), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> GetAssociatedProducts( int id, [FromQuery] DataHelperModel dataHelper)
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
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
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "Products do not exist for the category.";
                        return StatusCode((int)result.StatusCode, result);
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
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "No records present.";
                        return StatusCode((int)result.StatusCode, result);
                    }

                    result.Status = Status.Success;
                    result.StatusCode = HttpStatusCode.OK;
                    result.Body = resultModel;
                    return StatusCode((int)result.StatusCode, result);
                }

                result.Status = Status.Fail;
                result.StatusCode = HttpStatusCode.BadRequest;
                result.Message = "ID entered is null.";
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception e)
            {
                result.Status = Status.Error;
                result.Message = e.Message;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return StatusCode((int)result.StatusCode, result);
            }
        }


        /// <summary>
        /// Category list.
        /// </summary>
        /// <returns>
        /// Returns list of categories for home page.
        /// </returns>
        [HttpGet("getcategoriesforcustomer")]
        [ProducesResponseType(typeof(List<CategoryViewModel>), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<IResult>> GetCategoriesForCustomer()
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
            try
            {
                var categories = from category in context.Categories
                                 where category.IsDeleted != true && category.ParentCategory == true && category.IsActive == true
                                 orderby category.CreatedDate
                                 select new CategoryViewModel { Name = category.CategoryName, CreatedBy = category.CreatedBy, ID = category.CategoryID, IsActive = category.IsActive, CreatedDate = category.CreatedDate, Description = category.CategoryDescription, ModifiedBy = category.ModifiedBy, ModifiedDate = category.ModifiedDate, CreatedUser = "", ModifiedUser = "", Parent = category.ParentCategory, Child = category.ChildCategory, ImageContent = "", AssociatedProducts = 0 };
                var categoryList = await categories.ToListAsync();
                if (categoryList.Count == 0)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Categories do not exist.";
                    return StatusCode((int)result.StatusCode, result);
                }

                result.Status = Status.Success;
                result.StatusCode = HttpStatusCode.OK;
                result.Body = categoryList;
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception e)
            {
                result.Status = Status.Error;
                result.Message = e.Message;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return StatusCode((int)result.StatusCode, result);
            }
        }


        /// <summary>
        /// Cateogry list.
        /// </summary>
        /// <param name="id">Id of category.</param>
        /// <returns>
        /// Returns list of child categories for selected category.
        /// </returns>
        [HttpGet("getchildcategoriesforcustomer/{id}")]
        [ProducesResponseType(typeof(List<CategoryViewModel>), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<IResult>> GetChildCategoriesForCustomer(int id)
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
            try
            {
                var childCategory = from category in context.Categories
                                    where category.ChildCategory == id && category.IsDeleted != true
                                    select new CategoryViewModel { Name = category.CategoryName, CreatedBy = category.CreatedBy, ID = category.CategoryID, IsActive = category.IsActive, CreatedDate = category.CreatedDate, Description = category.CategoryDescription, ModifiedBy = category.ModifiedBy, ModifiedDate = category.ModifiedDate, CreatedUser = "", ModifiedUser = "", Parent = category.ParentCategory, Child = category.ChildCategory, ImageContent = "", AssociatedProducts = 0 };

                var list = await childCategory.ToListAsync();
                if (childCategory.Count() == 0)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Categories do not exist.";
                    return StatusCode((int)result.StatusCode, result);
                }

                result.Status = Status.Success;
                result.StatusCode = HttpStatusCode.OK;
                result.Body = list;
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception e)
            {
                result.Status = Status.Error;
                result.Message = e.Message;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return StatusCode((int)result.StatusCode, result);
            }
        }


        /// <summary>
        /// Category list.
        /// </summary>
        /// <returns>
        /// Returns latest categories for home page.
        /// </returns>
        [HttpGet("getlatestcategoriesforcustomer")]
        [ProducesResponseType(typeof(List<CategoryViewModel>), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<IResult>> GetLatestCategoriesForCustomer()
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
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
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Categories does not exist";
                    return result;
                }
                result.Status = Status.Success;
                result.StatusCode = HttpStatusCode.OK;
                result.Body = categoryList;
                result.Status = Status.Success;
                return result;
            }
            catch (Exception e)
            {
                result.Status = Status.Error;
                result.Message = e.Message;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return StatusCode((int)result.StatusCode, result);
            }
        }
    }
}