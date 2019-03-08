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
        public async Task<ActionResult> GetTemplate(string templateType)
        {
            var template = await context.ContentTable.Where(x => x.TemplateName == templateType).FirstOrDefaultAsync();
            if (template != null)
            {
                return Ok(template);
            }
            else
            {
                return NotFound(new { message = "Email Template does not exist." });
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut]
        public async Task<ActionResult> UpdateTemplate([FromBody] ContentModel contentModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var templateObj = await context.ContentTable.Where(x => x.TemplateName == contentModel.TemplateName).FirstOrDefaultAsync();
            if (templateObj == null)
            {
                return NotFound(new { message = "Template does not exist." });
            }
            var duplicateContentCheck = context.ContentTable.Where(x => x.TemplateName != contentModel.TemplateName && x.Content == contentModel.Content);
            if (duplicateContentCheck.Count() != 0)
            {
                return Ok(new { sameContentMessage = "This content already exists for another content template." });
            }
            templateObj.Content = contentModel.Content;            
            await context.SaveChangesAsync();            
            return Ok(new { content =  templateObj});
        }
    }
}