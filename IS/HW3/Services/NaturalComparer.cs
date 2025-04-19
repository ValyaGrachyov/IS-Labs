namespace HW3.Services;

public class NaturalSortComparer : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == null || y == null)
            return 0;

        int ix = 0, iy = 0;

        while (ix < x.Length && iy < y.Length)
        {
            if (char.IsDigit(x[ix]) && char.IsDigit(y[iy]))
            {
                var nx = 0;
                while (ix < x.Length && char.IsDigit(x[ix]))
                {
                    nx = nx * 10 + (x[ix] - '0');
                    ix++;
                }

                var ny = 0;
                while (iy < y.Length && char.IsDigit(y[iy]))
                {
                    ny = ny * 10 + (y[iy] - '0');
                    iy++;
                }

                if (nx != ny)
                {
                    return nx - ny;
                }
            }
            else if (x[ix] != y[iy])
            {
                return x[ix] - y[iy];
            }
            else
            {
                ix++;
                iy++;
            }
        }

        return x.Length - y.Length;
    }
}