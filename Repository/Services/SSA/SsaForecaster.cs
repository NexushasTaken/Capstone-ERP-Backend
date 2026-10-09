namespace ERP.Repository.Services.SSA
{
    // One SSA forecast: a value per future week, with a 95% range around it
    public record SsaForecast(double[] Expected, double[] Low, double[] BusyCase);

    // Singular Spectrum Analysis, the standard ("textbook") version. No packages.
    //
    // In plain words: cut the history into overlapping slices of `window` weeks, find the few
    // repeating shapes that explain most of those slices (the trend and the yearly season),
    // rebuild the history from only those shapes (this drops the week-to-week noise), and
    // continue that smoothed line into the future.
    //
    // It continues the smoothed line, not the raw recent weeks. That is the difference from
    // ML.NET's ForecastBySsa, which lost to a simple 4-week average on our data
    // (docs/ssa-design/mlnet-ssa-test.typ on the experiment/mlnet-ssa-forecast branch).
    public static class SsaForecaster
    {
        private const double Z95 = 1.96; // 95% of a normal spread is within 1.96 standard deviations

        // Returns null when the history can't be modelled (too short, all zeros, or no stable
        // continuation); the caller then falls back to the simple average.
        public static SsaForecast? Forecast(IReadOnlyList<double> series, int window, int rank, int horizon)
        {
            int n = series.Count;
            int slices = n - window + 1;
            if (window < 2 || rank < 1 || rank >= window || slices < window || series.All(x => x == 0))
            {
                return null;
            }

            // 1. How each week in a slice moves together with every other week in it
            //    (the lag-covariance matrix of the trajectory matrix), window x window
            var covariance = new double[window, window];
            for (int i = 0; i < window; i++)
            {
                for (int j = i; j < window; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < slices; k++)
                    {
                        sum += series[i + k] * series[j + k];
                    }
                    covariance[i, j] = covariance[j, i] = sum;
                }
            }

            // 2. The repeating shapes (eigenvectors), strongest first
            var shapes = Eigenvectors(covariance);

            // 3. Rebuild every slice from the top `rank` shapes only, then average the
            //    overlapping slices back into one smoothed series (diagonal averaging)
            var smoothed = new double[n];
            var overlaps = new int[n];
            for (int k = 0; k < slices; k++)
            {
                var rebuilt = new double[window];
                for (int c = 0; c < rank; c++)
                {
                    double weight = 0;
                    for (int i = 0; i < window; i++)
                    {
                        weight += shapes[i, c] * series[i + k];
                    }
                    for (int i = 0; i < window; i++)
                    {
                        rebuilt[i] += weight * shapes[i, c];
                    }
                }
                for (int i = 0; i < window; i++)
                {
                    smoothed[i + k] += rebuilt[i];
                    overlaps[i + k]++;
                }
            }
            for (int i = 0; i < n; i++)
            {
                smoothed[i] /= overlaps[i];
            }

            // 4. The shapes also say how a week follows from the window - 1 weeks before it
            //    (the linear recurrence). Use that rule to continue the smoothed series.
            double lastWeekShare = 0;
            for (int c = 0; c < rank; c++)
            {
                lastWeekShare += shapes[window - 1, c] * shapes[window - 1, c];
            }
            if (lastWeekShare >= 1 - 1e-9)
            {
                return null; // the rule would divide by ~0
            }

            var rule = new double[window - 1];
            for (int j = 0; j < window - 1; j++)
            {
                double sum = 0;
                for (int c = 0; c < rank; c++)
                {
                    sum += shapes[window - 1, c] * shapes[j, c];
                }
                rule[j] = sum / (1 - lastWeekShare);
            }

            var extended = new List<double>(smoothed);
            var expected = new double[horizon];
            for (int t = 0; t < horizon; t++)
            {
                double next = 0;
                for (int j = 0; j < window - 1; j++)
                {
                    next += rule[j] * extended[extended.Count - (window - 1) + j];
                }
                extended.Add(next);
                expected[t] = next;
            }

            // The range: how far real weeks landed from the smoothed line in the past
            double squared = 0;
            for (int i = 0; i < n; i++)
            {
                squared += (series[i] - smoothed[i]) * (series[i] - smoothed[i]);
            }
            var spread = Z95 * Math.Sqrt(squared / n);

            return new SsaForecast(
                expected,
                expected.Select(e => Math.Max(0, e - spread)).ToArray(),
                expected.Select(e => e + spread).ToArray()
            );
        }

        // Eigen-decomposition of a symmetric matrix (Jacobi rotations).
        // Returns the eigenvectors as columns, sorted by eigenvalue, largest first.
        private static double[,] Eigenvectors(double[,] matrix)
        {
            int size = matrix.GetLength(0);
            var a = (double[,])matrix.Clone();
            var v = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                v[i, i] = 1;
            }

            for (int sweep = 0; sweep < 100; sweep++)
            {
                double offDiagonal = 0;
                for (int p = 0; p < size; p++)
                {
                    for (int q = p + 1; q < size; q++)
                    {
                        offDiagonal += a[p, q] * a[p, q];
                    }
                }
                if (offDiagonal < 1e-18)
                {
                    break;
                }

                for (int p = 0; p < size; p++)
                {
                    for (int q = p + 1; q < size; q++)
                    {
                        if (Math.Abs(a[p, q]) < 1e-300)
                        {
                            continue;
                        }

                        double theta = (a[q, q] - a[p, p]) / (2 * a[p, q]);
                        double t = theta == 0 ? 1 : Math.Sign(theta) / (Math.Abs(theta) + Math.Sqrt(theta * theta + 1));
                        double cos = 1 / Math.Sqrt(t * t + 1);
                        double sin = t * cos;

                        for (int k = 0; k < size; k++)
                        {
                            double akp = a[k, p],
                                akq = a[k, q];
                            a[k, p] = cos * akp - sin * akq;
                            a[k, q] = sin * akp + cos * akq;
                        }
                        for (int k = 0; k < size; k++)
                        {
                            double apk = a[p, k],
                                aqk = a[q, k];
                            a[p, k] = cos * apk - sin * aqk;
                            a[q, k] = sin * apk + cos * aqk;
                        }
                        for (int k = 0; k < size; k++)
                        {
                            double vkp = v[k, p],
                                vkq = v[k, q];
                            v[k, p] = cos * vkp - sin * vkq;
                            v[k, q] = sin * vkp + cos * vkq;
                        }
                    }
                }
            }

            var order = Enumerable.Range(0, size).OrderByDescending(i => a[i, i]).ToArray();
            var sorted = new double[size, size];
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    sorted[k, j] = v[k, order[j]];
                }
            }
            return sorted;
        }
    }
}
