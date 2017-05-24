using System;
using Newtonsoft.Json;


namespace MyYouTubePlaylistsDemo.Models
{
    public class YouTubePlaylistInfo
	{
		[JsonProperty(PropertyName="publishedAt")]
		public DateTime PublishedAt { get; set; }

		[JsonProperty(PropertyName="title")]
		public string Title { get; set; }

		[JsonProperty(PropertyName="description")]
		public string Description { get; set; }

		[JsonProperty(PropertyName="thumbnails")]
		public YouTubeThumbnail Thumbnails { get; set; }
	}
}