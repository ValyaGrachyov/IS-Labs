namespace HW5.Services;

public static class CosineSimilarityService
{
    public static decimal CalculateCosineSimilarity(List<decimal> x, List<decimal> y)
    {
        if (x.Count != y.Count)
            throw new ArgumentException("Длины должны совпадать!");

        var product = CalculateProduct(x, y);
        var xNorm = CalculateNorm(x);
        var yNorm = CalculateNorm(y);

        if(xNorm == 0 || yNorm == 0)
            return 0;
        return product / (xNorm * yNorm);
    }

    private static decimal CalculateProduct(IEnumerable<decimal> x, IEnumerable<decimal> y) => 
        x.Zip(y)
            .Sum(d => d.First * d.Second);

    private static decimal CalculateNorm(IEnumerable<decimal> x) =>
        (decimal) Math.Sqrt(x
            .Select(d => (double)d)
            .Sum(d => Math.Pow(d, 2)));
}