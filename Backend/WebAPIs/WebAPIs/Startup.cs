using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using WebAPIs.Data;
using WebAPIs.Models;

namespace WebAPIs
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private const string DefaultCorsPolicyName = "http://localhost:4200/";

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IPrincipal>(
                provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);

            services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName, p =>
                {
                    p.WithOrigins(Configuration["AllowedHeader"]).AllowAnyHeader().AllowAnyMethod();
                });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
            };
        });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.AddRequirements(new RoleRequirement(RoleTypes.Admin)));
                options.AddPolicy("UserOnly", policy => policy.AddRequirements(new RoleRequirement(RoleTypes.User)));
                options.AddPolicy("SuperAdminOnly", policy => policy.AddRequirements(new RoleRequirement(RoleTypes.SuperAdmin)));
            });
            services.AddSingleton<IAuthorizationHandler, RoleHandler>();

            services.AddAutoMapper();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddMvc(setup => { })
              .AddFluentValidation();
            services.AddTransient<IValidator<ProductModel>, ProductValidator>();
            services.AddTransient<IValidator<UserModel>, UserValidator>();
            services.AddTransient<IValidator<CategoryModel>, CategoryValidator>();

            services.AddDbContext<WebApisContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("WebAPIsContext")));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1.0", new Info { Title = "Main API v1.0", Version = "v1.0" });
                // Swagger 2.+ support
                var security = new Dictionary<string, IEnumerable<string>>
            {
                    {"Bearer", new string[] { }},
            };
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(security);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCors(DefaultCorsPolicyName);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Versioned API v1.0");
                c.DocumentTitle = "Title Documentation";
                c.DocExpansion(DocExpansion.None);
            });
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
