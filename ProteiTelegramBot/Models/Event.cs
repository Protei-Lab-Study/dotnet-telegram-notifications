using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ProteiTelegramBot.Models
{
    public class Event
    {
        [JsonProperty("object_kind")]
        public string ObjectKind { get; set; }

    }
}