using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StreamDeckLib.DependencyInjection;
using StreamDeckLib.Hosting;
using tv.tavernfire.spotify.models;

namespace tv.tavernfire.spotify
{
    class Program
    {
        static void Main(string[] args)
        {
            Debugger.Launch();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            
            Host.CreateDefaultBuilder(args)
                .ConfigureStreamDeckToolkit(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddStreamDeck(hostContext.Configuration, typeof(Program).Assembly);
                    services.AddSpotify();
                });
    }

    static class SpotifyExtenstions
    {
        public static IServiceCollection AddSpotify(this IServiceCollection services)
        {
            services.AddSingleton<ITVGlobalSettings, SpotifySettingsModel>();
            services.AddSingleton<SpotifySettingsModel>( new SpotifySettingsModel());
            services.AddSingleton<ISpotifyClientFactory, SpotifyClientFactory>();
            return services;
        }
    }
}
