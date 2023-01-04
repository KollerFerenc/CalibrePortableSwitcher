using System;
using System.IO;
using System.Reflection;
using Serilog;
using CommandDotNet;

namespace CalibrePortableSwitcher;

class Program
{
    internal static readonly string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    static void Main(string[] args)
    {
        SetCurrentDirectory();
        ConfigureLogger();

        var appRunner = new AppRunner<CalibrePortableSwitcher>()
            .UseDefaultMiddleware();
        appRunner.AppSettings.DefaultArgumentMode = ArgumentMode.Option;
        appRunner.Run(args);

        Exit(ErrorCode.NoError);
    }

    private static void ConfigureLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(@"logs\log-.txt",
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
                rollingInterval: RollingInterval.Year,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private static void SetCurrentDirectory()
    {
        string executingAssemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

        if (Environment.CurrentDirectory != executingAssemblyDirectory)
        {
            Environment.CurrentDirectory = executingAssemblyDirectory;
        }
    }

    internal static void Exit(ErrorCode errorCode)
    {
        Log.CloseAndFlush();
        Environment.Exit((int)errorCode);
    }
}