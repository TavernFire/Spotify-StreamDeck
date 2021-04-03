using System.ComponentModel;
using System.Runtime.CompilerServices;
using StreamDeckLib;

namespace tv.tavernfire.spotify.models
{
    public class SpotifySettingsModel : ITVGlobalSettings
    {
        private string _clientID = "";
        private string _clientSecret = "";
        private string? _token;

        public string ClientID
        {
            get => _clientID;
            set
            {
                if (value == _clientID) return;
                _clientID = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Initialized));
            }
        }

        public string ClientSecret
        {
            get => _clientSecret;
            set
            {
                if (value == _clientSecret) return;
                _clientSecret = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Initialized));
            }
        }

        public string? Token
        {
            get => _token;
            set
            {
                if (value == _token) return;
                _token = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Ready));
            }
        }

        public bool Initialized => !string.IsNullOrEmpty(ClientID) && !string.IsNullOrEmpty(ClientSecret);
        public bool Ready => !string.IsNullOrEmpty(Token);
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}