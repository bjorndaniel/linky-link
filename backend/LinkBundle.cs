using System.Collections.Generic;
using Newtonsoft.Json;

namespace LinkyLink
{
  public class LinkBundle
  {
    public LinkBundle(string userId, string vanityUrl, string description, IEnumerable<LinkItem> links)
    {
      this.UserId = userId;
      this.VanityUrl = vanityUrl;
      this.Description = description;
      this.Links = links;
    }

    [JsonProperty("userId")]
    public string UserId { get; set; }

    [JsonProperty("vanityUrl")]
    public string VanityUrl { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("links")]
    public IEnumerable<LinkItem> Links { get; }

    //TODO: I hate that I have to do this. Rather just use a KeyValuePair
    public class LinkItem
    {
      [JsonProperty("url")]
      public string Url { get; set; }

      [JsonProperty("title")]
      public string Title { get; set; }
    }
  }
}