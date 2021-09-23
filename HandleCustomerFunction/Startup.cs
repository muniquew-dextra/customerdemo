using CustomerCore.Data;
using Serilog;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.Extensibility;

[assembly: FunctionsStartup(typeof(HandleCustomerFunction.Startup))]
namespace HandleCustomerFunction
{
    public class Startup : FunctionsStartup
    {
        public Startup()
        {
            // Initialize serilog logger
            Log.Logger = new LoggerConfiguration()
                     .WriteTo
                        .ApplicationInsights(TelemetryConfiguration.CreateDefault(), TelemetryConverter.Traces)
                     .WriteTo
                        .Console()
                     .MinimumLevel.Information()
                     .Enrich.FromLogContext()
                     .CreateLogger();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services).BuildServiceProvider(true);
        }

        private IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IConnectionFactory, ConnectionFactory>();
            services.AddTransient<IBaseRepository, BaseRepository>();
            services
                .AddLogging(loggingBuilder =>
                    loggingBuilder.AddSerilog(dispose: true)
                );

            return services;
        }
    }
}
