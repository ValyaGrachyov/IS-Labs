using System.Text;
using HW4.Models;
using HW3.Services;

namespace HW4.Services;

public static class CsvService
{
    public static async Task SaveCsv(string path, SortedDictionary<Token, SortedDictionary<Document, decimal>> dictionary, IEnumerable<string> fileNames)
    {
        if(File.Exists(path))
            return;

        var sorted = fileNames
            .Order(new NaturalSortComparer())
            .Select(Path.GetFileName);
        
        List<string> lines = [$"terms;{string.Join(";", sorted)}"];

        lines.AddRange(dictionary.Select(kv => $"{kv.Key.Value};{string.Join(";", kv.Value.Values)}"));

        await File.WriteAllLinesAsync(path, lines, Encoding.UTF8);
        Console.WriteLine($"Документ сохранен по пути: {path}");
    }
    
    public static async Task SaveCsv(string path, SortedDictionary<Token, decimal> dictionary)
    {
        if(File.Exists(path))
            return;

        List<string> lines = ["terms;values"];
        lines.AddRange(dictionary.Select(kv => $"{kv.Key.Value};{kv.Value}"));

        await File.WriteAllLinesAsync(path, lines, Encoding.UTF8);
        Console.WriteLine($"Документ сохранен по пути: {path}");
    }
}