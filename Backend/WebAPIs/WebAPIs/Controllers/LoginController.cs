using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebAPIs.Data;
using WebAPIs.Models;
using System.Linq;
using System;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace WebAPIs.Controllers
{
    /// <summary>
    /// Login controller.
    /// </summary>
    [Route("api/login")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly WebApisContext context;
        private IConfiguration config;
        private EmailService emailService;

        public LoginController(WebApisContext APIcontext, IConfiguration _config)
        {
            context = APIcontext;
            config = _config;
            emailService = new EmailService(_config);
        }


        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="loginModel">username and password of user.</param>
        /// <returns>
        /// Token string for correct details.
        /// </returns>
        [HttpGet("loginuser")]
        [ProducesResponseType(typeof(CategoryViewModel), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]        
        public async Task<ActionResult<IResult>> LoginUser([FromQuery] LoginModel loginModel)
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
            try
            {
                IActionResult response = Unauthorized();
                if (loginModel.Username != null && loginModel.Password != null)
                {
                    if (!ModelState.IsValid)
                    {
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        return StatusCode((int)result.StatusCode, result);
                    }

                    var userDetail = from login in context.Login
                                     where (login.Username == loginModel.Username
                                     && login.Password == loginModel.Password) || (login.EmailID == loginModel.Username && login.Password == loginModel.Password)
                                     select new UserViewModel { UserID = login.UserID, Username = login.Username, EmailID = login.EmailID, FirstName = login.FirstName, ImageContent = login.ImageContent, LastName = login.LastName, RoleID = null };
                    if (userDetail.Count() == 0)
                    {
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "Username or Password is incorrect";
                        return StatusCode((int)result.StatusCode, result);
                    }
                    var user = await userDetail.FirstOrDefaultAsync();
                    var userRoles = await context.AssignedRolesTable.Where(x => x.UserID == user.UserID).Select(x => x.RoleID).ToArrayAsync();
                    user.RoleID = userRoles;
                    var tokenString = BuildToken(user);
                    response = Ok(new { token = tokenString, user });

                    result.Status = Status.Success;
                    result.StatusCode = HttpStatusCode.OK;
                    result.Body = response;
                    return StatusCode((int)result.StatusCode, result);
                }
                else
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message =  "Enter username and password";
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
        /// Generates the token.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>
        /// Returns token generated.
        /// </returns>        
        private string BuildToken(UserViewModel user)
        {
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier , user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.EmailID),
                new Claim("Roles", string.Join(",", user.RoleID))
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(config["Jwt:Issuer"],
              config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(75),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        /// <summary>
        /// Forgot password to reset new password.
        /// </summary>
        /// <param name="Username">Username/Email Address of user.</param>
        /// <returns>
        /// Status with message for email status.
        /// </returns>
        [HttpGet("forgotpassword")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<IResult>> ForgotPassword(string Username)
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
            try
            {
                var User = from userDetail in context.Login
                           where (userDetail.Username == Username) || (userDetail.EmailID == Username)
                           select userDetail;
                var userObj = await User.SingleOrDefaultAsync();
                if (userObj != null)
                {
                    string num = Guid.NewGuid().ToString().Replace("-", "");
                    PasswordResetModel resetModel = new PasswordResetModel
                    {
                        Email = userObj.EmailID,
                        OldPassword = userObj.Password,
                        Token = num,
                        TokenTimeOut = DateTime.Now.AddHours(2),
                        UserID = userObj.UserID
                    };

                    context.PasswordResetTable.Add(resetModel);
                    await context.SaveChangesAsync();

                    var template = await context.ContentTable.Where(x => x.TemplateName == "change_password").FirstOrDefaultAsync();
                    if (template == null)
                    {
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "Email Template not found.";
                        return StatusCode((int)result.StatusCode, result);
                    }
                    var body = template.Content;

                    var url = config["DefaultCorsPolicyName"] + "reset_password/" + num;
                    EmailViewModel emailView = new EmailViewModel();
                    emailView.Subject = "Reset your password";
                    emailView.Body = body.Replace("{ResetUrl}", url).Replace("{UserName}", userObj.Username);
                    emailView.ToEmailList.Add(new MailUser() { Email = userObj.EmailID, Name = userObj.Username });
                    var mail = emailService.SendEmail(emailView);

                    if (mail == "OnSuccess")
                    {
                        result.Status = Status.Success;
                        result.StatusCode = HttpStatusCode.OK;
                        result.Message =  "Success";
                        return StatusCode((int)result.StatusCode, result);
                    }
                    else
                    {
                        result.Status = Status.Fail;
                        result.StatusCode = HttpStatusCode.BadRequest;
                        result.Message = "Fail";
                        return StatusCode((int)result.StatusCode, result);
                    }
                }
                else
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Body = "wrongEmail";
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
        /// Vadilates reset token.
        /// </summary>
        /// <param name="token">Token for reest password.</param>
        /// <returns>
        /// Returns status for email message sent.
        /// </returns>
        [HttpGet("validatetoken")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]      
        public async Task<ActionResult<IResult>> ValidateToken(string token)
        {
            var result = new Result
            {
                Operation = Operation.Read,
                Status = Status.Success
            };
            try
            {
                var tokenDetail = await context.PasswordResetTable.Where(x => x.Token == token && x.PasswordChanged != true && x.TokenTimeOut > DateTime.Now).SingleOrDefaultAsync();
                if (tokenDetail != null)
                {
                    result.Status = Status.Success;
                    result.StatusCode = HttpStatusCode.OK;
                    result.Message = "validIoken";
                    return StatusCode((int)result.StatusCode, result); ;
                }
                else
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "invalidToken";
                    return StatusCode((int)result.StatusCode, result); ;
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
        /// Change user paswword.
        /// </summary>
        /// <param name="userToken">usertoken for verification of url</param>
        /// <param name="newPassword">new password for user.</param>
        /// <returns>
        /// Status with message string.
        /// </returns>
        [HttpPut("changepassword")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status206PartialContent)]
        [ProducesResponseType(typeof(IResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]        
        public async Task<ActionResult<IResult>> ChangePassword(string userToken, string newPassword)
        {
            var result = new Result
            {
                Operation = Operation.Update,
                Status = Status.Success
            };
            try
            {
                var tokenVerify = context.PasswordResetTable.Where(x => x.Token == userToken && x.PasswordChanged != true && x.TokenTimeOut > DateTime.Now).Select(x => x);
                if (tokenVerify.Any())
                {
                    var tokenDetail = await tokenVerify.SingleOrDefaultAsync();
                    var user = await context.Login.Where(x => x.UserID == tokenDetail.UserID).SingleOrDefaultAsync();
                    user.Password = newPassword;
                    await context.SaveChangesAsync();
                    tokenDetail.PasswordChanged = true;
                    tokenDetail.ResetDate = DateTime.Now;
                    await context.SaveChangesAsync();

                    result.Status = Status.Success;
                    result.StatusCode = HttpStatusCode.OK;
                    result.Message = "success";
                    return StatusCode((int)result.StatusCode, result);
                }
                else
                {
                    result.Status = Status.Fail;
                    result.StatusCode = HttpStatusCode.BadRequest;
                    result.Message = "This page does not exist.";
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

    }
}



