using Newtonsoft.Json;

namespace MyYouTubePlaylistsDemo.Models
{
    public class YouTubePlaylist
	{
		[JsonProperty(PropertyName="id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName="snippet")]
		public YouTubePlaylistInfo Info { get; set; }

		[JsonProperty(PropertyName="status")]
		public YouTubePlaylistStatus Status { get; set; }
	}
}
