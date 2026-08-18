using Ampere.Application.Common.Contracts;
using Ampere.Application.Common.Pipeline.Validation;
using Ampere.Application.Identity.Abstractions;
using Ampere.Application.Identity.Handlers;
using Ampere.Application.Identity.Validators;
using Ampere.Application.Setup.Abstractions;
using Ampere.Infrastructure.Identity.Models;
using Ampere.Infrastructure.Identity.Options;
using Ampere.Infrastructure.Identity.Services;
using Ampere.Infrastructure.Persistence;
using Ampere.Infrastructure.Setup.Services;
using FluentValidation;
using Mediator.Net.MicrosoftDependencyInjection;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
namespace Ampere.Composition.Extensions;
/// <summary> Class to add extension methods and properties to <see cref="WebApplicationBuilder"/>.</summary>
public static class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        /// Run partial extension method to perform DI setup
        /// </summary>
        /// <typeparam name="T">App.razor type;</typeparam>
        /// <remarks>The last line must call build and call the partial run extension method.</remarks>
        public async Task RunAmpereAsync<T>() where T : Microsoft.AspNetCore.Components.IComponent
        {
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddMudServices();
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

            builder.Services.AddDbContext<AmpereDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Identity")));

            builder.Services.AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            }).AddRoles<Role>()
                .AddEntityFrameworkStores<AmpereDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddScoped<IMessageValidator, FluentMessageValidator>();
            builder.Services.AddSingleton<IRevokedTokenStore, RevokedTokenStore>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IIdentityEmailSender, LoggingIdentityEmailSender>();
            builder.Services.AddScoped<ISystemSetupService, SystemSetupService>();
            builder.Services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    JwtOptions jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                        ?? throw new InvalidOperationException("JWT builder.Configuration is missing.");

                    if (Encoding.UTF8.GetByteCount(jwt.Key) < 32)
                    {
                        throw new InvalidOperationException("Jwt:Key must contain at least 256 bits.");
                    }

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
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
                            var token = context.SecurityToken as JwtSecurityToken;
                            var tokenType = token?.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Typ)?.Value;
                            if (!string.Equals(tokenType, "access", StringComparison.Ordinal))
                            {
                                context.Fail("The supplied token is not an access token.");
                                return Task.CompletedTask;
                            }

                            if (token is not null && context.HttpContext.RequestServices.GetRequiredService<IRevokedTokenStore>().IsRevoked(token.Id))
                            {
                                context.Fail("The supplied token has been revoked.");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy(IdentityPolicies.Administrator, policy =>
                    policy.RequireClaim(IdentityClaimTypes.Permission,
                    AdministratorPermission))
                .AddPolicy(IdentityPolicies.Manager, policy =>
                    policy.RequireClaim(IdentityClaimTypes.Permission,
                    ManagerPermission))
                .AddPolicy(IdentityPolicies.User, policy =>
                    policy.RequireClaim(IdentityClaimTypes.Permission,
                    UserPermission));
            var mb = new Mediator.Net.MediatorBuilder();
            mb.RegisterHandlers(typeof(IdentityHandlers).Assembly)
                .ConfigureCommandReceivePipe(pipe => pipe.UseValidation())
                .ConfigureRequestPipe(pipe => pipe.UseValidation());

            builder.Services.RegisterMediator(mb);

            await builder.Build().RunAmpereAsync<T>();
        }
    }   
    private const string AdministratorPermission = "system.admin";
    private const string ManagerPermission = "system.manager";
    private const string UserPermission = "system.user";
}
