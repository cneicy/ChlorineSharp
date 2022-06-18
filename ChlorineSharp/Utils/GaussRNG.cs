using System;

namespace ChlorineSharp.Utils;

/// <summary>
/// https://zhuanlan.zhihu.com/p/399266153
/// </summary>
public class GaussianRng
{
    private int _iset;
    private double _gset;
    private readonly Random _r1;
    private readonly Random _r2;

    public GaussianRng(int seed)
    {
        _r1 = new Random(seed);
        _r2 = new Random(seed + 7);
        _iset = 0;
    }

    public double Next()
    {
        if (_iset == 0)
        {
            double v2;
            double v1;
            double rsq;
            do
            {
                v1 = 2.0 * _r1.NextDouble() - 1.0;
                v2 = 2.0 * _r2.NextDouble() - 1.0;
                rsq = v1 * v1 + v2 * v2;
            } while (rsq >= 1.0 || rsq == 0.0);

            var fac = Math.Sqrt(-2.0 * Math.Log(rsq) / rsq);
            _gset = v1 * fac;
            _iset = 1;
            return v2 * fac;
        }

        _iset = 0;
        return _gset;
    }
}