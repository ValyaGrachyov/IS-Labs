
using HW3.Services;

if (!InvertedIndexBuilder.TryGetIndex(out var index))
{
    index = await InvertedIndexBuilder.BuildIndex();
    await InvertedIndexBuilder.SaveIndex(index);
}

if (index is null)
{
    Console.WriteLine("Не удалось построить обратный индекс!");
    return;
}

while (true)
{
    Console.WriteLine("Введите булев запрос (например: слово1 И слово2 ИЛИ !слово3):");
    var query = Console.ReadLine()!;
    var search = new BooleanSearch(index);
    var docs = await search.Search(query);
    var result = $"[{string.Join(", ", docs)}]";

    Console.WriteLine("Документы:");
    Console.WriteLine(result);
}