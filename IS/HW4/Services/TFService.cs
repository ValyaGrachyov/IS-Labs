using System.Text;
using HW4.Models;
using HW3.Extensions;
using HW3.Services;

namespace HW4.Services;

public class TfService
{
    private static readonly string BasePath = Directory.GetCurrentDirectory();
    private static readonly string ProcessedPath = Path.Combine(BasePath, "Processed");
    private static readonly string TfIdfPath = Path.GetFullPath(Path.Combine(BasePath, @"..\..\..\TF-IDF"));
    private static readonly string TfFilePath = Path.Combine(TfIdfPath, "tf.csv");

    private static readonly SortedDictionary<Token, SortedDictionary<Document, decimal>> Tf = new (new NaturalSortComparerForModels());
    public static async Task<SortedDictionary<Token, SortedDictionary<Document, decimal>>> CreateTermFrequency()
    {
        var txtFiles = Directory.GetFiles(ProcessedPath, "*.txt");

        var frequencies = txtFiles
            .Order(new NaturalSortComparer())
            .Select(Path.GetFileName)
            .ToDictionary(kv => new Document(kv!), _ => 0m);
        
        foreach (var textFile in txtFiles)
        {
            var content = await File.ReadAllTextAsync(textFile, Encoding.UTF8);
            var filePath = new Document(Path.GetFileName(textFile));

            var words = content.Split(" ");
            var terms = words
                .GroupBy(x => x)
                .Select(x => new {Key = new Token(x.Key), Count = x.Count() })
                .ToList();
            
            foreach (var term in terms)
            {
                Tf.TryAdd(term.Key, frequencies.ToSortedDictionary(new NaturalSortComparerForModels()));
                var frequency = Math.Round((decimal) term.Count / words.Length, 6);
                Tf[term.Key][filePath] = frequency;
            }
        }

        await CsvService.SaveCsv(TfFilePath, Tf, txtFiles);
        return Tf;
    }
    
    public static SortedDictionary<Token, decimal> CalculateTermFrequencyForQuery(IEnumerable<Token> tokens)
    {
        var list = tokens.ToList();
        var dict = list
            .GroupBy(x => x)
            .ToDictionary(
                kv => kv.Key,
                kv => Math.Round((decimal) kv.Count() / list.Count, 6));
        
        return dict.ToSortedDictionary(new NaturalSortComparerForModels());
    }
    
    public static bool TryGetTermFrequency(out SortedDictionary<Token, SortedDictionary<Document, decimal>> tf)
    {
        tf = new SortedDictionary<Token, SortedDictionary<Document, decimal>>(new NaturalSortComparerForModels());
        
        if (!File.Exists(TfFilePath))
            return false;
        
        var tfFile = File.ReadAllLines(TfFilePath, Encoding.UTF8);

        var docs = tfFile[0].Split(";")[1..];
        
        foreach (var tfInfo in tfFile.Skip(1))
        {
            var content = tfInfo.Split(";").ToList();
            var word = new Token(content[0]);

            var values = content
                .Skip(1)
                .Select(decimal.Parse)
                .Zip(docs);
            
            var frequencies = values.ToDictionary(
                kv => new Document(kv.Second), 
                kv => kv.First);
            
            tf.TryAdd(word, frequencies.ToSortedDictionary(new NaturalSortComparerForModels()));
        }

        return tf.Count > 0;
    }
}