using Newtonsoft.Json;

namespace MyYouTubePlaylistsDemo.Models
{
	public class YouTubeThumbnailInfo
	{
		[JsonProperty(PropertyName="url")]
		public string Url { get; set; }

		[JsonProperty(PropertyName="width")]
		public double Width { get; set; }

		[JsonProperty(PropertyName="Height")]
		public double Height { get; set; }
	}
}