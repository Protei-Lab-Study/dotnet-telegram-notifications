using System.Reflection.Emit;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ProteiTelegramBot.Models
{
    public class MergeRequestEvent : Event
    {
        public MergeRequestUser User { get; set; }
        public MergeRequestProject Project { get; set; }
        [JsonProperty("object_attributes")]
        public MergeRequestObjectAttributes ObjectAttributes { get; set; }
    }
    public class MergeRequestObjectAttributes
    {
        public string Title { get; set; }
        public string State { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Action { get; set; }
    }


    public class MergeRequestProject
    {
        public string Name { get; set; }
    }

    public class MergeRequestUser
    {
        public string Name { get; set; }
        public string Username { get; set; }
    }


}