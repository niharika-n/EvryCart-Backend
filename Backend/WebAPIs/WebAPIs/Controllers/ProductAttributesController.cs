using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIs.Data;
using WebAPIs.Models;

namespace WebAPIs.Controllers
{
    [Route("api/ProductAttributes/[action]")]
    public class ProductAttributesController : ControllerBase
    {
        private WebApisContext context;
        private readonly ClaimsPrincipal principal;
        Helper helper;

        public ProductAttributesController(WebApisContext _context, IPrincipal _principal)
        {
            context = _context;
            principal = _principal as ClaimsPrincipal;
            helper = new Helper(_principal);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id != 0)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var attribute = await context.ProductAttributes.Where(x => x.AttributeID == id).FirstOrDefaultAsync();
                if (attribute != null)
                {
                    return Ok(attribute);
                }
                else
                {
                    return NotFound("Attribute does not exist.");
                }
            }
            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> InsertAttribute([FromBody] ProductAttribute productAttribute)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var attributeNameCheck = await context.ProductAttributes.Where(x => x.AttributeName == productAttribute.AttributeName).ToListAsync();
            if (attributeNameCheck.Count() != 0)
            {
                return Ok(new { message = "Attribute exists already" });
            }
            productAttribute.CreatedDate = DateTime.Now;
            productAttribute.CreatedBy = helper.GetSpecificClaim("ID");

            context.ProductAttributes.Add(productAttribute);
            await context.SaveChangesAsync();
            ProductAttributeViewModel attributeViewModel = new ProductAttributeViewModel()
            {
                CreatedUser = helper.GetSpecificClaim("Name")
            };
            return Ok(new { attribute = productAttribute });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut]
        public async Task<IActionResult> UpdateAttribute([FromBody] ProductAttribute productAttribute)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var attributeObj = context.ProductAttributes.Where(x => x.AttributeID == productAttribute.AttributeID).SingleOrDefault();
            if (attributeObj == null)
            {
                return NotFound("Attribute does not exist");
            }
            var attributeNameCheck = await context.ProductAttributes.Where(x => (x.AttributeName == productAttribute.AttributeName) && (x.AttributeID != productAttribute.AttributeID)).ToListAsync();
            if (attributeNameCheck.Count() != 0)
            {
                return Ok(new { message = "Attribute exists already" });
            }
            attributeObj.AttributeID = productAttribute.AttributeID;
            attributeObj.AttributeName = productAttribute.AttributeName;
            attributeObj.ModifiedDate = DateTime.Now;
            attributeObj.ModifiedBy = helper.GetSpecificClaim("ID");
            await context.SaveChangesAsync();
            ProductViewModel localmodel = new ProductViewModel()
            {
                ModifiedUser = helper.GetSpecificClaim("Name")
            };
            return Ok(new { attribute = attributeObj });

        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Listing(DataHelperModel dataHelper, bool getAll)
        {
            var listQuery = from attribute in context.ProductAttributes
                            join createdUser in context.Login
                            on attribute.CreatedBy equals createdUser.UserID
                            into createname
                            from createdUsername in createname.DefaultIfEmpty()
                            let createdByUser = createdUsername.Username
                            join modifiedUser in context.Login
                            on attribute.ModifiedBy equals modifiedUser.UserID
                            into modifyname
                            from modifiedUsername in modifyname.DefaultIfEmpty()
                            let modifiedByUser = modifiedUsername.Username
                            join Values in context.ProductAttributeValues
                            on attribute.AttributeID equals Values.AttributeID
                            into attributeValuesCount
                            from attributeValues in attributeValuesCount.DefaultIfEmpty()                               
                            group new { attributeValues, attribute, createdByUser, modifiedByUser } by
                            new { attribute, createdByUser, modifiedByUser } into valuesCount
                            select new ProductAttributeViewModel
                            {
                                AttributeID = valuesCount.Key.attribute.AttributeID,
                                AttributeName = valuesCount.Key.attribute.AttributeName,
                                CreatedBy = valuesCount.Key.attribute.CreatedBy,
                                AssociatedProductValues = valuesCount.Where(x => x.attributeValues!= null? x.attributeValues.AttributeID == x.attribute.AttributeID : false).Count(),
                                CreatedDate = valuesCount.Key.attribute.CreatedDate,
                                ModifiedBy = valuesCount.Key.attribute.ModifiedBy,
                                ModifiedDate = valuesCount.Key.attribute.ModifiedDate,
                                CreatedUser = valuesCount.Key.createdByUser,
                                ModifiedUser = valuesCount.Key.modifiedByUser
                            };
            if (!getAll)
            {
                if (dataHelper.Search != null)
                {
                    listQuery = listQuery.Where(x => x.AttributeName.Contains(dataHelper.Search));
                }
                var list = listQuery;
                list = DataSort.SortBy(list, dataHelper.SortColumn, dataHelper.SortOrder);
                var resultCount = list.Count();
                var pagedList = DataCount.Page(list, dataHelper.PageNumber, dataHelper.PageSize);
                var resultList = await pagedList.ToListAsync();
                ResultModel result = new ResultModel();
                result.ProductAttributeResult = resultList;
                result.TotalCount = resultCount;
                if (resultList.Count == 0)
                {
                    return NotFound("No records present.");
                }
                return Ok(result);
            }
            else
            {
                listQuery = listQuery.OrderBy(x => x.AttributeName);
                var attributeList = await listQuery.ToListAsync();
                return Ok(attributeList);
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int Id)
        {
            var deleteQuery = await context.ProductAttributes.Where(x => x.AttributeID == Id).FirstOrDefaultAsync();
            if (deleteQuery == null)
            {
                return NotFound("Attribute does not exist.");
            }
            context.ProductAttributes.Remove(deleteQuery);
            await context.SaveChangesAsync();
            var deletedAttribute = await context.ProductAttributes.Where(x => x.AttributeID == Id).FirstOrDefaultAsync();
            if (deletedAttribute == null)
            {
                var attributeValues = await context.ProductAttributeValues.Where(x => x.AttributeID == Id).ToListAsync();
                context.ProductAttributeValues.RemoveRange(attributeValues);
                await context.SaveChangesAsync();
            }
            return Ok("Deleted successfully.");
        }

    }
}