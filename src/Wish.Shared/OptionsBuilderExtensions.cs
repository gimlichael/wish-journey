using System;
using Cuemon;
using Cuemon.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Wish.Shared
{
    public static class OptionsBuilderExtensions
    {
        public static OptionsBuilder<TOptions> ConfigureTriple<TOptions>(this OptionsBuilder<TOptions> builder, Action<TOptions> setup) where TOptions : class, IParameterObject, new()
        {
            Validator.ThrowIfInvalidConfigurator(setup, out var options);
            builder.Configure(setup); // support for IOptions<TOptions>
            builder.Services.TryAddSingleton(setup); // support for Action<TOptions>
            builder.Services.TryAddSingleton(options); // support for TOptions
            return builder;
        }
    }
}
