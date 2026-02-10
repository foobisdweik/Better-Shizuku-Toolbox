namespace Shizuku.Services.Decimation;

/// <summary>
/// Visual decimation helper that reduces a dense signal array to a min/max pair
/// per horizontal pixel column. This preserves the visual envelope of the waveform
/// while dramatically reducing the number of points the chart must render.
/// </summary>
public static class MinMaxDecimator
{
    /// <summary>
    /// Decimate the <paramref name="source"/> array into at most
    /// <paramref name="targetPointCount"/> min/max pairs.
    /// Returns an array of length &lt;= 2 * targetPointCount.
    /// Odd indices are min values, even indices are max values.
    /// </summary>
    public static double[] Decimate(ReadOnlySpan<double> source, int targetPointCount)
    {
        if (source.Length == 0 || targetPointCount <= 0)
            return Array.Empty<double>();

        if (source.Length <= targetPointCount * 2)
        {
            // No decimation needed.
            return source.ToArray();
        }

        var result = new double[targetPointCount * 2];
        var samplesPerBucket = (double)source.Length / targetPointCount;

        for (var i = 0; i < targetPointCount; i++)
        {
            var start = (int)(i * samplesPerBucket);
            var end = (int)((i + 1) * samplesPerBucket);
            end = Math.Min(end, source.Length);

            var min = double.MaxValue;
            var max = double.MinValue;
            for (var j = start; j < end; j++)
            {
                var v = source[j];
                if (v < min) min = v;
                if (v > max) max = v;
            }

            result[i * 2] = min;
            result[i * 2 + 1] = max;
        }

        return result;
    }
}
