using System;
using ConnectGame.Runner.Configuration;
using ConnectGame.Runner.Engines;
using ConnectGame.Runner.Game;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;

namespace ConnectGame.Runner
{
    class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureSerilog();
            services.AddLogging(ConfigureLogging);

            services.AddSingleton<IConfigurationLoader, ConfigurationLoader>();
            services.AddSingleton<ITournamentRunner, TournamentRunner>();
            services.AddTransient<IEngine, Engine>();
            services.AddTransient<IEngineHandlerManager, EngineHandlerManager>();
            services.AddTransient<IEngineProcess, EngineProcess>();
            services.AddSingleton<IBoardSerializer, BoardSerializer>();
            services.AddSingleton<IWinDetector, WinDetector>();
            services.AddTransient<IMatchTimeControl, MatchTimeControl>();

            services.AddTransient<RootConfig>(provider => ConfigurationHolder.Config);
            services.AddTransient<TimeControlConfig>(provider => provider.GetRequiredService<RootConfig>().TimeControl);
        }

        private static void ConfigureSerilog()
        {
            SelfLog.Enable(Console.Out);
            var loggerConfiguration = new LoggerConfiguration();
            //const string consoleTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            const string consoleTemplate = "[{Timestamp:HH:mm:ss}] {EngineId} {Message:lj}{NewLine}{Exception}";
            //const string consoleTemplate = "[{Timestamp:HH:mm:ss}] {Message:lj}{NewLine}{Exception}";
            loggerConfiguration.WriteTo.Console(outputTemplate: consoleTemplate);
            loggerConfiguration.MinimumLevel.Information();
//            loggerConfiguration.MinimumLevel.Debug();
            Log.Logger = loggerConfiguration.CreateLogger();
        }

        private void ConfigureLogging(ILoggingBuilder builder)
        {
            builder.ClearProviders();
            builder.AddSerilog();
            builder.SetMinimumLevel(LogLevel.Debug);
        }
    }
}