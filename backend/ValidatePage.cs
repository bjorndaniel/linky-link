using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenGraphNet;
using System.Net;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using OpenGraphNet.Metadata;
using OpenGraphNet.Namespaces;

namespace LinkyLink
{
    public static partial class LinkOperations
    {
        [FunctionName(nameof(ValidatePage))]
        public static async Task<IActionResult> ValidatePage(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "validatePage")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            try
            {
                if (data is JArray)
                {
                    // expecting a JSON array of objects with url(string), id(string)
                    IEnumerable<OpenGraphResult> result = await GetMultipleGraphResults(data, log);
                    return new OkObjectResult(result);
                }
                else if (data is JObject)
                {
                    // expecting a JSON object with url(string), id(string)
                    OpenGraphResult result = await GetGraphResult(data, log);
                    return new OkObjectResult(result);
                }

                log.LogError("Invalid playload");
                ProblemDetails problemDetails = new ProblemDetails
                {
                    Title = "Could not validate links",
                    Detail = "Payload must be a valid JSON object or array",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "/linkylink/clientissue",
                    Instance = req.Path
                };
                return new BadRequestObjectResult(problemDetails);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                ProblemDetails problemDetails = new ProblemDetails
                {
                    Title = "Could not validate links",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Type = "/linkylink/clientissue",
                    Instance = req.Path
                };
                return new BadRequestObjectResult(problemDetails);
            }
        }

        public static async Task<OpenGraphResult> GetGraphResult(dynamic singleLinkItem, ILogger log)
        {
            string url = singleLinkItem.url, id = singleLinkItem.id;
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(id))
            {
                try
                {
                    if (!Regex.IsMatch(url, @"^https?:\/\/", RegexOptions.IgnoreCase))
                        url = "http://" + url;

                    if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriLink))
                    {
                        log.LogWarning("Invalid URL: {url}", url);
                        return new OpenGraphResult { Id = id };
                    }
                    HttpDownloader downloader = new HttpDownloader(uriLink, null, "AzureFunctions");
                    string html = await downloader.GetPageAsync();

                    OpenGraph graph = OpenGraph.ParseHtml(html);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    var descriptionMetaTag = doc.DocumentNode.SelectSingleNode("//meta[@name='description']");
                    var titleTag = doc.DocumentNode.SelectSingleNode("//head/title");
                    return new OpenGraphResult(id, graph, descriptionMetaTag, titleTag);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, ex.Message);
                    return new OpenGraphResult { Id = id };
                }
            }
            return null;
        }

        public static async Task<IEnumerable<OpenGraphResult>> GetMultipleGraphResults(dynamic multiLinkItem, ILogger log)
        {
            log.LogInformation("Running batch url validation");
            IEnumerable<OpenGraphResult> allResults =
                await Task.WhenAll((multiLinkItem as JArray).Select(item => GetGraphResult(item, log)));

            return allResults;
        }
    }
}