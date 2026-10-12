using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

namespace ERP.Repository.Services.SSA
{
    // One SSA forecast: a value per future week, with a 95% range around it
    public record SsaForecast(double[] Expected, double[] Low, double[] BusyCase);

    // Singular Spectrum Analysis through ML.NET's ForecastBySsa (Microsoft.ML.TimeSeries).
    //
    // In plain words: cut the history into overlapping slices of `window` weeks, find the few
    // repeating shapes that explain most of those slices (the trend and the yearly season),
    // and use them to continue the series into the future. It is statistics, re-fitted from
    // scratch on every call; nothing is kept between runs.
    //
    // Out of the box ML.NET lost to a simple 4-week average on our data: it continues from the
    // raw recent weeks (copying their random wobble forward) and its default "stabilize" step
    // flattens the yearly season. So we feed it a 4-week average of the history instead of the
    // raw weeks (like textbook SSA continuing its smoothed line) and turn stabilizing off.
    // ML.NET's own range would then be too narrow, so the range is computed here, the same way
    // as master: from how far the real weeks landed from the smoothed line.
    public static class SsaForecaster
    {
        private const double Z95 = 1.96; // 95% of a normal spread is within 1.96 standard deviations
        private const int SmoothingWeeks = 4;

        // Returns null when the history can't be modelled (too short, all zeros, or ML.NET
        // rejects it); the caller then falls back to the simple average.
        public static SsaForecast? Forecast(IReadOnlyList<double> series, int window, int rank, int horizon)
        {
            int n = series.Count;
            int slices = n - window + 1;
            if (window < 2 || rank < 1 || rank >= window || slices < window || series.All(x => x == 0))
            {
                return null;
            }

            // Each week becomes the average of itself and the weeks just before it
            var smoothed = new double[n];
            for (int i = 0; i < n; i++)
            {
                int from = Math.Max(0, i - SmoothingWeeks + 1);
                double sum = 0;
                for (int j = from; j <= i; j++)
                {
                    sum += series[j];
                }
                smoothed[i] = sum / (i - from + 1);
            }

            // A new context per call: the backtest endpoint runs outside the forecast lock
            var ml = new MLContext(seed: 0);
            var data = ml.Data.LoadFromEnumerable(smoothed.Select(d => new DemandInput { Demand = (float)d }));

            double[] expected;
            try
            {
                var model = ml
                    .Forecasting.ForecastBySsa(
                        outputColumnName: nameof(SsaOutput.Forecast),
                        inputColumnName: nameof(DemandInput.Demand),
                        windowSize: window,
                        seriesLength: n,
                        trainSize: n,
                        horizon: horizon,
                        // Keep exactly `rank` shapes; ML.NET ignores `rank` unless the method is Fixed
                        rankSelectionMethod: RankSelectionMethod.Fixed,
                        rank: rank,
                        // Stabilizing flattens the yearly season (31.7% -> 25.6% error without it)
                        shouldStabilize: false
                    )
                    .Fit(data);

                using var engine = model.CreateTimeSeriesEngine<DemandInput, SsaOutput>(ml);
                expected = engine.Predict().Forecast.Select(x => (double)x).ToArray();
            }
            catch (Exception e) when (e is ArgumentException or InvalidOperationException)
            {
                return null;
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

        private class DemandInput
        {
            public float Demand { get; set; }
        }

        private class SsaOutput
        {
            public float[] Forecast { get; set; } = [];
        }
    }
}
