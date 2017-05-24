using System.Collections.Generic;
using Newtonsoft.Json;

namespace MyYouTubePlaylistsDemo.Models
{
	public class YouTubePlaylistResponse
	{
		[JsonProperty(PropertyName="items")]
		public IEnumerable<YouTubePlaylist> Playlists { get; set; }
	}
}