using System;
using System.ComponentModel;
using System.Threading.Tasks;
using StreamDeckLib;
using StreamDeckLib.Messages;
using tv.tavernfire.spotify.models;

namespace tv.tavernfire.spotify
{
    public interface ITVGlobalSettings : INotifyPropertyChanged
    {
    }
    public abstract class BaseStreamDeckActionWithGlobalSettings<T> : BaseStreamDeckAction where T:ITVGlobalSettings
    {
        public abstract T GlobalSettingsModel { get; }

        public override Task OnWillAppear(StreamDeckEventPayload args)
        {
            SetModelProperties(args);
            return base.OnWillAppear(args);
        }

        public override Task OnWillDisappear(StreamDeckEventPayload args)
        {
            Manager.SetGlobalSettingsAsync(args.context, GlobalSettingsModel);
            return base.OnWillDisappear(args);
        }

        
        
        public override async Task OnDidReceiveGlobalSettings(StreamDeckEventPayload args)
        {
            SetModelProperties(args);
            await base.OnDidReceiveGlobalSettings(args);
        }

        protected void SetModelProperties(StreamDeckEventPayload args)
        {
            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                if (args.payload != null && args.payload.settings != null && args.payload.settings.settingsModel != null)
                {
                    if (args.PayloadSettingsHasProperty(prop.Name))
                    {
                        var value = args.GetPayloadSettingsValue(prop.Name);
                        var value2 = Convert.ChangeType(value, prop.PropertyType);
                        prop.SetValue(GlobalSettingsModel, value2);
                    }
                }
            }
        }
    }
}