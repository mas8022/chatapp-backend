using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using backend.common.utils;
using backend.model;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace backend.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy("frontend", policy =>
                {
                    policy.WithOrigins(configuration.GetConnectionString("FRONTEND_URL")!)
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Service")))
            .AsSelf()
            .WithScopedLifetime()
            );

            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            services.AddHttpClient();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DB_URL")));

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var options = ConfigurationOptions.Parse(configuration.GetConnectionString("REDIS_URI")!);
                options.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(options);
            });
            services.AddScoped<IDatabase>(sp =>
                sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

            services.AddScoped<GenerateUniqueUsername>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration.GetConnectionString("JWT_SECRET_KEY")!)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration.GetConnectionString("JWT_ISSUER"),
                    ValidateAudience = true,
                    ValidAudience = configuration.GetConnectionString("JWT_AUDIENCE"),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                     {
                         var accessToken = context.Request.Cookies["access_token"]
                                           ?? context.Request.Query["access_token"].ToString();

                         if (!string.IsNullOrEmpty(accessToken))
                         {
                             context.Token = accessToken;
                         }

                         return Task.CompletedTask;
                     }
                };
            });

            services.AddSignalR();

            services.AddHttpContextAccessor();

            return services;

        }
    }
}