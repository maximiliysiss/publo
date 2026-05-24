using System;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Publo.Abstraction.Extensions;
using Publo.Postgres.Extensions;
using Publo.Postgres.Infrastructure.Database;
using Publo.Postgres.IntegrationTests.Cases;
using Publo.Postgres.IntegrationTests.Repositories;

namespace Publo.Postgres.IntegrationTests.Shared.Fixture;

public sealed class IntegrationTestFixture : WebApplicationFactory<IntegrationTestFixture.Startup>
{
    protected override IHostBuilder? CreateHostBuilder() => Host.CreateDefaultBuilder();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseStartup<Startup>()
            .UseContentRoot(Directory.GetCurrentDirectory());
    }

    public sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddPublo(opt => opt.UseNpgsql<ConnectionFactory>());

            services
                .AddPubloExecutor<CommonHandlerTests.Event, CommonHandlerTests.Executor>()
                .AddPubloExecutor<DoubleServicesHandlerTests.Event, DoubleServicesHandlerTests.Executor>()
                .AddPubloExecutor<MultipleMessagesHandlerTests.Event, MultipleMessagesHandlerTests.Executor>()
                .AddPubloExecutor<PostgresRepositoryTests.TestEvent, PostgresRepositoryTests.Executor>();
        }

        public void Configure(IApplicationBuilder app)
        {
        }
    }

    private sealed class ConnectionFactory(IConfiguration configuration) : IConnectionFactory
    {
        public string GetConnectionString()
            => configuration.GetConnectionString("Postgres") ??
               throw new InvalidOperationException("Postgres connection string not found in configuration");

        public Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
            => Task.FromResult<DbConnection>(new NpgsqlConnection(GetConnectionString()));
    }
}
