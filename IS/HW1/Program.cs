using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Linq;

class WebCrawler
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static readonly HashSet<string> visited = new HashSet<string>();
    private static readonly Queue<string> queue = new Queue<string>();
    private static readonly List<string> indexLines = new List<string>();
    private const int PageLimit = 100;

    public static async Task Main(string[] args)
    {
        var baseUrl= "https://ru.wikipedia.org/wiki/%D0%97%D0%B0%D0%B3%D0%BB%D0%B0%D0%B2%D0%BD%D0%B0%D1%8F_%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0";
        
        queue.Enqueue(baseUrl);
        

        int docId = 0;

        while (queue.Count > 0 && docId < PageLimit)
        {
            string url = queue.Dequeue();
            if (visited.Contains(url)) continue;

            try
            {
                Console.WriteLine($"Загрузка: {url}");
                var html = await httpClient.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                string text = ExtractText(doc.DocumentNode);
                int wordCount = CountWords(text);

                if (wordCount >= 1000)
                {
                    string fileName = $"doc_{docId}.txt";
                    await File.WriteAllTextAsync(fileName, text);
                    indexLines.Add($"{docId} {url}");
                    docId++;
                }

                var links = ExtractLinks(doc.DocumentNode, url);
                foreach (var link in links)
                {
                    if (!visited.Contains(link))
                        queue.Enqueue(link);
                }

                visited.Add(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке {url}: {ex.Message}");
            }
        }

        await File.WriteAllLinesAsync("index.txt", indexLines);
        Console.WriteLine($"Скачано {docId} страниц.");
    }

    private static string ExtractText(HtmlNode node)
    {
        var text = node.InnerText;
        text = Regex.Replace(text, @"\s+", " ").Trim();
        return text;
    }

    private static int CountWords(string text)
    {
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static IEnumerable<string> ExtractLinks(HtmlNode node, string baseUrl)
    {
        var hrefs = node.SelectNodes("//a[@href]")
            ?.Select(a => a.GetAttributeValue("href", ""))
            ?.Where(href => !string.IsNullOrWhiteSpace(href)) ?? Enumerable.Empty<string>();

        foreach (var href in hrefs)
        {
            string absoluteUrl = GetAbsoluteUrl(baseUrl, href);
            if (absoluteUrl.StartsWith("http"))
                yield return absoluteUrl;
        }
    }

    private static string GetAbsoluteUrl(string baseUrl, string href)
    {
        if (Uri.TryCreate(href, UriKind.Absolute, out var absolute))
            return absolute.ToString();

        if (Uri.TryCreate(new Uri(baseUrl), href, out var relative))
            return relative.ToString();

        return "";
    }
}
