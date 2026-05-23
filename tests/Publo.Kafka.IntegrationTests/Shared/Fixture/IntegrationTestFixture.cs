using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Publo.Abstraction.Extensions;
using Publo.Kafka.Extensions;
using Publo.Kafka.IntegrationTests.Cases;

namespace Publo.Kafka.IntegrationTests.Shared.Fixture;

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
                .AddPublo(opt => opt.UseKafka());

            services
                .AddPubloExecutor<CommonHandlerTests.Event, CommonHandlerTests.Executor>()
                .AddPubloExecutor<DoubleServicesHandlerTests.Event, DoubleServicesHandlerTests.Executor>()
                .AddPubloExecutor<MultipleMessagesHandlerTests.Event, MultipleMessagesHandlerTests.Executor>();
        }

        public void Configure(IApplicationBuilder app)
        {
        }
    }
}
