using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MyYouTubePlaylistsDemo.Models;
using Newtonsoft.Json;

namespace MyYouTubePlaylistsDemo.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public async Task<IActionResult> Index()
        {
            var youtubeToken = HttpContext.Request.Cookies["ytToken"];

            if (youtubeToken != null)
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        AuthenticationHeaderValue.Parse("Bearer " + youtubeToken);

                    using (var response = await client.GetAsync(
                        $"https://www.googleapis.com/youtube/v3/playlists?part=snippet,status&key={Configuration["Authentication:YouTube:ApiKey"]}&mine=true&maxResults=10"))
                    {
                        try
                        {
                            string stringResponse = await response.Content.ReadAsStringAsync();

                            return View(JsonConvert.DeserializeObject<YouTubePlaylistResponse>(stringResponse));
                        }
                        catch (HttpRequestException)
                        {

                        }
                    }
                }
            }

            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
