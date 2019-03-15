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
    /// Product Controller.
    /// </summary>
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private WebApisContext context;
        private readonly ClaimsPrincipal principal;
        Helper helper;

        public ProductController(WebApisContext _context, IPrincipal _principal)
        {
            context = _context;
            principal = _principal as ClaimsPrincipal;
            helper = new Helper(_principal);
        }

        /// <summary>
        /// Returns the details of product.
        /// </summary>
        /// <param name="id">Id of the product.</param>
        /// <returns>
        /// Details of the selected product.
        /// </returns>
        [HttpGet("getdetail/{id}")]
        [ProducesResponseType(typeof(ProductViewModel), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> GetDetail(int? id)
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
                    var productDetail = from product in context.Products
                                        join createdUser in context.Login
                                        on product.CreatedBy equals createdUser.UserID
                                        into createname
                                        from createUserName in createname.DefaultIfEmpty()
                                        join modifiedUser in context.Login
                                        on product.ModifiedBy equals modifiedUser.UserID
                                        into modifyname
                                        from modifyUserName in modifyname.DefaultIfEmpty()
                                        join categoryname in context.Categories
                                        on product.CategoryID equals categoryname.CategoryID
                                        into namecategory
                                        from categoryName in namecategory.DefaultIfEmpty()
                                        where product.IsDeleted != true && product.ProductID == id
                                        orderby product.CreatedDate descending
                                        select new ProductViewModel { ProductName = product.ProductName, ProductID = product.ProductID, ShortDescription = product.ShortDescription, CategoryID = product.CategoryID, CategoryName = categoryName.CategoryName, IsActive = product.IsActive, CreatedBy = product.CreatedBy, CreatedDate = product.CreatedDate, CreatedUser = createUserName.Username, Price = product.Price, QuantityInStock = product.QuantityInStock, VisibleEndDate = product.VisibleEndDate, AllowCustomerReviews = product.AllowCustomerReviews, DiscountPercent = product.DiscountPercent, VisibleStartDate = product.VisibleStartDate, IsDiscounted = product.IsDiscounted, LongDescription = product.LongDescription, MarkNew = product.MarkNew, ModelNumber = product.ModelNumber, ModifiedBy = product.ModifiedBy, ModifiedDate = product.ModifiedDate, ModifiedUser = modifyUserName.Username, OnHomePage = product.OnHomePage, ShipingEnabled = product.ShipingEnabled, ShippingCharges = product.ShippingCharges, Tax = product.Tax, TaxExempted = product.TaxExempted, QuantityType = product.QuantityType };
                    var productObj = await productDetail.FirstOrDefaultAsync();
                    if (productDetail.ToList().Count() != 0)
                    {
                        result.Status = Status.Success;
                        result.StatusCode = HttpStatusCode.OK;                        
                        result.Body = productObj;
                        return StatusCode((int)result.StatusCode, result);
                    }
                    else
                    {
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "Product does not exist.";
                        return StatusCode((int)result.StatusCode, result);
                    }
                }
                result.Status = Status.Fail;
                result.StatusCode = HttpStatusCode.BadRequest;
                result.Message = "Product ID is not valid.";
                return StatusCode((int)result.StatusCode, result);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.Status = Status.Error;
                return StatusCode((int)result.StatusCode, result);
            }
        }


        /// <summary>
        /// Inserts product.
        /// </summary>
        /// <param name="product">Object of ProductModel.</param>
        /// <returns>
        /// Value of product added.
        /// </returns>
        [HttpPost("insertproduct")]
        [ProducesResponseType(typeof(ProductViewModel), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> InsertProduct([FromBody]ProductModel product)
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
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Status = Status.Fail;
                    return StatusCode((int)result.StatusCode, result); 
                }
                var productCheckQuery = context.Products.Where(x => (x.ProductName == product.ProductName && x.CategoryID == product.CategoryID) || (x.ModelNumber == product.ModelNumber));
                if (productCheckQuery.Count() != 0)
                {
                    var productCheck = await productCheckQuery.ToArrayAsync();
                    foreach (var pdt in productCheck)
                    {
                        if (pdt.ProductName == product.ProductName && pdt.CategoryID == product.CategoryID)
                        {
                            result.StatusCode = HttpStatusCode.BadRequest;
                            result.Status = Status.Fail;
                            result.Message = "SameName";
                            return StatusCode((int)result.StatusCode, result);
                        }
                        else if (pdt.ModelNumber == product.ModelNumber)
                        {
                            result.StatusCode = HttpStatusCode.BadRequest;
                            result.Status = Status.Fail;
                            result.Message = "SameModel";
                            return StatusCode((int)result.StatusCode, result);
                        }
                    }
                }
                product.CreatedDate = DateTime.Now;
                product.CreatedBy = helper.GetSpecificClaim("ID");

                context.Products.Add(product);
                await context.SaveChangesAsync();
                ProductViewModel localmodel = new ProductViewModel()
                {
                    CreatedUser = helper.GetSpecificClaim("Name")
                };

                result.Status = Status.Success;
                result.StatusCode = HttpStatusCode.OK;
                result.Body = product;               
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
        /// Updates product details.
        /// </summary>
        /// <param name="product">Object of product model.</param>
        /// <returns>
        /// Value of product updated.
        /// </returns>
        [HttpPut("updateproduct")]
        [ProducesResponseType(typeof(ProductViewModel), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> UpdateProduct([FromBody] ProductModel product)
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
                var productObj = context.Products.Where(x => x.ProductID == product.ProductID && x.IsDeleted != true).SingleOrDefault();
                if (productObj == null)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Product does not exist";
                    return StatusCode((int)result.StatusCode, result); 
                }
                var productCheckQuery = context.Products.Where(x => ((x.ProductName == product.ProductName && x.CategoryID == product.CategoryID) || (x.ModelNumber == product.ModelNumber)) && (x.ProductID != product.ProductID));
                if (productCheckQuery.Count() != 0)
                {
                    var productCheck = await productCheckQuery.ToArrayAsync();
                    foreach (var pdt in productCheck)
                    {
                        if (pdt.ProductName == product.ProductName && pdt.CategoryID == product.CategoryID)
                        {
                            result.StatusCode = HttpStatusCode.BadRequest;
                            result.Status = Status.Fail;
                            result.Message = "SameName";
                            return StatusCode((int)result.StatusCode, result);
                        }
                        else if (pdt.ModelNumber == product.ModelNumber)
                        {
                            result.StatusCode = HttpStatusCode.BadRequest;
                            result.Status = Status.Fail;
                            result.Message = "SameModel";
                            return StatusCode((int)result.StatusCode, result);
                        }
                    }
                }
                productObj.ProductID = product.ProductID;
                productObj.ProductName = product.ProductName;
                productObj.ShortDescription = product.ShortDescription;
                productObj.LongDescription = product.LongDescription;
                productObj.CategoryID = product.CategoryID;
                productObj.AllowCustomerReviews = product.AllowCustomerReviews;
                productObj.DiscountPercent = product.DiscountPercent;
                productObj.IsActive = product.IsActive;
                productObj.IsDiscounted = product.IsDiscounted;
                productObj.MarkNew = product.MarkNew;
                productObj.ModelNumber = product.ModelNumber;
                productObj.OnHomePage = product.OnHomePage;
                productObj.Price = product.Price;
                productObj.QuantityInStock = product.QuantityInStock;
                productObj.QuantityType = product.QuantityType;
                productObj.ShipingEnabled = product.ShipingEnabled;
                productObj.ShippingCharges = product.ShippingCharges;
                productObj.Tax = product.Tax;
                productObj.TaxExempted = product.TaxExempted;
                productObj.VisibleEndDate = product.VisibleEndDate;
                productObj.VisibleStartDate = product.VisibleStartDate;
                productObj.ModifiedBy = helper.GetSpecificClaim("ID");
                productObj.ModifiedDate = DateTime.Now;
                await context.SaveChangesAsync();
                ProductViewModel localmodel = new ProductViewModel()
                {
                    ModifiedUser = helper.GetSpecificClaim("Name")
                };
                result.StatusCode = HttpStatusCode.OK;
                result.Status = Status.Success;
                result.Body = productObj;
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
        /// List of products.
        /// </summary>
        /// <param name="dataHelper">DataHelper object of paging and sorting list.</param>
        /// <returns>
        /// Paged and sorted list of prodcuts.
        /// </returns>
        [HttpGet("listing")]
        [ProducesResponseType(typeof(List<ProductViewModel>), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> Listing(DataHelperModel dataHelper)
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
            try
            {
                var listQuery = from product in context.Products
                                join createdUser in context.Login
                                on product.CreatedBy equals createdUser.UserID
                                into createname
                                from createUserName in createname.DefaultIfEmpty()
                                join modifiedUser in context.Login
                                on product.ModifiedBy equals modifiedUser.UserID
                                into modifyname
                                from modifyUserName in modifyname.DefaultIfEmpty()
                                join categoryname in context.Categories
                                on product.CategoryID equals categoryname.CategoryID
                                into namecategory
                                from categoryName in namecategory.DefaultIfEmpty()
                                where product.IsDeleted != true
                                orderby product.CreatedDate descending
                                select new ProductViewModel { ProductName = product.ProductName, ProductID = product.ProductID, ShortDescription = product.ShortDescription, CategoryID = product.CategoryID, CategoryName = categoryName.CategoryName, IsActive = product.IsActive, CreatedBy = product.CreatedBy, CreatedDate = product.CreatedDate, CreatedUser = createUserName.Username, Price = product.Price, QuantityInStock = product.QuantityInStock, VisibleEndDate = product.VisibleEndDate, AllowCustomerReviews = product.AllowCustomerReviews, DiscountPercent = product.DiscountPercent, VisibleStartDate = product.VisibleStartDate, IsDiscounted = product.IsDiscounted, LongDescription = product.LongDescription, MarkNew = product.MarkNew, ModelNumber = product.ModelNumber, ModifiedBy = product.ModifiedBy, ModifiedDate = product.ModifiedDate, ModifiedUser = modifyUserName.Username, OnHomePage = product.OnHomePage, ShipingEnabled = product.ShipingEnabled, ShippingCharges = product.ShippingCharges, Tax = product.Tax, TaxExempted = product.TaxExempted, QuantityType = product.QuantityType };
                if (dataHelper.Search != null)
                {
                    listQuery = listQuery.Where(x => x.ProductName.Contains(dataHelper.Search) || x.ShortDescription.Contains(dataHelper.Search) || x.LongDescription.Contains(dataHelper.Search));
                }
                var list = listQuery;
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
            catch (Exception e)
            {
                result.Status = Status.Error;
                result.Message = e.Message;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return StatusCode((int)result.StatusCode, result);
            }
        }


        /// <summary>
        /// Deletes the product.
        /// </summary>
        /// <param name="ID">Id of selected product.</param>
        /// <returns>
        /// Status code with success message.
        /// </returns>
        [HttpDelete("delete")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> Delete(int ID)
        {
            var result = new Result
            {
                Operation = Operation.Delete,
                Status = Status.Success
            };
            try
            {
                var deleteQuery = await context.Products.Where(x => x.ProductID == ID && x.IsDeleted != true).FirstOrDefaultAsync();
                if (deleteQuery == null)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Product does not exist.";
                    return StatusCode((int)result.StatusCode, result);
                }
                deleteQuery.IsDeleted = true;
                var deleteCheck = await context.SaveChangesAsync();
                if (deleteCheck > 0)
                {
                    var productImages = await context.ProductImage.Where(x => x.ProductID == ID).ToListAsync();
                    if (productImages != null)
                    {
                        context.ProductImage.RemoveRange(productImages);
                    }
                    var productAttributeValues = await context.ProductAttributeValues.Where(x => x.ProductID == ID).ToListAsync();
                    if (productAttributeValues != null)
                    {
                        context.ProductAttributeValues.RemoveRange(productAttributeValues);
                    }
                    await context.SaveChangesAsync();
                }

                result.StatusCode = HttpStatusCode.OK;
                result.Status = Status.Success;
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
        /// Images of product.
        /// </summary>
        /// <param name="id">id of product.</param>
        /// <returns>
        /// Images of selected product.
        /// </returns>
        [HttpGet("getproductimages/{id}")]
        [ProducesResponseType(typeof(List<ProductImageViewModel>), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> GetProductImages(int id)
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
            try
            {
                var pdtImage = from productImage in context.ProductImage
                               where productImage.ProductID == id
                               orderby productImage.ID descending
                               select new ProductImageViewModel { ID = productImage.ID, ImageContent = productImage.ImageContent };
                if (pdtImage.Count() == 0)
                {
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Status = Status.Fail;
                    result.Message = "Product images do not exist.";
                    return StatusCode((int)result.StatusCode, result);
                }
                var list = await pdtImage.ToListAsync();
                if (list.Count == 0)
                {
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Status = Status.Fail;
                    result.Message = "No records present.";
                    return StatusCode((int)result.StatusCode, result); 
                }
                ResultModel resultModel = new ResultModel();
                resultModel.ProductImageResult = list;

                result.Status = Status.Success;
                result.StatusCode = HttpStatusCode.OK;
                result.Body = resultModel;

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
        /// Add images.
        /// </summary>
        /// <returns>
        /// Status for success.
        /// </returns>
        [HttpPost("addproductimages")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> AddProductImages()
        {
            var result = new Result
            {
                Operation = Operation.Create,
                Status = Status.Success
            };
            try
            {
                var id = 0;
                var productID = Request.Form["productID"];
                var product = JsonConvert.DeserializeAnonymousType(productID, id);
                if (Request.Form.Files.Count != 0)
                {
                    var images = Request.Form.Files;
                    ImageService imageService = new ImageService();
                    foreach (IFormFile i in images)
                    {
                        ProductImage productimage = new ProductImage();
                        productimage.ImageName = i.FileName;
                        productimage.ImageContent = imageService.Image(i);
                        productimage.ProductID = product;
                        productimage.ImageExtenstion = Path.GetExtension(i.FileName);
                        context.ProductImage.Add(productimage);
                    }
                    await context.SaveChangesAsync();
                }
                result.StatusCode = HttpStatusCode.OK;
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


        /// <summary>
        /// Deletes selected image for a product.
        /// </summary>
        /// <param name="pdtID">Id of product.</param>
        /// <param name="imageID">Id of image.</param>
        /// <returns>
        /// Status with message.
        /// </returns>
        [HttpDelete("deleteproductimage")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> DeleteProductImage(int pdtID, int imageID)
        {
            var result = new Result
            {
                Operation = Operation.Delete,
                Status = Status.Success
            };
            try
            {
                var image = await context.ProductImage.Where(x => x.ProductID == pdtID && x.ID == imageID).SingleOrDefaultAsync();
                if (image == null)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Image does not exist.";
                    return StatusCode((int)result.StatusCode, result);
                }
                context.ProductImage.Remove(image);
                await context.SaveChangesAsync();

                result.StatusCode = HttpStatusCode.OK;
                result.Status = Status.Success;
                result.Message = "Image deleted";

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
        /// Inserts new attribute.
        /// </summary>
        /// <param name="attributeValue">Object of ProductAttributeValue</param>
        /// <returns>
        /// Details of new attribute added.
        /// </returns>
        [HttpPost("insertproductattributevalue")]
        [ProducesResponseType(typeof(ProductAttributeValues), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> InsertProductAttributeValue([FromBody] ProductAttributeValues attributeValue)
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
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Status = Status.Fail;
                    return StatusCode((int)result.StatusCode, result);
                }
                var attributeValueCheck = await context.ProductAttributeValues.Where(x => x.Value == attributeValue.Value && x.ProductID == attributeValue.ProductID).ToListAsync();
                if (attributeValueCheck.Count() != 0)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Product value exists already";
                    return StatusCode((int)result.StatusCode, result);
                }
                context.ProductAttributeValues.Add(attributeValue);
                await context.SaveChangesAsync();
                var product = await context.Products.Where(x => x.ProductID == attributeValue.ProductID).FirstOrDefaultAsync();
                product.ModifiedBy = helper.GetSpecificClaim("ID");
                product.ModifiedDate = DateTime.Now;
                ProductViewModel viewModel = new ProductViewModel()
                {
                    ModifiedUser = helper.GetSpecificClaim("Name")
                };
                await context.SaveChangesAsync();

                result.StatusCode = HttpStatusCode.OK;
                result.Status = Status.Success;
                result.Body = attributeValue;
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
        /// Updates the attribute value.
        /// </summary>
        /// <param name="attributeValue">Object of ProductAttributeValue.</param>
        /// <returns>
        /// Details of updated attribute.
        /// </returns>
        [HttpPut("updateproductattributevalue")]
        [ProducesResponseType(typeof(ProductAttributeValues), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> UpdateProductAttributeValue([FromBody] ProductAttributeValues attributeValue)
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
                var attribute = await context.ProductAttributeValues.Where(x => x.ID == attributeValue.ID).FirstOrDefaultAsync();
                var attributeValueCheck = await context.ProductAttributeValues.Where(x => x.Value == attributeValue.Value).ToListAsync();
                if (attributeValueCheck.Count() != 0)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Product value exists already.";
                    return StatusCode((int)result.StatusCode, result);
                }
                attribute.ID = attributeValue.ID;
                attribute.AttributeID = attributeValue.AttributeID;
                attribute.ProductID = attributeValue.ProductID;
                attribute.Value = attributeValue.Value;
                await context.SaveChangesAsync();

                var product = await context.Products.Where(x => x.ProductID == attributeValue.ProductID).FirstOrDefaultAsync();
                product.ModifiedBy = helper.GetSpecificClaim("ID");
                product.ModifiedDate = DateTime.Now;
                ProductViewModel viewModel = new ProductViewModel()
                {
                    ModifiedUser = helper.GetSpecificClaim("Name")
                };
                await context.SaveChangesAsync();

                result.StatusCode = HttpStatusCode.OK;
                result.Status = Status.Success;
                result.Body = attribute;
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
        /// Details of selected product attribute.
        /// </summary>
        /// <param name="id">Id of productAttribute.</param>
        /// <returns>
        /// Details of selected product attribute. 
        /// </returns>
        [HttpGet("getdetailproductattributevalue/{id}")]
        [ProducesResponseType(typeof(ProductAttributeValues), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> GetDetailProductAttributeValue(int id)
        {
            var result = new Result
            {
                Operation = Operation.Read,
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
                if (id != 0)
                {
                    var attributeValue = await context.ProductAttributeValues.Where(x => x.ID == id).FirstOrDefaultAsync();
                    if (attributeValue == null)
                    {
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "This attribute for product does not exist.";
                        return StatusCode((int)result.StatusCode, result); 
                    }

                    result.StatusCode = HttpStatusCode.OK;
                    result.Status = Status.Success;
                    result.Body = attributeValue;
                    return StatusCode((int)result.StatusCode, result);
                }                
                result.Status = Status.Fail;
                result.StatusCode = HttpStatusCode.BadRequest;
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
        /// List of product attribute.
        /// </summary>
        /// <param name="id">Id od product.</param>
        /// <param name="dataHelper">Datahelper object for paging.</param>
        /// <returns>
        /// Paged list of product attributes.
        /// </returns>
        [HttpGet("getlistproductattributevalue/{id}")]
        [ProducesResponseType(typeof(List<ProductAttributeValues>), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> GetListProductAttributeValue(int id, DataHelperModel dataHelper)
        {
            var result = new Result
            {
                Operation = Operation.Read,
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
                var attribute = from productAttributeValue in context.ProductAttributeValues
                                join attributeName in context.ProductAttributes
                                on productAttributeValue.AttributeID equals attributeName.AttributeID
                                into attributes
                                from attributeName in attributes.DefaultIfEmpty()
                                where productAttributeValue.ProductID == id
                                orderby productAttributeValue.ID descending
                                select new ProductAttributeValueViewModel { ID = productAttributeValue.ID, AttributeID = productAttributeValue.AttributeID, AttributeName = attributeName.AttributeName, ProductID = productAttributeValue.ProductID, Value = productAttributeValue.Value };
                if (attribute.Count() == 0)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Attributes do not exist for the product.";
                    return StatusCode((int)result.StatusCode, result);
                }
                var list = attribute;
                list = DataSort.SortBy(list, dataHelper.SortColumn, dataHelper.SortOrder);
                var resultCount = list.Count();
                var pagedList = DataCount.Page(list, dataHelper.PageNumber, dataHelper.PageSize);
                var resultList = await pagedList.ToListAsync();
                ResultModel resultModel = new ResultModel();
                resultModel.ProductAttributeValueResult = resultList;
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
            catch (Exception e)
            {
                result.Status = Status.Error;
                result.Message = e.Message;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return StatusCode((int)result.StatusCode, result);
            }
        }


        /// <summary>
        /// Deletes selected attribute.
        /// </summary>
        /// <param name="ID">Id of product attribute.</param>
        /// <returns>
        /// Status with success message.
        /// </returns>
        [HttpDelete("deleteproductattributevalue")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> DeleteProductAttributeValue(int ID)
        {
            var result = new Result
            {
                Operation = Operation.Delete,
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

                var attribute = await context.ProductAttributeValues.Where(x => x.ID == ID).FirstOrDefaultAsync();
                if (attribute == null)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Attributes do not exist for the product.";
                    return StatusCode((int)result.StatusCode, result);
                }
                context.ProductAttributeValues.Remove(attribute);
                await context.SaveChangesAsync();

                result.Status = Status.Success;
                result.StatusCode = HttpStatusCode.BadRequest;
                result.Message = "Attribute deleted";
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
        /// Gets product for customer to display on home page.
        /// </summary>
        /// <returns>
        /// List of products.
        /// </returns>
        [HttpGet("getproductsforcustomer")]
        [ProducesResponseType(typeof(List<ProductViewModel>), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<IResult>> GetProductsForCustomer()
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
            try
            {
                var products = from product in context.Products
                               join category in context.Categories
                               on product.CategoryID equals category.CategoryID
                               where product.IsDeleted != true &&
                               product.OnHomePage == true && category.IsActive == true
                               select new ProductViewModel { ProductName = product.ProductName, ProductID = product.ProductID, ShortDescription = product.ShortDescription, CategoryID = product.CategoryID, CategoryName = "", IsActive = product.IsActive, CreatedBy = product.CreatedBy, CreatedDate = product.CreatedDate, CreatedUser = "", Price = product.Price, QuantityInStock = product.QuantityInStock, VisibleEndDate = product.VisibleEndDate, AllowCustomerReviews = product.AllowCustomerReviews, DiscountPercent = product.DiscountPercent, VisibleStartDate = product.VisibleStartDate, IsDiscounted = product.IsDiscounted, LongDescription = product.LongDescription, MarkNew = product.MarkNew, ModelNumber = product.ModelNumber, ModifiedBy = product.ModifiedBy, ModifiedDate = product.ModifiedDate, ModifiedUser = "", OnHomePage = product.OnHomePage, ShipingEnabled = product.ShipingEnabled, ShippingCharges = product.ShippingCharges, Tax = product.Tax, TaxExempted = product.TaxExempted, QuantityType = product.QuantityType };
                var productList = await products.ToListAsync();
                if (productList.Count == 0)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Products for home page are not selected";
                    return StatusCode((int)result.StatusCode, result);
                }

                result.Status = Status.Success;
                result.StatusCode = HttpStatusCode.OK;
                result.Body = productList;
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
        /// Gets images for product to display on home page.
        /// </summary>
        /// <param name="id">Id of product.</param>
        /// <returns>
        /// Image of the selected product.
        /// </returns>
        [HttpGet("getproductimagesforcustomer/{id}")]
        [ProducesResponseType(typeof(ProductImageViewModel), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<IResult>> GetProductImagesForCustomer(int id)
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
            try
            {
                var productImage = await context.ProductImage.Where(x => x.ProductID == id).Take(5).FirstOrDefaultAsync();
                if (productImage == null)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Product image does not exist.";
                    return StatusCode((int)result.StatusCode, result);
                }
                ProductImageViewModel imageViewModel = new ProductImageViewModel()
                {
                    ID = productImage.ProductID,
                    ImageContent = productImage.ImageContent
                };

                result.StatusCode = HttpStatusCode.OK;
                result.Status = Status.Success;
                result.Body = imageViewModel;
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
    }
}
