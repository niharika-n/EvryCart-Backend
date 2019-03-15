using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIs.Data;
using WebAPIs.Models;

namespace WebAPIs.Controllers
{
    /// <summary>
    /// Product Attribute controller.
    /// </summary>
    [Route("api/productattributes")]
    [ApiController]
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


        /// <summary>
        /// Prpduct attribute value.
        /// </summary>
        /// <param name="id">Id of product attribute.</param>
        /// <returns>
        /// Detail of product attribtue.
        /// </returns>
        [HttpGet("detail/{id}")]
        [ProducesResponseType(typeof(ProductAttributeViewModel), StatusCodes.Status206PartialContent)]
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
                    var attributeDetail = from attribute in context.ProductAttributes
                                          where attribute.AttributeID == id
                                          select new ProductAttributeViewModel
                                          { AttributeID = attribute.AttributeID, AttributeName = attribute.AttributeName, CreatedBy = attribute.CreatedBy, AssociatedProductValues = 0, CreatedDate = attribute.CreatedDate, ModifiedBy = attribute.ModifiedBy, ModifiedDate = attribute.ModifiedDate, CreatedUser = "", ModifiedUser = "" };
                    var attributeObj = await attributeDetail.FirstOrDefaultAsync();
                    if (attributeObj != null)
                    {
                        result.Status = Status.Success;
                        result.StatusCode = HttpStatusCode.OK;
                        result.Body = attributeObj;
                        return StatusCode((int)result.StatusCode, result); 
                    }
                    else
                    {
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "Attribute does not exist.";
                        return StatusCode((int)result.StatusCode, result); 
                    }
                }
                result.Status = Status.Fail;
                result.StatusCode = HttpStatusCode.BadRequest;
                result.Message = "Attribute ID is not valid.";
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
        /// Insert Product attribute.
        /// </summary>
        /// <param name="productAttribute">Object of product attribute.</param>
        /// <returns>
        /// Status of attribute added.
        /// </returns>
        [HttpPost("insertattribute")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]        
        public async Task<ActionResult<IResult>> InsertAttribute([FromBody] ProductAttribute productAttribute)
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
                    result.Status = Status.Success;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    return StatusCode((int)result.StatusCode, result);
                }
                var attributeNameCheck = await context.ProductAttributes.Where(x => x.AttributeName == productAttribute.AttributeName).ToListAsync();
                if (attributeNameCheck.Count() != 0)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Attribute exists already";
                    return StatusCode((int)result.StatusCode, result);
                }
                productAttribute.CreatedDate = DateTime.Now;
                productAttribute.CreatedBy = helper.GetSpecificClaim("ID");

                context.ProductAttributes.Add(productAttribute);
                await context.SaveChangesAsync();
                ProductAttributeViewModel attributeViewModel = new ProductAttributeViewModel()
                {
                    CreatedUser = helper.GetSpecificClaim("Name")
                };

                result.Status = Status.Success;
                result.StatusCode = HttpStatusCode.OK;
                result.Body = productAttribute;                
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
        /// Updates attribute.
        /// </summary>
        /// <param name="productAttribute">Object of attribute.</param>
        /// <returns>
        /// Statisu of attribute updated.
        /// </returns>
        [HttpPut("updateattribute")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]        
        public async Task<ActionResult<IResult>> UpdateAttribute([FromBody] ProductAttribute productAttribute)
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
                var attributeObj = context.ProductAttributes.Where(x => x.AttributeID == productAttribute.AttributeID).SingleOrDefault();
                if (attributeObj == null)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Attribute does not exist.";
                    return StatusCode((int)result.StatusCode, result);
                }
                var attributeNameCheck = await context.ProductAttributes.Where(x => (x.AttributeName == productAttribute.AttributeName) && (x.AttributeID != productAttribute.AttributeID)).ToListAsync();
                if (attributeNameCheck.Count() != 0)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Body = "Attribute exists already";
                    return StatusCode((int)result.StatusCode, result);
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
                result.Status = Status.Success;
                result.StatusCode = HttpStatusCode.OK;
                result.Body = attributeObj;
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
        /// <param name="dataHelper">Datahelper object for paging and sorting the list.</param>
        /// <param name="getAll">Chekc ot get all product attributes.</param>
        /// <returns>
        /// List of product attributes.
        /// </returns>
        [HttpGet("listing")]
        [ProducesResponseType(typeof(List<ProductAttributeViewModel>), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]       
        public async Task<ActionResult<IResult>> Listing(DataHelperModel dataHelper, bool getAll)
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
            try
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
                                    AssociatedProductValues = valuesCount.Where(x => x.attributeValues != null ? x.attributeValues.AttributeID == x.attribute.AttributeID : false).Count(),
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
                    ResultModel resultModel = new ResultModel();
                    resultModel.ProductAttributeResult = resultList;
                    resultModel.TotalCount = resultCount;
                    if (resultList.Count == 0)
                    {
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "No records present.";
                        return StatusCode((int)result.StatusCode, result);
                    }

                    result.Status = Status.Success;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Body = resultModel;
                    return StatusCode((int)result.StatusCode, result);
                }
                else
                {
                    listQuery = listQuery.OrderBy(x => x.AttributeName);
                    var attributeList = await listQuery.ToListAsync();
                    result.Body = attributeList;
                    result.Status = Status.Success;
                    result.StatusCode = HttpStatusCode.OK;
                    return StatusCode((int)result.StatusCode, result);
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
        /// Deletes the product attribute.
        /// </summary>
        /// <param name="Id">Id of product attribute.</param>
        /// <returns>
        /// Status for attribute deleted with message.
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
                var deleteQuery = await context.ProductAttributes.Where(x => x.AttributeID == Id).FirstOrDefaultAsync();
                if (deleteQuery == null)
                {
                    result.Status = Status.Success;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Attribute does not exist.";
                    return StatusCode((int)result.StatusCode, result);
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
    }
}