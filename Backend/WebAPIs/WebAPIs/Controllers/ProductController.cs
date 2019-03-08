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

    [Route("api/Product/[action]")]
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

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetDetail(int? id)
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
                if ( productDetail.ToList().Count() != 0)
                {
                    return Ok(new { product = productObj});
                }
                else
                {
                    return BadRequest("Product does not exist.");
                }
            }
            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<ActionResult> InsertProduct([FromBody]ProductModel product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var productCheckQuery = context.Products.Where(x => (x.ProductName == product.ProductName && x.CategoryID == product.CategoryID) || (x.ModelNumber == product.ModelNumber));
            if(productCheckQuery.Count() != 0)
            {
                var productCheck = await productCheckQuery.ToArrayAsync();
                foreach (var pdt in productCheck) {
                    if (pdt.ProductName == product.ProductName && pdt.CategoryID == product.CategoryID)
                    {
                        return Ok(new { sameNameMessage = "This product already exists in the Category" });
                    }
                    else if (pdt.ModelNumber == product.ModelNumber)
                    {
                        return Ok(new { sameModelMessage = "Product with this Model Number already exists" });
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
            return Ok(new { productObj = product });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductModel product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var productObj = context.Products.Where(x => x.ProductID == product.ProductID && x.IsDeleted != true).SingleOrDefault();
                if (productObj == null)
                {
                    return NotFound("Product does not exist");
                }
                var productCheckQuery = context.Products.Where(x => ((x.ProductName == product.ProductName && x.CategoryID == product.CategoryID) || (x.ModelNumber == product.ModelNumber)) && (x.ProductID != product.ProductID));
                if (productCheckQuery.Count() != 0)
                {
                    var productCheck = await productCheckQuery.ToArrayAsync();
                    foreach (var pdt in productCheck)
                    {
                        if (pdt.ProductName == product.ProductName && pdt.CategoryID == product.CategoryID)
                        {
                            return Ok(new { sameNameMessage = "Product already exists in this Category" });
                        }
                        else if (pdt.ModelNumber == product.ModelNumber)
                        {
                            return Ok(new { sameModelMessage = "Product with same Model number already exists" });
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
                return Ok(new {product = productObj });
            }
            catch (Exception e)
            {
                return Ok(e.ToString());
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Listing(DataHelperModel dataHelper)
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
            ResultModel result = new ResultModel();
            result.ProductResult = resultList;
            result.TotalCount = resultCount;
            if (resultList.Count == 0)
            {
                return NotFound("No records present.");
            }
            return Ok(result);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int ID)
        {
            var deleteQuery = await context.Products.Where(x => x.ProductID == ID && x.IsDeleted != true).FirstOrDefaultAsync();
            if (deleteQuery == null)
            {
                return NotFound("Product does not exist.");
            }
            deleteQuery.IsDeleted = true;
            var result  = await context.SaveChangesAsync();
            if (result > 0)
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
            return Ok("Deleted successfully.");
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductImages(int id)
        {
            var pdtImage = from productImage in context.ProductImage
                           where productImage.ProductID == id
                           orderby productImage.ID descending
                           select new ProductImageViewModel {ID = productImage.ID, ImageContent = productImage.ImageContent };
            if (pdtImage.Count() == 0)
            {
                return NotFound("Prodouct images do not exist.");
            }
            var list = await pdtImage.ToListAsync();
            if (list.Count == 0)
            {
                return NotFound("No records present.");
            }
            ResultModel result = new ResultModel();
            result.ProductImageResult = list;            
            return Ok(result);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> AddProductImages()
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
            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete]
        public async Task<IActionResult> DeleteProductImage(int pdtID, int imageID)
        {
            var image = await context.ProductImage.Where(x => x.ProductID == pdtID && x.ID == imageID).SingleOrDefaultAsync();
            if (image == null)
            {
                return NotFound("Image does not exist.");
            }
            context.ProductImage.Remove(image);
            await context.SaveChangesAsync();
            return Ok("image deleted");
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> InsertProductAttributeValue([FromBody] ProductAttributeValues attributeValue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var attributeValueCheck = await context.ProductAttributeValues.Where(x => x.Value == attributeValue.Value && x.ProductID == attributeValue.ProductID).ToListAsync();
            if(attributeValueCheck.Count() != 0)
            {
                return Ok(new {message = "Product value exists already" });
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

            return Ok(new { attributeVal = attributeValue });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut]
        public async Task<IActionResult> UpdateProductAttributeValue([FromBody] ProductAttributeValues attributeValue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var attribute = await context.ProductAttributeValues.Where(x => x.ID == attributeValue.ID).FirstOrDefaultAsync();
            var attributeValueCheck = await context.ProductAttributeValues.Where(x => x.Value == attributeValue.Value).ToListAsync();
            if (attributeValueCheck.Count() != 0)
            {
                return Ok(new { message = "Product value exists already" });
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
            return Ok(new { attributeVal = attribute });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetailProductAttributeValue(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (id != 0)
            {
                var attributeValue = await context.ProductAttributeValues.Where(x => x.ID == id).FirstOrDefaultAsync();
                if (attributeValue == null)
                {
                    return Ok("This attribute for product does not exist.");
                }
                return Ok(attributeValue);
            }
            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        public async Task<IActionResult> ListProductAttributeValue(int id, DataHelperModel dataHelper)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
                return NotFound("Attributes do not exist for the product.");
            }
            var list = attribute;
            list = DataSort.SortBy(list, dataHelper.SortColumn, dataHelper.SortOrder);
            var resultCount = list.Count();
            var pagedList = DataCount.Page(list, dataHelper.PageNumber, dataHelper.PageSize);
            var resultList = await pagedList.ToListAsync();
            ResultModel result = new ResultModel();
            result.ProductAttributeValueResult = resultList;
            result.TotalCount = resultCount;
            if (resultList.Count == 0)
            {
                return NotFound("No records present.");
            }
            return Ok(result);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete]
        public async Task<IActionResult> DeleteProductAttributeValue(int ID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var attribute = await context.ProductAttributeValues.Where(x => x.ID == ID).FirstOrDefaultAsync();
            if (attribute == null)
            {
                return NotFound("Attrbutes do not exist for the product.");
            }
            context.ProductAttributeValues.Remove(attribute);
            await context.SaveChangesAsync();
            return Ok("attribute deleted");
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetProductsForCustomer()
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
                return NotFound("Products for home page are not selected");
            }
            return Ok(productList);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductImagesForCustomer(int id)
        {
            var productImage = await context.ProductImage.Where(x => x.ProductID == id).Take(5).FirstOrDefaultAsync();
            if (productImage == null)
            {
                return NotFound("Product image does not exist.");
            }
            ProductImageViewModel imageViewModel = new ProductImageViewModel()
            {
                ID = productImage.ProductID,
                ImageContent = productImage.ImageContent
            };
            return Ok(imageViewModel);
        }
    }
}
