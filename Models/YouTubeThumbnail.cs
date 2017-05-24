using Newtonsoft.Json;

namespace MyYouTubePlaylistsDemo.Models
{
	public class YouTubeThumbnail
	{
		[JsonProperty(PropertyName="medium")]
		public YouTubeThumbnailInfo Medium { get; set; }
	}
}