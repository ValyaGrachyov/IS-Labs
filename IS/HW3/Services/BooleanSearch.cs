using System.Diagnostics;
using System.Text;

namespace HW3.Services;

public class BooleanSearch
{
    private readonly SortedDictionary<string, SortedSet<string>> _index;
    private readonly IEnumerable<string> _fileNames;
    private static readonly string MystemPath = Path.Combine(Directory.GetCurrentDirectory(), @"mystem-yandex\mystem.exe");
    private static readonly Process Process;

    static BooleanSearch()
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
                StandardOutputEncoding = Encoding.UTF8
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
    
    public BooleanSearch(SortedDictionary<string, SortedSet<string>> index)
    {
        _index = index;
        _fileNames = index.Values
            .SelectMany(x => x)
            .Distinct();
    }
        
    public async Task<IEnumerable<string>> Search(string input)
    {
        var tokens = Tokenize(input);
        var postfix = ToPostfix(tokens);

        var docs = (await SearchDocs(postfix))
            .Order(new NaturalSortComparer());
        return docs;
    }

    private static string[] Tokenize(string input)
    {
        var replaced = input
            .Replace("ИЛИ", " | ")
            .Replace("И", " & ")
            .Replace("!", " ! ")
            .Replace("НЕ", " ! ")
            .ToLower();

        return replaced.Split(" ", StringSplitOptions.RemoveEmptyEntries);
    }

    private static List<string> ToPostfix(IEnumerable<string> tokens)
    {
        var output = new List<string>();
        var stack = new Stack<string>();
        var priorities = new Dictionary<string, int>
        {
            ["!"] = 3,
            ["&"] = 2,
            ["|"] = 1
        };

        foreach (var token in tokens)
        {
            if (token is "&" or "|" or "!")
            {
                while (stack.Count > 0 &&
                       priorities.TryGetValue(stack.Peek(), out var prec) &&
                       prec >= priorities[token])
                    output.Add(stack.Pop());
                stack.Push(token);
            }
            else 
                output.Add(token);
        }

        while (stack.Count > 0)
            output.Add(stack.Pop());

        return output;
    }

    private async Task<IEnumerable<string>> SearchDocs(IEnumerable<string> tokens)
    {
        var stack = new Stack<IEnumerable<string>>();
        
        foreach (var token in tokens)
        {
            switch (token)
            {
                case "&" or "|":
                {
                    var right = stack.Pop();
                    var left = stack.Pop();
                    var combined = token == "&" ?
                        And(left, right) :
                        Or(left, right);
                    stack.Push(combined);
                    break;
                }
                case "!":
                {
                    var operand = stack.Pop();
                    var notExpr = Not(operand);
                    stack.Push(notExpr);
                    break;
                }
                default:
                {
                    var constant = await Constant(token);
                    stack.Push(constant);
                    break;
                }
            }
        }

        return stack.Pop();
    }

    private IEnumerable<string> And(IEnumerable<string> a, IEnumerable<string> b) => a.Intersect(b);

    private IEnumerable<string> Or(IEnumerable<string> a, IEnumerable<string> b) => a.Union(b);

    private IEnumerable<string> Not(IEnumerable<string> a) => _fileNames.Except(a);

    private async Task<IEnumerable<string>> Constant(string token)
    {
        var lemma = await LemmatizeWithMystem(token);
        return !_index.TryGetValue(lemma, out var value) ? [] : value;
    }

    /// <summary>
    /// Лемматизация слова через mystem.exe
    /// </summary>
    private static async Task<string> LemmatizeWithMystem(string token)
    {
        if (Process.HasExited)
            Process.Start();

        await Process.StandardInput.WriteLineAsync(token);
        var result = await Process.StandardOutput.ReadLineAsync();
        var lemma = result?.Replace("?", "") ?? token;
        return lemma;
    }
}