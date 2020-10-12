using System;
using System.IO;
using System.Threading.Tasks;
using JV.Lib.CashManagement;
using JV.Lib.FinancialManagement;
using JV.Lib.Integrations;
using JV.Lib.ResourceManagement;
using JV.Lib.RevenueManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;


namespace Driver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(cb =>
                {
                    cb.SetBasePath(Directory.GetCurrentDirectory())
                      .AddCommandLine(args)
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                      .AddJsonFile("appsettings.secrets.json", optional: false, reloadOnChange: false);
                })
                .ConfigureLogging(lb =>
                {
                    Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                    .WriteTo.Console(LogEventLevel.Information)
                    //.WriteTo.Console(new JsonFormatter(), LogEventLevel.Information)
                    .CreateLogger();

                    lb.ClearProviders();
                    lb.AddSerilog();
                })
                .ConfigureServices((hc, svc) =>
                {
                    svc.AddTransient<Microsoft.Extensions.Logging.ILogger>(sp => sp.GetRequiredService<ILogger<Worker>>());
                    svc.AddOptions<WorkdayCredentialsOptions>()
                    .Bind(hc.Configuration.GetSection(WorkdayCredentialsOptions.Section))
                    .ValidateDataAnnotations();

                    svc.AddAuthenticatedSoapClient<Cash_ManagementPort, Cash_ManagementPortClient>();
                    svc.AddAuthenticatedSoapClient<Financial_ManagementPort, Financial_ManagementPortClient>();
                    svc.AddAuthenticatedSoapClient<IntegrationsPort, IntegrationsPortClient>();
                    svc.AddAuthenticatedSoapClient<Revenue_ManagementPort, Revenue_ManagementPortClient>();
                    svc.AddAuthenticatedSoapClient<Resource_ManagementPort, Resource_ManagementPortClient>();

                    // TODO(cspital) need a reflection based thing to automatically register all impls of IPocTask


                    svc.AddHostedService<Worker>();
                })
                .RunConsoleAsync();
            }
            catch (Exception e)
            {
                Log.Error(e, "Top level error");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
