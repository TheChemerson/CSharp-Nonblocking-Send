/***********************************************************************
 *
 * Example application to demonstrate non-blocking send using Solace's
 * .NET API.
 * 
 * Once the send buffer is full in non-blocking mode, the API will not 
 * send the message but return immediately with CLIENT_WOULD_BLOCK.
 * When the underlying buffer has available space, the session will
 * raise an event to signal it is ready to receive more messages.
 **********************************************************************/

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]
namespace PublisherNonBlocking {

    public class Program {

        static int Main(string[] args) {
            var host = CreateHostBuilder(args).Build();

            Console.WriteLine("Press Ctrl+C or Enter \"exit\" then press return to terminate.");
            using (var serviceScope = host.Services.CreateScope()) {
                var controller = serviceScope.ServiceProvider.GetRequiredService<Controller>();
                controller.Init();
                controller.SendMessageList();

                // keep the controller alive until it is time to go home
                while (Console.ReadLine().ToLower() != "exit") {
                    Thread.Sleep(1000);
                }
            }

            return 0;
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(config => {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddCommandLine(args);
                })
                .ConfigureLogging(builder => {
                    builder.ClearProviders();
                    builder.AddLog4Net();
                })
                .ConfigureServices((_, services) =>
                    services.AddTransient<Controller>()
                );

    }

}