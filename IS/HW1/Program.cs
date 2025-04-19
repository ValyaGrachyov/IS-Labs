using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ConsoleWebCrawler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            args = new string[] { "10", "https://ru.wikipedia.org/" };
            
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ConsoleWebCrawler <count> <url1> [url2] [...]");
                return;
            }

            int count = Math.Clamp(int.Parse(args[0]), 100, 150);
            var uriList = args
                .Skip(1)
                .Select(s => Uri.TryCreate(s, UriKind.Absolute, out var u) ? u : null)
                .Where(u => u != null)
                .Cast<Uri>()
                .ToList();

            using var httpClient = new HttpClient();
            var crawler = new WebCrawlerService(httpClient);
            await crawler.Crawl(count, uriList);
        }
    }

    public partial class WebCrawlerService
    {
        private readonly HttpClient _httpClient;
        private static readonly string PagesPath = Path.Combine(Directory.GetCurrentDirectory(), "Pages");

        public WebCrawlerService(HttpClient httpClient) => _httpClient = httpClient;

        public async Task Crawl(int maxCount, List<Uri> startUris)
        {
            Directory.CreateDirectory(PagesPath);
            using var indexWriter = new StreamWriter(Path.Combine(PagesPath, "index.txt"), true, Encoding.UTF8);

            var queue = new Queue<Uri>(startUris);
            var visited = new HashSet<Uri>();
            int saved = 0;

            Console.WriteLine($"Page count: {maxCount}");

            while (queue.Count > 0 && saved < maxCount)
            {
                var current = queue.Dequeue();
                if (!visited.Add(current)) continue;

                Console.WriteLine($"Processing: {current}");
                var info = await GetHtmlPageInfo(current);
                if (info == null) continue;

                foreach (var child in info.ChildUris)
                    queue.Enqueue(child);

                if (info.Words.Count < 1000) continue;

                var contentFile = Path.Combine(PagesPath, $"{saved}.txt");
                await File.WriteAllTextAsync(contentFile, string.Join(' ', info.Words), Encoding.UTF8);
                await indexWriter.WriteLineAsync($"{saved,-5}{current}");
                saved++;
            }
        }

        private async Task<HtmlPageInfo?> GetHtmlPageInfo(Uri uri)
        {
            try
            {
                var response = await _httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                var html = await response.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var langNode = doc.DocumentNode.SelectSingleNode("//html[@lang]");
                if (langNode != null)
                {
                    var lang = langNode.GetAttributeValue("lang", "");
                    if (lang != "ru" && lang != "ru-RU") return null;
                }

                var links = ExtractLinks(doc, uri);
                var words = ExtractWords(html);
                return new HtmlPageInfo(links, words);
            }
            catch
            {
                return null;
            }
        }

        private List<Uri> ExtractLinks(HtmlDocument doc, Uri baseUri)
        {
            var nodes = doc.DocumentNode.SelectNodes("//a[@href]");
            if (nodes == null) return new();

            return nodes
                .Select(n => n.GetAttributeValue("href", ""))
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Select(h =>
                {
                    try { return new Uri(baseUri, h); }
                    catch { return null; }
                })
                .Where(u => u != null && (string.IsNullOrEmpty(u.Query) || u.Query == "?"))
                .Distinct()!
                .Cast<Uri>()
                .ToList();
        }

        private List<string> ExtractWords(string html)
        {
            html = RemoveMainTags().Replace(html, " ");
            html = RemoveTags().Replace(html, " ");
            return CyrillicRegex()
                .Matches(html)
                .Select(m => m.Value)
                .ToList();
        }

        [GeneratedRegex(@"<(script|head|style)[^>]*>.*?</\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
        private static partial Regex RemoveMainTags();

        [GeneratedRegex("<.*?>", RegexOptions.Compiled)]
        private static partial Regex RemoveTags();

        [GeneratedRegex(@"\p{IsCyrillic}+", RegexOptions.Compiled)]
        private static partial Regex CyrillicRegex();
    }

    public record HtmlPageInfo(List<Uri> ChildUris, List<string> Words);
}
