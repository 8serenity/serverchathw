using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerAppLesson
{
    public class UserMessage
    {
        [JsonProperty("sender")]
        public string Sender { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("date")]
        public DateTime SentDate { get; set; }
        [JsonProperty("file")]
        public byte?[] File { get; set; }
    }
}