﻿using DefaultArguments.Extensions;
using FindRobot.Interface;
using FindRobot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using ServiceFileCreator.Extensions;
using ServiceFileCreator.Model;
using System.Threading.Tasks;

namespace FindRobot
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)

                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.Sources.Clear();
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    })

                    .ConfigureServices((context, services) =>
                    {
                        services.AddAdamArgumentsParserTransient<ArgumentService>(args);

                        SettingsService options = new();
                        context.Configuration.GetRequiredSection("AppSettingsOptions").Bind(options);

                        services.AddSingleton<ISettingsService>(options);

                        services.AddLogging(loggingBuilder =>
                        {
                            Logger logger = new LoggerConfiguration()
                                    .ReadFrom.Configuration(context.Configuration)
                                    .CreateLogger();

                            loggingBuilder.ClearProviders();
                            loggingBuilder.AddSerilog(logger, dispose: true);
                        });

                        services.AddAdamServiceFileCreator();
                        services.AddHostedService<FindRobotService>();      
                    })

                    .Build();

            host.UseAdamServiceFileCreator(projectType: ProjectType.DotnetProject);   
            await host.ParseAndRunAsync();

        }
    }
}
