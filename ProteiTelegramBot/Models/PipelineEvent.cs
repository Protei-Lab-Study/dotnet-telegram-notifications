using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ProteiTelegramBot.Models
{
    public class PipelineEvent : Event
    {
        [JsonProperty("object_attributes")]
        public PipelineObjectAttributes ObjectAttributes { get; set; }
        public PipelineUser User { get; set; }
        public PipelineProject Project { get; set; }
    }

    public class PipelineObjectAttributes
    {
        public string Status { get; set; }
        public List<string> Stages { get; set; }
        public List<Variable> Variables { get; set; }
    }

    public class PipelineProject
    {
        public string Name { get; set; }
        [JsonProperty("web_url")]
        public string Url { get; set; }
    }

    public class PipelineUser
    {
        public string Name { get; set; }
        public string Username { get; set; }
    }

    public class Variable
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
