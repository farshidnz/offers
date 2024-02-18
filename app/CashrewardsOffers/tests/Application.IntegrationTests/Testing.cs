using CashrewardsOffers.API;
using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Application.IntegrationTests.TestingHelpers;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Infrastructure.Identity;
using CashrewardsOffers.Infrastructure.Persistence;
using Dapper;
using MassTransit;
using MassTransit.Mediator;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Respawn;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static CashrewardsOffers.API.Startup;

[SetUpFixture]
public class Testing
{
    private static IConfigurationRoot _configuration;
    private static IServiceScopeFactory _scopeFactory;
    private static InMemoryTestHarness _harness;
    private static Checkpoint _checkpoint;
    private static string _currentUserId;

    [OneTimeSetUp]
    public async Task TryRunBeforeAnyTests()
    {
        try
        {
            await RunBeforeAnyTests();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task RunBeforeAnyTests()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(new LoggingLevelSwitch(LogEventLevel.Information))
            .Filter.ByExcluding(log => log.Exception != null)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        var configs = new Dictionary<string, string>
        {
            ["DocumentDbDatabseName"] = $"integratrion-testdb-{Guid.NewGuid()}",
            ["UseTransactions"] = "false"
        };

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"{Assembly.Load("CashrewardsOffers.API").Folder()}/appsettings.json", true)
            .AddJsonFile($"{Assembly.Load("CashrewardsOffers.API").Folder()}/appsettings.Development.json", true)
            .AddInMemoryCollection(configs)
            .AddEnvironmentVariables();

        _configuration = builder.Build();

        var startup = new Startup(_configuration);

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(_configuration);
        services.AddControllers();
        services.AddSingleton(Mock.Of<IWebHostEnvironment>(w =>
            w.EnvironmentName == "Development" &&
            w.ApplicationName == "CashrewardsOffers.WebUI"));

        startup.ConfigureServices(services);
        services.AddLogging();

        ReplaceServices(services);

        SqlMapper.AddTypeHandler(new MySqlStringTypeHandler());

        services.AddMassTransitInMemoryTestHarness(cfg =>
        {
            cfg.AddConsumers(Assembly.Load("CashrewardsOffers.Application"));
        });

        var provider = services.BuildServiceProvider();
        _scopeFactory = provider.GetService<IServiceScopeFactory>();

        _checkpoint = new Checkpoint
        {
            TablesToIgnore = new[] { "__EFMigrationsHistory" }
        };

        EnsureDatabase();

        _harness = provider.GetRequiredService<InMemoryTestHarness>();
        await _harness.Start();
    }

    private static void ReplaceServices(ServiceCollection services)
    {
        ReplaceCurrentUserServices(services);
        services.AddSingleton<IMongoClientFactory, MongoInMemoryClientFactory>();
    }

    private static void ReplaceCurrentUserServices(ServiceCollection services)
    {
        services.Remove(services.FirstOrDefault(d => d.ServiceType == typeof(ICurrentUserService)));
        services.AddTransient(provider => Mock.Of<ICurrentUserService>(s => s.UserId == _currentUserId));
    }

    private static void EnsureDatabase()
    {
    }

    public static IServiceScope GivenScope()
    {
        return _scopeFactory.CreateScope();
    }

    public static async Task GivenOffer(Offer offer)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetService<IOffersPersistenceContext>();

        var existingOffer = await dbContext.GetOffer((int)offer.Client, (int?)offer.PremiumClient, offer.OfferId);
        if (existingOffer == null)
        {
            await dbContext.InsertOffer(offer);
        }
    }

    public static async Task GivenFeature(string feature, string cognitoId)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetService<IFeaturePersistenceContext>();

        await dbContext.Enrol(feature, cognitoId);
    }

    public static IServiceScopeFactory ScopeFactory => _scopeFactory;

    public static async Task SendCommand<T>(T request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetService<IMediator>();

        await mediator.Send(request);
    }

    public static async Task<TResponse> SendQuery<T, TResponse>(T query) where T : class
                                                                         where TResponse : class
    {
        using var scope = _scopeFactory.CreateScope();

        var bus = scope.ServiceProvider.GetService<IBus>();

        var client = bus.CreateRequestClient<T>();

        var response = await client.GetResponse<TResponse>(query);

        return response.Message;
    }

    public static async Task<string> RunAsDefaultUserAsync()
    {
        return await RunAsUserAsync("test@local", "Testing1234!", new string[] { });
    }

    public static async Task<string> RunAsAdministratorAsync()
    {
        return await RunAsUserAsync("administrator@local", "Administrator1234!", new[] { "Administrator" });
    }

    public static async Task<string> RunAsUserAsync(string userName, string password, string[] roles)
    {
        using var scope = _scopeFactory.CreateScope();

        var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser { UserName = userName, Email = userName };

        var result = await userManager.CreateAsync(user, password);

        if (roles.Any())
        {
            var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            await userManager.AddToRolesAsync(user, roles);
        }

        if (result.Succeeded)
        {
            _currentUserId = user.Id;

            return _currentUserId;
        }

        var errors = string.Join(Environment.NewLine, result.ToApplicationResult().Errors);

        throw new Exception($"Unable to create {userName}.{Environment.NewLine}{errors}");
    }

    public static async Task ResetState()
    {
        using var scope = _scopeFactory.CreateScope();
        await scope.ServiceProvider.GetService<IDocumentPersistenceContext<OfferDocument>>().DropCollection();
        await scope.ServiceProvider.GetService<IDocumentPersistenceContext<FeatureDocument>>().DropCollection();
        await scope.ServiceProvider.GetService<IMongoLockService>().ReleaseAllLocks();
        scope.ServiceProvider.GetService<IMongoLockService>().Migrate();
        _currentUserId = null;
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        await _harness.Stop();
    }
}
