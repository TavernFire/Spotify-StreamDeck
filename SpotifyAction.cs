using System;
using StreamDeckLib;
using StreamDeckLib.Messages;
using System.Threading.Tasks;
using tv.tavernfire.spotify.models;

namespace tv.tavernfire.spotify
{
    [ActionUuid(Uuid = "tv.tavernfire.spotify.DefaultPluginAction")]
    public class SpotifyAction : BaseStreamDeckActionWithGlobalSettings<SpotifySettingsModel>
    {
        private ISpotifyClientFactory _spotify;
        public override SpotifySettingsModel GlobalSettingsModel => _spotify.Settings;

        public SpotifyAction(ISpotifyClientFactory spotify)
        {
            _spotify = spotify;
        }

        public override async Task OnKeyUp(StreamDeckEventPayload args)
        {
            await _spotify.Main();
            var privateUser = await _spotify.Client.UserProfile.Current();
            await Manager.SetTitleAsync(args.context, privateUser.DisplayName);
            return;
        }

    }
}