using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using tv.tavernfire.spotify.models;
using static SpotifyAPI.Web.Scopes;


namespace tv.tavernfire.spotify
{
    public interface ISpotifyClientFactory : INotifyPropertyChanged
    {
        public SpotifySettingsModel Settings { get; set; }
        SpotifyClient Client { get; }
        Task<int> Main();
    }
    public class SpotifyClientFactory : ISpotifyClientFactory
    {
        public ILogger<SpotifyClientFactory> Logger { get; internal set; }
        private SpotifySettingsModel _settings;
        public SpotifySettingsModel Settings
        {
            get => _settings;
            set
            {
                if (value == _settings) return;
                if(_settings!=null) _settings.PropertyChanged -= SettingChanged;
                _settings = value;
                _settings.PropertyChanged += SettingChanged;
                OnPropertyChanged(nameof(Settings));
                
                void SettingChanged(object sender, PropertyChangedEventArgs args) => OnPropertyChanged(nameof(Settings));
            }
        }

        private static readonly EmbedIOAuthServer _server =
            new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);

        public SpotifyClient _client;

        public SpotifyClientFactory(SpotifySettingsModel settings, ILogger<SpotifyClientFactory> logger)
        {
            Logger = logger;
            Settings = settings;
            var list = new List<string>();
        }

        public SpotifyClient Client => _client;

        public async Task<int> Main()
        {
            Logger.Log(LogLevel.Information, "Starting Spotify Service");
            if (!Settings.Initialized) return 1;
            Logger.Log(LogLevel.Information, $"Spotify Settings Initialized: {Settings}");
            if (!Settings.Ready)
                await StartAuthentication();
            else
               _client = await CreateClientWithToken();

            return 0;
        }

        private async Task<SpotifyClient> CreateClientWithToken()
        {
            var token = LoadToken();
            Logger.Log(LogLevel.Information, $"Resuming using token: {token}");
            var authenticator = new PKCEAuthenticator(Settings.ClientID!, token);
            authenticator.TokenRefreshed += (sender, t) => SaveToken(t);

            var config = SpotifyClientConfig.CreateDefault()
                .WithAuthenticator(authenticator);

            var spotify = new SpotifyClient(config);

            var me = await spotify.UserProfile.Current();
            Logger.Log(LogLevel.Information, $"Authenticated as: {me.DisplayName}");
            //Authenticated
            _server.Dispose();
            return spotify;
        }

        private async Task StartAuthentication()
        {
            Logger.Log(LogLevel.Information, "Starting Authentication");
            var (verifier, challenge) = PKCEUtil.GenerateCodes();

            await _server.Start();
            _server.AuthorizationCodeReceived += async (sender, response) =>
            {
                await _server.Stop();
                PKCETokenResponse token = await new OAuthClient().RequestToken(
                    new PKCETokenRequest(Settings.ClientID!, response.Code, _server.BaseUri, verifier)
                );

                SaveToken(token);
                await CreateClientWithToken();
            };

            var request = new LoginRequest(_server.BaseUri, Settings.ClientID!, LoginRequest.ResponseType.Code)
            {
                CodeChallenge = challenge,
                CodeChallengeMethod = "S256",
                Scope = new List<string>
                    {UserReadEmail, UserReadPrivate, PlaylistReadPrivate, PlaylistReadCollaborative}
            };

            Uri uri = request.ToUri();
            BrowserUtil.Open(uri);
        }

        private PKCETokenResponse LoadToken()
        {
            return JsonConvert.DeserializeObject<PKCETokenResponse>(Settings.Token);
        }

        private void SaveToken(PKCETokenResponse token)
        {
            Settings.Token = JsonConvert.SerializeObject(token);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
