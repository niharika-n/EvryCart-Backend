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
using System.Net;

namespace WebAPIs.Controllers
{
    /// <summary>
    /// User controller.
    /// </summary>
    [Route("api/user")]
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
        /// <returns>
        /// Login object with new UserID.
        /// </returns>        
        [HttpPost("createuser")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<IResult>> CreateUser()
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
                    return result;
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
                            result.Status = Status.Fail;
                            result.StatusCode = HttpStatusCode.BadRequest;
                            result.Body = "usernameMessage";
                            return StatusCode((int)result.StatusCode, result);
                        }
                        else if (existingUser.EmailID == file.EmailID)
                        {
                            result.Status = Status.Fail;
                            result.StatusCode = HttpStatusCode.BadRequest;
                            result.Body = "emailMessage";
                            return StatusCode((int)result.StatusCode, result);
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
                var assignRole = AssignUserRole(createdUserId, false, userRoles);
                if (assignRole)
                {
                    result.Status = Status.Success;
                    result.StatusCode = HttpStatusCode.OK;
                    result.Body = "success";
                    return StatusCode((int)result.StatusCode, result);
                }
                else
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Body = "fail";
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
        /// Creates new user.
        /// </summary>
        /// <returns>
        /// Status with message string for success.
        /// </returns>
        [HttpPost("createuserfromadmin")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<ActionResult<IResult>> CreateUserFromAdmin()
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
                    return result;
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
                            result.Status = Status.Fail;
                            result.StatusCode = HttpStatusCode.BadRequest;
                            result.Message = "usernameMessage";
                            return StatusCode((int)result.StatusCode, result);
                        }
                        else if (existingUser.EmailID == file.EmailID)
                        {
                            result.Status = Status.Fail;
                            result.StatusCode = HttpStatusCode.BadRequest;
                            result.Message = "emailMessage";
                            return StatusCode((int)result.StatusCode, result);
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
                var assignRole = AssignUserRole(createdUserId, true, roleTypes);
                if (assignRole == true)
                {
                    result.Status = Status.Success;
                    result.StatusCode = HttpStatusCode.OK;
                    result.Message = "success";
                    return StatusCode((int)result.StatusCode, result);
                }
                else if (assignRole == "Email not sent.")
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "emailError";
                    return StatusCode((int)result.StatusCode, result);
                }
                else
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "fail";
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


        private dynamic AssignUserRole(int createdID, bool fromAdmin, params Enums[] roles)
        {
            try
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
            catch (Exception e)
            {
                var result = new Result();
                result.Status = Status.Error;
                result.Message = e.Message;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return StatusCode((int)result.StatusCode, result);
            }
        }


        private bool SendMail(int userID)
        {
            try
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
            catch (Exception e)
            {
                var result = new Result();
                result.Status = Status.Error;
                result.Message = e.Message;
                result.StatusCode = HttpStatusCode.InternalServerError;

                return false;
            }
        }


        /// <summary>
        /// User detail.
        /// </summary>
        /// <param name="id">Id of user</param>
        /// <returns>
        /// Returns user details.
        /// </returns>
        [HttpGet("detail/{id}")]
        [ProducesResponseType(typeof(UserViewModel), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Detail(int? id)
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
                    var detail = from user in context.Login
                                 where user.UserID == id
                                 select new UserViewModel { UserID = user.UserID, EmailID = user.EmailID, FirstName = user.FirstName, ImageContent = user.ImageContent, LastName = user.LastName, RoleID = null, Username = user.Username };
                    var role = await context.AssignedRolesTable.Where(x => x.UserID == id).Select(x => x.RoleID).ToArrayAsync();
                    var userDetail = await detail.FirstOrDefaultAsync();
                    userDetail.RoleID = role;
                    if (userDetail != null)
                    {
                        result.Status = Status.Success;
                        result.StatusCode = HttpStatusCode.OK;
                        result.Body = userDetail;
                        return StatusCode((int)result.StatusCode, result);
                    }
                    else
                    {
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "User does not exist.";
                        return StatusCode((int)result.StatusCode, result);
                    }
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
        /// Update user details.
        /// </summary>
        /// <returns>
        /// Status of user updated.
        /// </returns>
        [HttpPut("update")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IResult>> Update()
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
                    return result;
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
                            result.Status = Status.Fail;
                            result.StatusCode = HttpStatusCode.BadRequest;
                            result.Message = "usernameMessage";
                            return result;
                        }
                        else if (existingUser.EmailID == file.EmailID)
                        {
                            result.Status = Status.Fail;
                            result.StatusCode = HttpStatusCode.BadRequest;
                            result.Message = "emailMessage";
                            return result;
                        }
                    }
                }
                if (Request.Form.Files.Count != 0)
                {
                    if (user == null)
                    {
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "User does not exist.";
                        return result;
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


        /// <summary>
        /// Change password.
        /// </summary>
        /// <param name="oldPassword">Old password of user.</param>
        /// <param name="newPassword">New password set.</param>
        /// <returns>
        /// Status of password changed.
        /// </returns>
        [HttpPut("changepassword")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            var result = new Result
            {
                Operation = Operation.Update,
                Status = Status.Success
            };
            try
            {
                var userEmail = helper.GetSpecificClaim("Email");
                var PasswordCheckModel = await context.Login.Where(x => x.Password == oldPassword).FirstOrDefaultAsync();
                if (PasswordCheckModel == null)
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "User details incorrect";
                    return StatusCode((int)result.StatusCode, result);
                }
                if (userEmail == PasswordCheckModel.EmailID)
                {
                    if (PasswordCheckModel.Password != oldPassword)
                    {
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "Incorrect password entered.";
                        return StatusCode((int)result.StatusCode, result);
                    }
                    else
                    {
                        PasswordCheckModel.Password = newPassword;
                        await context.SaveChangesAsync();
                        result.Status = Status.Success;
                        result.StatusCode = HttpStatusCode.OK;
                        result.Message = "Password changed successfully.";
                        return StatusCode((int)result.StatusCode, result);
                    }
                }
                result.Status = Status.Success;
                result.StatusCode = HttpStatusCode.OK;
                result.Message = "Incorrect Password entered.";
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