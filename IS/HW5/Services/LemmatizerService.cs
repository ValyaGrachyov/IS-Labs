using System.Diagnostics;
using System.Text;

namespace HW5.Services;

public static class LemmatizerService
{
    private static readonly string MystemPath =
        Path.Combine(Directory.GetCurrentDirectory(), @"mystem-yandex\mystem.exe");
    
    private static readonly Process Process;

    static LemmatizerService()
    {
        Process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = MystemPath,
                Arguments = "-nld",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true, 
                CreateNoWindow = true,
                StandardInputEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
            },
            EnableRaisingEvents = true
        };
        
        Process.ErrorDataReceived += (_, e) => 
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine($"MyStem ERROR: {e.Data}");
        };
        
        Process.Start();
        Process.BeginErrorReadLine();
    }
    
    /// <summary>
    /// Лемматизация слова через mystem.exe
    /// </summary>
    public static async Task<IEnumerable<string>> Lemmatize(IEnumerable<string> tokens)
    {
        List<string> lemmas = [];
        foreach (var token in tokens)
        {
            if (Process.HasExited)
                Process.Start();

            await Process.StandardInput.WriteLineAsync(token);
            var result = await Process.StandardOutput.ReadLineAsync();
            var lemma = result?.Replace("?", "") ?? token;
            lemmas.Add(lemma);
        }

        return lemmas;
    }
}