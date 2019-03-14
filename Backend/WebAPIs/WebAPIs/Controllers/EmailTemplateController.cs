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
    [Route("api/EmailTemplate/[action]")]
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

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IResult> GetTemplate(string templateType)
        {
            Result result = new Result();
            try
            {
                var template = await context.ContentTable.Where(x => x.TemplateName == templateType).FirstOrDefaultAsync();
                if (template != null)
                {
                    result.Status = true;
                    result.Body = template;
                    return result;
                }
                else
                {
                    result.Message = "Email Template does not exist.";
                    return result;
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
        [HttpPut]
        public async Task<IResult> UpdateTemplate([FromBody] ContentModel contentModel)
        {
            Result result = new Result();
            try
            {
                if (!ModelState.IsValid)
                {
                    result.StatusCode = HttpStatusCode.BadRequest;
                    return result;
                }
                var templateObj = await context.ContentTable.Where(x => x.TemplateName == contentModel.TemplateName).FirstOrDefaultAsync();
                if (templateObj == null)
                {
                    result.Message = "Template does not exist.";
                    return result;
                }
                var duplicateContentCheck = context.ContentTable.Where(x => x.TemplateName != contentModel.TemplateName && x.Content == contentModel.Content);
                if (duplicateContentCheck.Count() != 0)
                {
                    result.Body = new { sameContentMessage = "This content already exists for another content template." };
                    return result;
                }
                templateObj.Content = contentModel.Content;
                await context.SaveChangesAsync();

                result.Status = true;
                result.Body = new { content = templateObj };
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