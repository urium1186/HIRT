using HaloInfiniteResearchTools.Cli;
using HaloInfiniteResearchTools.Services;
using HaloInfiniteResearchTools.Services.Abstract;
using LibHIRT.Files;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools
{
    internal class EntryPoint
    {
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(uint dwProcessId);

        const uint ATTACH_PARENT_PROCESS = 0x0ffffffff;

        public static async Task<int> MainL(string[] args)
        {
            Console.WriteLine("Enabled cmd");
            return 0;
            HirtRootCommand? rootCommand = new HirtRootCommand();
            return await new CommandLineBuilder(rootCommand)
               .UseDefaults()
               .UseExceptionHandler(CommandLineExtensions.ExceptionHandler)
               .Build()
               .InvokeAsync(args);
        }

        static private IServiceProvider _serviceProvider;

        static public IServiceProvider ServiceProvider
        {
            get => _serviceProvider;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                
                AttachConsole(ATTACH_PARENT_PROCESS);
                Console.WriteLine("Enabled cmd");
                OnCliCOnsoleApp();
                HirtRootCommand? rootCommand = new HirtRootCommand();
                
                new CommandLineBuilder(rootCommand)
                   .UseDefaults()
                   .UseExceptionHandler(CommandLineExtensions.ExceptionHandler)
                   .Build()
                   .Invoke(args);
            }
            else
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }

        private static async void OnCliCOnsoleApp() {
            var services = new ServiceCollection();
            ConfigureDependencies(services);
            _serviceProvider = services.BuildServiceProvider();

            await _serviceProvider.GetRequiredService<IPreferencesService>().Initialize();
        }

        static private void ConfigureDependencies(IServiceCollection services)
        {
            ConfigureServices(services);
        }

        static private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHIFileContext, HIFileContext>();
            services.AddSingleton<IFileTypeService, FileTypeService>();
            services.AddSingleton<IMeshIdentifierService, MeshIdentifierService>();
            services.AddSingleton<IPreferencesService, PreferencesService>();
            services.AddTransient<ITextureConversionService, TextureConversionService>();
        }
    }
}
