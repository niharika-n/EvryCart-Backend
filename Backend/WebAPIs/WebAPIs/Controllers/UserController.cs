using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WebAPIs.Data;
using WebAPIs.Models;
using System;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace WebAPIs.Controllers
{
    [Route("api/user/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly WebApisContext context;
        private IConfiguration config;
        Helper helper;
        private readonly IHostingEnvironment environment;
        private EmailService emailService;

        public UserController(WebApisContext APIcontext, IPrincipal _principal, IConfiguration _config, IHostingEnvironment _environment)
        {
            context = APIcontext;
            config = _config;
            environment = _environment;
            helper = new Helper(_principal);
            emailService = new EmailService(_config);
        }

        /// <summary>
        /// Creates new user.
        /// </summary>
        /// <param name="login"></param>
        /// <returns>
        /// Login object with new UserID.
        /// </returns>        
        [AllowAnonymous]
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> CreateUser()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            IFormFile img = null;
            var file = JsonConvert.DeserializeObject<UserModel>(Request.Form["model"]);
            var userRoles = JsonConvert.DeserializeObject<Enums[]>(Request.Form["role"]);
            UserModel user = new UserModel();
            var image = Request.Form.Files;
            foreach (var i in image)
            {
                img = image[0];
            }
            ImageService imageService = new ImageService();
            var userCheckQuery = context.Login.Where(x => (x.Username == file.Username) || (x.EmailID == file.EmailID));
            if (userCheckQuery.Count() != 0)
            {
                var userCheck = await userCheckQuery.ToArrayAsync();
                foreach (var existingUser in userCheck)
                {
                    if (existingUser.Username == file.Username)
                    {
                        return Ok(new { usernameMessage = "This username exists already" });
                    }
                    else if (existingUser.EmailID == file.EmailID)
                    {
                        return Ok(new { emailMessage = "This Email exists already" });
                    }
                }
            }
            user.UserID = file.UserID;
            user.Username = file.Username;
            user.FirstName = file.FirstName;
            user.LastName = file.LastName;
            user.EmailID = file.EmailID;
            user.Password = file.Password;
            user.ImageContent = imageService.Image(img);
            context.Login.Add(user);
            await context.SaveChangesAsync();            
            var createdUserId = await context.Login.Where(x => x.EmailID == file.EmailID).Select(x => x.UserID).FirstOrDefaultAsync();
            var result = AssignUserRole(createdUserId, false, userRoles);
            if (result)
            {
                return Ok(new { success = "user created." });
            }
            else
            {
                return Ok(new { fail = "user could not be created." });
            }
        }

        [Authorize(Policy = "SuperAdminOnly")]
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> CreateUserFromAdmin()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            IFormFile img = null;
            var file = JsonConvert.DeserializeObject<UserModel>(Request.Form["model"]);
            var userRoles = JsonConvert.DeserializeObject<int[]>(Request.Form["role"]);
            UserModel user = new UserModel();
            var image = Request.Form.Files;
            foreach (var i in image)
            {
                img = image[0];
            }
            ImageService imageService = new ImageService();
            var userCheckQuery = context.Login.Where(x => (x.Username == file.Username) || (x.EmailID == file.EmailID));
            if (userCheckQuery.Count() != 0)
            {
                var userCheck = await userCheckQuery.ToArrayAsync();
                foreach (var existingUser in userCheck)
                {
                    if (existingUser.Username == file.Username)
                    {
                        return Ok(new { usernameMessage = "This username exists already" });
                    }
                    else if (existingUser.EmailID == file.EmailID)
                    {
                        return Ok(new { emailMessage = "This Email exists already" });
                    }
                }
            }
            string tempPassword = Guid.NewGuid().ToString().Replace("-", "");
            user.UserID = file.UserID;
            user.Username = file.Username;
            user.FirstName = file.FirstName;
            user.LastName = file.LastName;
            user.EmailID = file.EmailID;
            user.Password = tempPassword;
            user.ImageContent = imageService.Image(img);
            context.Login.Add(user);
            await context.SaveChangesAsync();
            var totalRoles = Enum.GetValues(typeof(Enums)).Cast<int>();
            var selectedRoles = userRoles.Intersect(totalRoles).ToArray();
            List<Enums> roleList = new List<Enums>();
            foreach (var role in selectedRoles)
            {
                Enums roleVal = (Enums)role;
                roleList.Add(roleVal);
            }
            var roleTypes = roleList.ToArray();
            var createdUserId = await context.Login.Where(x => x.EmailID == file.EmailID).Select(x => x.UserID).FirstOrDefaultAsync();
            var result = AssignUserRole(createdUserId, true, roleTypes);
            if (result == true)
            {
                return Ok(new { success = "User created." });
            }
            else if (result == "Email not sent.")
            {
                return Ok(new { emailError = "User created, password Email not sent." });
            }
            else
            {
                return Ok(new { fail = "User not created." });
            }
        }

        private dynamic AssignUserRole(int createdID, bool fromAdmin, params Enums[] roles)
        {
            var userRoles = context.AssignedRolesTable.Where(x => x.UserID == createdID).Select(x => x.RoleID).ToArray();
            if (userRoles.Count() == 0)
            {
                foreach (var role in roles)
                {
                    AssignedRolesModel assignedRoles = new AssignedRolesModel();
                    assignedRoles.RoleID = (int)role;
                    assignedRoles.UserID = createdID;
                    context.AssignedRolesTable.Add(assignedRoles);
                    context.SaveChangesAsync();
                }
                if (!fromAdmin)
                {
                    return true;
                }
                else
                {
                    var assignedNewRole = context.AssignedRolesTable.Where(x => x.UserID == createdID).Select(x => x.RoleID).ToArray();
                    if (assignedNewRole.Contains((int)Enums.Admin) && assignedNewRole.Count() == 1)
                    {
                        AssignedRolesModel assignedRoles = new AssignedRolesModel();
                        assignedRoles.RoleID = (int)Enums.User;
                        assignedRoles.UserID = createdID;
                        context.AssignedRolesTable.Add(assignedRoles);
                        context.SaveChangesAsync();
                    }
                    var result = SendMail(createdID);
                    if (result)
                    {
                        return true;
                    }
                    else
                    {
                        return "Email not sent.";
                    }
                }
            }
            else
            {
                return false;
            }
        }

        private bool SendMail(int userID)
        {
            var user = context.Login.Where(x => x.UserID == userID).FirstOrDefault();
            string num = Guid.NewGuid().ToString().Replace("-", "");
            PasswordResetModel resetModel = new PasswordResetModel
            {
                Email = user.EmailID,
                OldPassword = user.Password,
                Token = num,
                TokenTimeOut = DateTime.Now.AddHours(2),
                UserID = user.UserID
            };

            context.PasswordResetTable.Add(resetModel);
            context.SaveChangesAsync();

            string absolutePath = Path.GetFullPath("Data\\ResetPassword.html");
            var template = context.ContentTable.Where(x => x.TemplateName == "change_password").FirstOrDefault();
            if (template == null)
            {
                return false;
            }
            var body = template.Content;
            var url = config["DefaultCorsPolicyName"] + "reset_password/" + num;
            EmailViewModel emailView = new EmailViewModel();
            emailView.Subject = "Reset your password";
            emailView.Body = body.Replace("{ResetUrl}", url).Replace("{UserName}", user.Username);
            emailView.ToEmailList.Add(new MailUser() { Email = user.EmailID, Name = user.Username });
            var mail = emailService.SendEmail(emailView);
            if (mail == "OnSuccess")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id != 0)
            {
                var detail = from user in context.Login
                             where user.UserID == id
                             select new UserViewModel { UserID = user.UserID, EmailID = user.EmailID, FirstName = user.FirstName, ImageContent = user.ImageContent, LastName = user.LastName, RoleID = null, Username = user.Username };
                var role = await context.AssignedRolesTable.Where(x => x.UserID == id).Select(x => x.RoleID).ToArrayAsync();
                var userDetail = await detail.FirstOrDefaultAsync();
                userDetail.RoleID = role;
                if (userDetail != null)
                {
                    return Ok(userDetail);
                }
                else
                {
                    return NotFound("User does not exist.");
                }
            }
            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut]
        public async Task<IActionResult> Update()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var file = JsonConvert.DeserializeObject<UserModel>(Request.Form["model"]);
            var user = context.Login.Where(x => x.UserID == file.UserID).FirstOrDefault();
            var userCheckQuery = context.Login.Where(x => ((x.Username == file.Username) || (x.EmailID == file.EmailID)) && (x.UserID != file.UserID));
            if (userCheckQuery.Count() != 0)
            {
                var userCheck = await userCheckQuery.ToArrayAsync();
                foreach (var existingUser in userCheck)
                {
                    if (existingUser.Username == file.Username)
                    {
                        return Ok(new { usernameMessage = "This username exists already" });
                    }
                    else if (existingUser.EmailID == file.EmailID)
                    {
                        return Ok(new { emailMessage = "This Email exists already" });
                    }
                }
            }
            if (Request.Form.Files.Count != 0)
            {
                if (user == null)
                {
                    return Ok(new { message = "User does not exist." });
                }
                IFormFile img = null;
                var image = Request.Form.Files;
                foreach (var i in image)
                {
                    img = image[0];
                }
                ImageService imageService = new ImageService();
                user.ImageContent = imageService.Image(img);
            }
            user.FirstName = file.FirstName;
            user.LastName = file.LastName;
            user.Username = file.Username;
            user.EmailID = file.EmailID;
            await context.SaveChangesAsync();
            return Ok(new { user });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            var userEmail = helper.GetSpecificClaim("Email");
            var PasswordCheckModel = await context.Login.Where(x => x.Password == oldPassword).FirstOrDefaultAsync();
            if (PasswordCheckModel == null)
            {
                return NotFound("User details incorrect");
            }
            if (userEmail == PasswordCheckModel.EmailID)
            {
                if (PasswordCheckModel.Password != oldPassword)
                {
                    return NotFound("Incorrect password entered");
                }
                else
                {
                    PasswordCheckModel.Password = newPassword;
                    await context.SaveChangesAsync();
                    return Ok("Password changed successfully");
                }
            }
            return NotFound("Incorrect Password entered");
        }

    }
}