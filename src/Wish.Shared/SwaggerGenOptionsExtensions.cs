using Cuemon.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Wish.Shared
{
    public static class SwaggerGenOptionsExtensions
    {
        public static SwaggerGenOptions AddBasicAuthenticationSecurity(this SwaggerGenOptions options)
        {
            options.AddSecurityDefinition(HttpAuthenticationSchemes.Basic, new OpenApiSecurityScheme
            {
                Description = $"Protects an API by adding an {HttpHeaderNames.Authorization} header using the {HttpAuthenticationSchemes.Basic} scheme.",
                Name = HttpHeaderNames.Authorization,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = HttpAuthenticationSchemes.Basic.ToLowerInvariant()
            });

            var basicRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = HttpAuthenticationSchemes.Basic
                        },
                        In = ParameterLocation.Header
                    },
                    Array.Empty<string>()
                }
            };

            options.AddSecurityRequirement(basicRequirement);
            return options;
        }
    }
}
