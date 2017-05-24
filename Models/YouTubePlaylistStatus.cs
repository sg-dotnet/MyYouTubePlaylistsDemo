using Newtonsoft.Json;

namespace MyYouTubePlaylistsDemo.Models
{
	public class YouTubePlaylistStatus
	{
		[JsonProperty(PropertyName="privacyStatus")]
		public string PrivacyStatus { get; set; }
	}
}