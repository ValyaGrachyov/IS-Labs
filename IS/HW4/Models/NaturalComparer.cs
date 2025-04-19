namespace HW4.Models;

public class NaturalSortComparerForModels : IComparer<BaseModel>
{
    public int Compare(BaseModel? x, BaseModel? y)
    {
        if (x == null || y == null)
            return 0;

        int ix = 0, iy = 0;

        while (ix < x.Value.Length && iy < y.Value.Length)
        {
            if (char.IsDigit(x.Value[ix]) && char.IsDigit(y.Value[iy]))
            {
                var nx = 0;
                while (ix < x.Value.Length && char.IsDigit(x.Value[ix]))
                {
                    nx = nx * 10 + (x.Value[ix] - '0');
                    ix++;
                }

                var ny = 0;
                while (iy < y.Value.Length && char.IsDigit(y.Value[iy]))
                {
                    ny = ny * 10 + (y.Value[iy] - '0');
                    iy++;
                }

                if (nx != ny)
                {
                    return nx - ny;
                }
            }
            else if (x.Value[ix] != y.Value[iy])
            {
                return x.Value[ix] - y.Value[iy];
            }
            else
            {
                ix++;
                iy++;
            }
        }

        return x.Value.Length - y.Value.Length;
    }
}