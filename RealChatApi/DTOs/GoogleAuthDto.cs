using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace RealChatApi.DTOs


{
    public class GoogleAuthDto
    {
        public const string PROVIDER = "google";


        [JsonProperty("idToken")]
        [Required]
        public string IdToken { get; set; }
    }
}
