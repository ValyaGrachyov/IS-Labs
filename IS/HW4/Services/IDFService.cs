using System.Text;
using HW4.Models;
using HW3.Extensions;

namespace HW4.Services;

public class IdfService
{
    private static readonly string BasePath = Directory.GetCurrentDirectory();
    private static readonly string ProcessedPath = Path.Combine(BasePath, "Processed");
    private static readonly string TfIdfPath = Path.GetFullPath(Path.Combine(BasePath, @"..\..\..\TF-IDF"));
    private static readonly string IdfFilePath = Path.Combine(TfIdfPath, "idf.csv");
    
    private static readonly SortedDictionary<Token, decimal> Idf = new(new NaturalSortComparerForModels());

    public static async Task<SortedDictionary<Token, decimal>> CreateInverseDocumentFrequency(SortedDictionary<string, SortedSet<string>> index)
    {
        var txtFilesCount = Directory.GetFiles(ProcessedPath, "*.txt").Length;

        foreach (var kv in index)
        {
            var idf = Math.Round((decimal) Math.Log10(txtFilesCount / kv.Value.Count), 6);
            Idf.TryAdd(new Token(kv.Key), idf);
        }

        await CsvService.SaveCsv(IdfFilePath, Idf);
        return Idf;
    }
    
    public static bool TryGetInverseDocumentFrequency(out SortedDictionary<Token, decimal> idf)
    {
        idf = [];
        
        if (!File.Exists(IdfFilePath))
            return false;
        
        var idfFile = File.ReadAllLines(IdfFilePath, Encoding.UTF8);

        var dictionary = idfFile
            .Skip(1)
            .Select(x => x.Split(";"))
            .ToDictionary(
                kv => new Token(kv[0]), 
                kv => decimal.Parse(kv[1]));
        idf = dictionary.ToSortedDictionary(new NaturalSortComparerForModels());

        return idf.Count > 0;
    }
}