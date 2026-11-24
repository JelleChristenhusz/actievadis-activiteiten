using Microsoft.Extensions.DependencyInjection;
using Application.Services.Interfaces;
using Application.Services.Implementations;

namespace Application.Extensions
{
    /// <summary>
    /// Extension methods for registering application services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>Registers application use cases and authentication services.</summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IVisitorRegistrationService, VisitorRegistrationService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAdministratorBootstrapService, AdministratorBootstrapService>();
            services.AddScoped<IAccessTokenValidator, AccessTokenValidator>();

            return services;
        }
    }
}
