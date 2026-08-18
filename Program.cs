using ERP.Middleware;
using ERP.Repository;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Data;
using ERP.Repository.Data.OrderData;
using ERP.Repository.Data.ProductData;
using ERP.Repository.Data.UserAccounts;
using ERP.Repository.Interface.Data;
using ERP.Repository.Interface.Data.OrderData;
using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Interface.Data.UserAccountData;
using ERP.Repository.Interface.Order;
using ERP.Repository.Interface.Product;
using ERP.Repository.Interface.TokenManager;
using ERP.Repository.Interface.UserAccounts;
using ERP.Repository.Services.Order;
using ERP.Repository.Services.Product;
using ERP.Repository.Services.TokenManager;
using ERP.Repository.Services.UserAccounts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ERP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontEnd", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // Add services to the container.

            #region Scoped Services

            #region Business Services

            builder.Services.AddScoped<IUserAccountService, UserAccountService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            #endregion

            #region Data Services
            builder.Services.AddScoped<IUserAccountData, UserAccountData>();
            builder.Services.AddScoped<IProductData, ProductData>();
            builder.Services.AddScoped<ICategoryData, CategoryData>();
            builder.Services.AddScoped<IOrderData, OrderData>();
            builder.Services.AddScoped<IBaseData, BaseData>();
            #endregion

            #endregion

            #region Singleton Services

            builder.Services.AddSingleton<ITokenManagerService,TokenManagerService>();
            builder.Services.AddSingleton<ResponseHelper>();

            #endregion

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                    RoleClaimType = ClaimTypes.Role
                };


                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.HttpContext.Request.Cookies["AccessToken"];
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });


                    builder.Services.AddDbContext<DatabaseContext>(context => context.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseCors("FrontEnd");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseMiddleware<RequestMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}
