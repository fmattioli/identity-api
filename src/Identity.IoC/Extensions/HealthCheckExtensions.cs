﻿using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using SpendManagement.Identity.IoC.Models;

namespace SpendManagement.Identity.IoC.Extensions
{
    public static class HealthCheckExtensions
    {
        private static readonly string[] tags = ["db", "data"];

        public static IServiceCollection AddHealthCheckers(this IServiceCollection services, SqlServerSettings? settings)
        {
            if (settings?.ConnectionString is not null)
            {
                services
                    .AddHealthChecks()
                    .AddNpgSql(settings.ConnectionString, name: "Postgres", tags: tags);

                services
                    .AddHealthChecksUI()
                    .AddInMemoryStorage();
            }

            return services;
        }

        public static IApplicationBuilder UseHealthCheckers(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI(options => options.UIPath = "/monitor");

            return app;
        }
    }
}
