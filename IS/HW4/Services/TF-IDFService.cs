using System.Text;
using HW4.Models;
using HW3.Extensions;

namespace HW4.Services;

public class TfIdfService
{
    private static readonly string BasePath = Directory.GetCurrentDirectory();
    private static readonly string ProcessedPath = Path.Combine(BasePath, "Processed");
    private static readonly string TfIdfPath = Path.GetFullPath(Path.Combine(BasePath, @"..\..\..\TF-IDF"));
    private static readonly string TfIdfFilePath = Path.Combine(TfIdfPath, "tf-idf.csv");
    
    private static SortedDictionary<Token, SortedDictionary<Document, decimal>> TfIdf = new(new NaturalSortComparerForModels());
    
    public static async Task CreateTermFrequencyInverseDocumentFrequency(SortedDictionary<Token, decimal> idf,
        SortedDictionary<Token, SortedDictionary<Document, decimal>> tf)
    {
        foreach (var (term, frequencies) in tf)
        {
            var frequency = idf[term];
            var termTfIdf = frequencies.ToDictionary(
                kv => kv.Key,
                kv => Math.Round(kv.Value * frequency, 6));
            
            TfIdf.TryAdd(term, termTfIdf.ToSortedDictionary(new NaturalSortComparerForModels()));
        }
     
        var txtFiles = Directory.GetFiles(ProcessedPath, "*.txt");
        await CsvService.SaveCsv(TfIdfFilePath, TfIdf, txtFiles);
    }
    
    /// <summary>
    /// Получаю вектора TF-IDF для документов
    /// </summary>
    /// <param name="tfIdf"></param>
    /// <returns></returns>
    public static bool TryGetTermFrequencyInverseDocumentFrequency(out SortedDictionary<Document, SortedDictionary<Token, decimal>> tfIdf)
    {
        tfIdf = new SortedDictionary<Document, SortedDictionary<Token, decimal>>(new NaturalSortComparerForModels());
        
        if (!File.Exists(TfIdfFilePath))
            return false;
        
        var tfIdfFile = File.ReadAllLines(TfIdfFilePath, Encoding.UTF8);

        var docs = tfIdfFile[0]
            .Split(";")
            .Skip(1)
            .Select(x => new Document(x))
            .ToList();

        foreach (var doc in docs)
            tfIdf.TryAdd(doc, new SortedDictionary<Token, decimal>(new NaturalSortComparerForModels()));
        
        foreach (var tfInfo in tfIdfFile.Skip(1))
        {
            var content = tfInfo
                .Split(";")
                .ToList();
            
            var word = new Token(content[0]);

            var values = content
                .Skip(1)
                .Select(decimal.Parse)
                .Zip(docs);
            
            foreach (var (frequency, doc) in values)
                tfIdf[doc].TryAdd(word, frequency);
        }

        return tfIdf.Count > 0;
    }
}