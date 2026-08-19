using Ampere.Application.Common.Abstractions;
using Ampere.Application.Common.Contracts;
using Ampere.Application.Common.Pipeline.Validation;
using Ampere.Application.Identity.Abstractions;
using Ampere.Application.Identity.Handlers;
using Ampere.Application.Identity.Validators;
using Ampere.Application.Setup.Abstractions;
using Ampere.Infrastructure.Common.Repository;
using Ampere.Infrastructure.Common.UnitOfWork;
using Ampere.Infrastructure.Identity.Models;
using Ampere.Infrastructure.Identity.Options;
using Ampere.Infrastructure.Identity.Services;
using Ampere.Infrastructure.Persistence;
using Ampere.Infrastructure.Persistence.Middlewares;
using Ampere.Infrastructure.Setup.Services;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Ampere.Composition.Extensions;

/// <summary>Adds Ampere services to the web host.</summary>
public static class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        /// <summary>Runs the Ampere web application.</summary>
        /// <typeparam name="TProgram">The program type.</typeparam>
        /// <typeparam name="TApp">The root component.</typeparam>
        /// <returns>A task for application startup.</returns>
        public async Task RunAmpereAsync<
            TProgram,
            TApp>()
            where TProgram : class
            where TApp : IComponent
        {
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddMudServices();
            builder.Services.Configure<JwtOptions>(
                builder.Configuration.GetSection(
                    JwtOptions.SectionName));

            builder.Services.AddDbContext<AmpereDbContext>(
                options => options.UseSqlServer(
                    builder.Configuration
                        .GetConnectionString("Ampere"),
                    sql => sql.CommandTimeout(90)));

            builder.Services.AddIdentityCore<User>(
                options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = false;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(15);
                })
                .AddRoles<Role>()
                .AddEntityFrameworkStores<AmpereDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddSingleton<IRevokedTokenStore, RevokedTokenStore>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IIdentityEmailSender, LoggingIdentityEmailSender>();
            builder.Services.AddScoped<ISystemSetupService, SystemSetupService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    JwtOptions jwt = builder.Configuration
                        .GetSection(JwtOptions.SectionName)
                        .Get<JwtOptions>()
                        ?? throw new InvalidOperationException(
                            "JWT configuration is missing.");

                    if (Encoding.UTF8.GetByteCount(jwt.Key) < 32)
                    {
                        throw new InvalidOperationException(
                            "Jwt:Key must contain 256 bits.");
                    }

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        jwt.Key)),
                            ValidateIssuer = true,
                            ValidIssuer = jwt.Issuer,
                            ValidateAudience = true,
                            ValidAudience = jwt.Audience,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.FromSeconds(30)
                        };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            JwtSecurityToken? token =
                                context.SecurityToken
                                    as JwtSecurityToken;
                            string? tokenType = token?
                                .Claims
                                .FirstOrDefault(
                                    claim => claim.Type ==
                                        JwtRegisteredClaimNames.Typ)
                                ?.Value;

                            if (!string.Equals(tokenType, "access", StringComparison.Ordinal))
                            {
                                context.Fail("The token is not an access token.");
                                return Task.CompletedTask;
                            }

                            if (token is not null && 
                                context.HttpContext
                                .RequestServices
                                .GetRequiredService<IRevokedTokenStore>()
                                .IsRevoked(token.Id))
                            {
                                context.Fail(
                                    "The access token "
                                    + "has been revoked.");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy(
                    IdentityPolicies.Administrator,
                    policy => policy.RequireClaim(
                        IdentityClaimTypes.Permission,
                        "system.admin"))
                .AddPolicy(
                    IdentityPolicies.User,
                    policy => policy.RequireClaim(
                        IdentityClaimTypes.Permission,
                        "system.user"));

            builder.Services.AddMediator(options =>
            {
                options.ServiceLifetime =
                    ServiceLifetime.Scoped;
                options.Assemblies =
                    [typeof(IdentityHandlers).Assembly];
                options.PipelineBehaviors =
                [
                    typeof(ValidationMiddleware<,>),
                    typeof(TransactionMiddleware<,>)
                ];
            });

            await builder.Build().RunAmpereAsync<TApp>();
        }
    }
}
