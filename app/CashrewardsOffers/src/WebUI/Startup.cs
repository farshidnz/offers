using CashrewardsOffers.API.Extensions;
using CashrewardsOffers.API.Middlewares;
using CashrewardsOffers.API.Services;
using CashrewardsOffers.Application;
using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.Feature.Services;
using CashrewardsOffers.Application.Merchants.Services;
using CashrewardsOffers.Application.MerchantSuggestions.Mappings;
using CashrewardsOffers.Application.MerchantSuggestions.Services;
using CashrewardsOffers.Application.Offers.Services;
using CashrewardsOffers.Infrastructure;
using CashrewardsOffers.Infrastructure.Persistence;
using Dapper;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using Unleash;

namespace CashrewardsOffers.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddControllers();

            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

            services.AddSingleton<ICurrentUserService, CurrentUserService>();

            services.AddHttpContextAccessor();

            // Customise default API behaviour
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddAndConfigureApiVersioning();

            services.AddAndConfigureSwagger();

            services.AddAndConfigureCognitoAuthentication(Configuration);

            services.AddApplication();

            services.AddInfrastructure(Configuration);

            services.AddSingleton<IUnleash>(_ => new DefaultUnleash(new UnleashSettings()
            {
                AppName = "ofers-ms-client",
                Environment = Configuration["ENVIRONMENT"],
                UnleashApi = new Uri(Configuration["UnleashUrl"]),
                FetchTogglesInterval = TimeSpan.FromSeconds(30),
                CustomHttpHeaders = new Dictionary<string, string>()
                {
                    ["Authorization"] = Configuration["UnleashApiKey"]
                }
            }));
            services.AddSingleton<IUnleashService, UnleashService>();
            services.AddSingleton<IMongoClientFactory, MongoClientFactory>();
            services.AddSingleton<ISyncOffersService, SyncOffersService>();
            services.AddSingleton<ISyncRankedMerchantService, SyncRankedMerchantService>();
            services.AddSingleton<IMerchantSuggestionMapper, MerchantSuggestionMapper>();
            services.AddSingleton<IRankedMerchantRandomizationService, RankedMerchantRandomizationService>();
            services.AddSingleton<ISyncMerchantsService, SyncMerchantsService>();
            
            try
            {
                //ConfigureHangFire(services);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to start hangfire - ", ex);
            }

            AnzItemConfigService.Setup(Configuration["ENVIRONMENT"], Configuration["ANZ_TRACKING_PARAM"]);

            services.AddHealthChecks();
        }

        private void ConfigureHangFire(IServiceCollection services)
        {
            var mongoClient = new MongoClientFactory(Configuration).CreateClient();

            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMongoStorage(mongoClient, Configuration["DocumentDbDatabseName"], new MongoStorageOptions
                {
                    Prefix = "hangfire.mongo",
                    CheckConnection = true,
                    /* Warning, the below skips migrations because of DocumentDb not supporting Mongodb "capped collections"
                        If migrations strategies are used, you'd get an Exception regarding "Not supported capped:true".
                        Therefore, migrations are bypassed for now.
                        If the nuget library 'Hangfire.Mongodb' is upgraded and the lib's expected schema is different, 
                        then without migration strategies, there will be problems. So PLEASE CHECK before upgrading the nuget package.
                    */
                    ByPassMigration = true
                })
            );

            // services.AddHangfireServer(serverOptions =>
            // {
            //     serverOptions.ServerName = "Hangfire.Mongo server 1";
            // });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UsePathBase("/api/offers");
            app.UseSerilogRequestLogging();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                foreach (var desc in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"../swagger/{desc.GroupName}/swagger.json", $"API v{desc.ApiVersion}");
                }
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health-check");
            });

            // app.UseHangfireDashboard();

            SqlMapper.AddTypeHandler(new MySqlStringTypeHandler());
        }

        public class MySqlStringTypeHandler : SqlMapper.TypeHandler<string>
        {
            public override void SetValue(IDbDataParameter parameter, string value)
            {
                parameter.Value = value.ToString();
            }

            public override string Parse(object value)
            {
                return value.ToString();
            }
        }
    }
}
