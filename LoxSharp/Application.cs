using LoxSharp.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LoxSharp
{
    public class Application
    {
        public IReadOnlyList<string> Arguments { get; }

        private readonly Runtime runtime;
        private readonly ILogger logger;

        public Application(string[] args)
        {
            Arguments = new List<string>(args);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            logger = serviceProvider.GetRequiredService<ILogger<Application>>();
            runtime = serviceProvider.GetRequiredService<Runtime>();
        }

        private static void ConfigureServices(IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.AddSingleton<ILoggerFactory, LoggerFactory>();
            serviceDescriptors.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            serviceDescriptors.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .Build();

            serviceDescriptors.AddSingleton(configuration);
            serviceDescriptors.AddSingleton<Runtime>();
            serviceDescriptors.AddSingleton<AstPrinter>();
            serviceDescriptors.AddSingleton<Interpreter>();
        }

        public int Execute()
        {
            switch (Arguments.Count)
            {
                case 0:
                    runtime.RunPrompt();
                    break;

                case 1:
                    switch (runtime.RunFile(Arguments[0]))
                    {
                        case RuntimeResult.InputError:
                            return 65;
                        case RuntimeResult.RuntimeError:
                            return 70;
                    }
                    break;

                default:
                    logger.LogError("usage: LoxSharp <File>");
                    logger.LogError("       <File> Script to run (optional)");
                    logger.LogError("When using without parameters, live compilation is availabe");
                    return 64;
            }
            return 0;
        }
    }
}
