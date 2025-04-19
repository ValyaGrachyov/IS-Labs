
using HW5.Services;
using HW4.Models;
using HW4.Services;
using HW3.Extensions;

var loadedTfIdf = TfIdfService.TryGetTermFrequencyInverseDocumentFrequency(out var tfIdf);
if (!loadedTfIdf)
{
    Console.WriteLine("Не удалось загрузить TF-IDF вектора документов!");
    return;
}

var loadedIdf = IdfService.TryGetInverseDocumentFrequency(out var idf);
if (!loadedIdf)
{
    Console.WriteLine("Не удалось инвертированный индекс!");
    return;
}

while (true)
{
    Console.WriteLine("Введите фразу:");
    
    //lemmatization
    var query = Console.ReadLine()!
        .Split(" ")
        .Select(x => x.ToLower());
    
    var lemmas = await LemmatizerService.Lemmatize(query);
    
    //find tokens in idf, if didn't find -> remove from list
    var tokens = lemmas
        .Select(x => new Token(x))
        .Where(x => idf.ContainsKey(x))
        .ToList();

    //calculate tf for each token
    var tfForQuery = TfService.CalculateTermFrequencyForQuery(tokens); 

    //calculate Tf-Idf for query
    var tfIdfForQuery = idf
        .Select(kv => new KeyValuePair<Token, decimal>(
            kv.Key, 
            Math.Round(kv.Value * tfForQuery.GetValueOrDefault(kv.Key, 0), 6)))
        .ToSortedDictionary(new NaturalSortComparerForModels());

    //calculate Cosine Similarity
    var cosineSimilarities = new List<(Document Document, decimal Similarity)>();
    foreach (var (document, dictionary) in tfIdf)
    {
        var cosineSimilarity = CosineSimilarityService
            .CalculateCosineSimilarity(
                dictionary.Values.ToList(),
                tfIdfForQuery.Values.ToList()
            ); 
        cosineSimilarities.Add((document, cosineSimilarity));
    }

    //then sort by results
    var sorted = cosineSimilarities
        .OrderByDescending(x => x.Similarity)
        .GroupBy(x => x.Similarity)
        .Select(x =>
        {
            var docNames = x.Select(d => d.Document.Value);
            return $"{string.Join(", ", docNames)}: Score = {x.Key}";
        })
        .ToList();
    
    //then cw
    Console.WriteLine("\nРезультаты:");
    Console.WriteLine($"{string.Join(";\n", sorted)};\n");
    
}
