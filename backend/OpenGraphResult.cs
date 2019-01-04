using System.Linq;
using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenGraphNet;

namespace LinkyLink
{
    public class OpenGraphResult
    {
        public OpenGraphResult() { }

        public OpenGraphResult(string id, OpenGraph graph, params HtmlNode[] nodes)
        {
            Id = id;

            //Use og:title else fallback to html title tag
            var title = nodes.SingleOrDefault(n => n.Name == "title").InnerText;
            Title = string.IsNullOrEmpty(graph.Title) ? title : graph.Title;

            Image = graph.Metadata["og:image"].FirstOrDefault()?.Value;

            //Default to og:description else fallback to description meta tag
            string descriptionData = string.Empty;
            var descriptionNode = nodes.FirstOrDefault(n => n.Attributes.Contains("content")
                                              && n.Attributes.Contains("name")
                                              && n.Attributes["name"].Value == "description");

            Description = graph.Metadata["og:description"].FirstOrDefault()?.Value ?? descriptionNode.Attributes["content"].Value;
        }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public string Image { get; set; }
    }
}