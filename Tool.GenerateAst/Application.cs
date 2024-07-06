using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Tool.GenerateAst
{
    public class Application
    {
        public IReadOnlyList<string> Arguments { get; }

        private readonly AstBuilder builder;
        private readonly ILogger logger;

        public Application(string[] args)
        {
            Arguments = new List<string>(args);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            logger = serviceProvider.GetRequiredService<ILogger<Application>>();
            builder = serviceProvider.GetRequiredService<AstBuilder>();
        }

        private static void ConfigureServices(IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.AddSingleton<ILoggerFactory, LoggerFactory>();
            serviceDescriptors.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            serviceDescriptors.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", false)
                    .Build();

                serviceDescriptors.AddSingleton(configuration);
            }
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("treeconfiguration.json")
                    .Build();

                TreeConfiguration treeConfiguration = new TreeConfiguration();
                configuration.Bind(treeConfiguration);
                serviceDescriptors.AddSingleton(treeConfiguration);
            }
            serviceDescriptors.AddSingleton<AstBuilder>();
        }

        public int Execute()
        {
            switch (Arguments.Count)
            {
                case 1:
                    if (!builder.BuildTree(Arguments[0]))
                    {
                        return 65;
                    }
                    break;

                default:
                    logger.LogError("usage: Tool.GenerateAst <Directory>");
                    logger.LogError("       <Directory> Directory to create the code for the tree in");
                    return 64;
            }
            return 0;
        }
    }
}
