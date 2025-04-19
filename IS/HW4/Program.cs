// See https://aka.ms/new-console-template for more information

using HW4.Services;
using HW3.Services;

if (!InvertedIndexBuilder.TryGetIndex(out var index))
{
    Console.WriteLine("Не удалось получить обратный индекс!");
    return;
}

if (!TfService.TryGetTermFrequency(out var tf))
    tf = await TfService.CreateTermFrequency();

if (!IdfService.TryGetInverseDocumentFrequency(out var idf))
    idf = await IdfService.CreateInverseDocumentFrequency(index);

await TfIdfService.CreateTermFrequencyInverseDocumentFrequency(idf, tf);

Console.WriteLine("Done");