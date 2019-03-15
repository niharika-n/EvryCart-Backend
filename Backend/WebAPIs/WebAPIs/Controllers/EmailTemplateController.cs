using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPIs.Data;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore;
using WebAPIs.Models;
using System.Net;

namespace WebAPIs.Controllers
{
    [Route("api/emailtemplate")]
    [ApiController]
    public class EmailTemplateController : ControllerBase
    {
        private WebApisContext context;
        private readonly ClaimsPrincipal principal;
        Helper helper;
        public EmailTemplateController(WebApisContext _context, IPrincipal _principal)
        {
            context = _context;
            principal = _principal as ClaimsPrincipal;
            helper = new Helper(_principal);
        }


        /// <summary>
        /// Gets template.
        /// </summary>
        /// <param name="templateType">Template name for selected template.</param>
        /// <returns>
        /// Returns selected template.
        /// </returns>
        [HttpGet("gettemplate")]
        [ProducesResponseType(typeof(ContentViewModel), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]        
        public async Task<ActionResult<IResult>> GetTemplate(string templateType)
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
            try
            {
                var templateModel = from template in context.ContentTable
                                    where template.TemplateName == templateType
                                    select new ContentViewModel { ID = template.ID, TemplateName = template.TemplateName, Content = template.Content};
                var templateObj = await templateModel.FirstOrDefaultAsync();

                if (templateObj != null)
                {
                    result.Status = Status.Success;
                    result.StatusCode = HttpStatusCode.OK;
                    result.Body = templateObj;
                    return StatusCode((int)result.StatusCode, result);
                }
                else
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Email Template does not exist.";
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
        /// Updates template.
        /// </summary>
        /// <param name="contentModel">Template name of selected template.</param>
        /// <returns>
        /// Status of template updated.
        /// </returns>
        [HttpPut("updatetemplate")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]       
        public async Task<ActionResult<IResult>> UpdateTemplate([FromBody] ContentModel contentModel)
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
                var templateObj = await context.ContentTable.Where(x => x.TemplateName == contentModel.TemplateName).FirstOrDefaultAsync();
                if (templateObj == null)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "Template does not exist.";
                    return StatusCode((int)result.StatusCode, result);
                }
                var duplicateContentCheck = context.ContentTable.Where(x => x.TemplateName != contentModel.TemplateName && x.Content == contentModel.Content);
                if (duplicateContentCheck.Count() != 0)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "sameContentMessage";
                    return StatusCode((int)result.StatusCode, result);
                }
                templateObj.Content = contentModel.Content;
                await context.SaveChangesAsync();

                result.Status = Status.Success;
                result.StatusCode = HttpStatusCode.OK;
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