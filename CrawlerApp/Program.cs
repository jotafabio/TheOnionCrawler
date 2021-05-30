using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrawlerApp
{
    class Program
    {

        static async Task Main(string[] args)
        {
            List<pageContent> listArticles = await CrawlRootPage();
            Console.Clear();
            Console.WriteLine("\"URI\", \"headline\", \"datePublished\", \"authorName\", \"keywords\", \"articleBody\"");
            foreach (var article in listArticles)
            {
                Console.WriteLine("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"", article.URI, article.headline, article.datePublished, article.authorName, article.keywords, article.articleBody);
            };
            Console.ReadKey();
            Environment.Exit(0);
        }

        private async static Task<List<pageContent>> CrawlRootPage()
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            var cSeedURI = "http://www.theonion.com";
            var httpClient = new HttpClient();
            var cDocument = await httpClient.GetStringAsync(cSeedURI);
            var cDocumentParsed = new HtmlDocument();
            cDocumentParsed.LoadHtml(cDocument);

            List<pageContent> colPageContent = new List<pageContent>();

            foreach (var aURL in cDocumentParsed.DocumentNode.Descendants("a"))
            {
                Console.Write(".");
                if (aURL.GetAttributeValue("href", "").ToString().Contains("https://www.theonion.com/"))
                {
                    var cURI = aURL.GetAttributeValue("href", "").ToString();
                    var cChildDocument = await httpClient.GetStringAsync(cURI);
                    var cChildDocumentParsed = new HtmlDocument();
                    cChildDocumentParsed.LoadHtml(cChildDocument);

                    var cJSON = cChildDocumentParsed.DocumentNode.Descendants("script").Where(node => node.GetAttributeValue("type", "").Equals("application/ld+json")).ToList();

                    var jsonObject = JsonConvert.DeserializeObject(cJSON.FirstOrDefault().InnerHtml.ToString());
                    JObject jObject = (JObject)jsonObject;

                    var sHeadline = jObject["headline"]?.ToString();
                    var sType = jObject["@type"]?.ToString();
                    var sDescription = jObject["description"]?.ToString();
                    var sPublishedDate = jObject["datePublished"]?.ToString();
                    var sArticleBody = jObject["articleBody"]?.ToString();
                    var sAuthorArticle = jObject["name"]?.ToString();
                    var sKeywords = jObject["keywords"]?.ToString();

                    pageContent itemPageContent = new pageContent()
                    {
                        headline = sHeadline,
                        type = sType,
                        description = sDescription,
                        datePublished = sPublishedDate,
                        articleBody = sArticleBody,
                        authorName = sAuthorArticle,
                        keywords = sKeywords,
                        URI = cURI
                    };
                    colPageContent.Add(itemPageContent);

                };
            };
            List<pageContent> outPageContent = colPageContent.GroupBy(o => o.headline).Select(o => o.FirstOrDefault()).OrderBy(o => o.headline).ToList<pageContent>();

            return outPageContent;
        }
    }

    internal class pageContent
    {
        public pageContent() { }
        public string URI { get; set; }
        public string description { get; set; }
        public string datePublished { get; set; }
        public string articleBody { get; set; }
        public string authorName { get; set; }
        public string keywords { get; set; }
        public string headline { get; set; }
        public string type { get; set; }

    }
}
