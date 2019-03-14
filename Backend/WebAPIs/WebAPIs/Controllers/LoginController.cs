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

namespace WebAPIs.Controllers
{
    [Route("api/login/[action]")]
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
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>
        /// Token string for correct details.
        /// </returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IResult> LoginUser([FromQuery] LoginModel loginModel)
        {
            Result result = new Result();
            try
            {
                IActionResult response = Unauthorized();
                if (loginModel.Username != null && loginModel.Password != null)
                {
                    if (!ModelState.IsValid)
                    {
                        result.StatusCode = HttpStatusCode.BadRequest;
                        return result;
                    }

                    var userDetail = from login in context.Login
                                     where (login.Username == loginModel.Username
                                     && login.Password == loginModel.Password) || (login.EmailID == loginModel.Username && login.Password == loginModel.Password)
                                     select new UserViewModel { UserID = login.UserID, Username = login.Username, EmailID = login.EmailID, FirstName = login.FirstName, ImageContent = login.ImageContent, LastName = login.LastName, RoleID = null };
                    if (userDetail.Count() == 0)
                    {
                        result.Body = new { message = "Username or Password is incorrect" };
                        return result;
                    }
                    var user = await userDetail.FirstOrDefaultAsync();
                    var userRoles = await context.AssignedRolesTable.Where(x => x.UserID == user.UserID).Select(x => x.RoleID).ToArrayAsync();
                    user.RoleID = userRoles;
                    var tokenString = BuildToken(user);
                    response = Ok(new { token = tokenString, user });

                    result.Status = true;
                    result.Body = response;
                    return result;
                }
                else
                {
                    result.Body = new { message = "Enter username and password" };
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


        [AllowAnonymous]
        [HttpGet]
        public async Task<IResult> ForgotPassword(string Username)
        {
            Result result = new Result();
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
                        result.Message = "Email Template not found.";
                        return result;
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
                        result.Status = true;
                        result.Body = new { success = "Email sent." };
                        return result;
                    }
                    else
                    {
                        result.Body = new { fail = "Email cound not be sent." };
                        return result;
                    }
                }
                else
                {
                    result.Body = new { wrongEmail = "This email address does not exist" };
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


        [AllowAnonymous]
        [HttpGet]
        public async Task<IResult> ValidateToken(string token)
        {
            Result result = new Result();
            try
            {
                var tokenDetail = await context.PasswordResetTable.Where(x => x.Token == token && x.PasswordChanged != true && x.TokenTimeOut > DateTime.Now).SingleOrDefaultAsync();
                if (tokenDetail != null)
                {
                    result.Body = new { vaildIoken = "token is valid" };
                    return result;
                }
                else
                {
                    result.Body = new { invalidToken = "token is not valid" };
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


        [AllowAnonymous]
        [HttpPut]
        public async Task<IResult> ChangePassword(string userToken, string newPassword)
        {
            Result result = new Result();
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

                    result.Status = true;
                    result.Body = new { success = "Password changed" };
                    return result;
                }
                else
                {
                    result.Body = new { notFound = "This page does not exist." };
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

    }
}



