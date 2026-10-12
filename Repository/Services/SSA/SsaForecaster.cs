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
    // Unlike the textbook SSA on master, ML.NET continues from the raw recent weeks, not the
    // smoothed line. On our data that lost to a simple 4-week average
    // (docs/ssa-design/mlnet-ssa-test.typ); this branch exists to compare the two.
    public static class SsaForecaster
    {
        private const float ConfidenceLevel = 0.95f;

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

            // A new context per call: the backtest endpoint runs outside the forecast lock
            var ml = new MLContext(seed: 0);
            var data = ml.Data.LoadFromEnumerable(series.Select(d => new DemandInput { Demand = (float)d }));

            SsaOutput output;
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
                        confidenceLevel: ConfidenceLevel,
                        confidenceLowerBoundColumn: nameof(SsaOutput.Low),
                        confidenceUpperBoundColumn: nameof(SsaOutput.BusyCase)
                    )
                    .Fit(data);

                using var engine = model.CreateTimeSeriesEngine<DemandInput, SsaOutput>(ml);
                output = engine.Predict();
            }
            catch (Exception e) when (e is ArgumentException or InvalidOperationException)
            {
                return null;
            }

            return new SsaForecast(
                output.Forecast.Select(x => (double)x).ToArray(),
                output.Low.Select(x => Math.Max(0, (double)x)).ToArray(),
                output.BusyCase.Select(x => (double)x).ToArray()
            );
        }

        private class DemandInput
        {
            public float Demand { get; set; }
        }

        private class SsaOutput
        {
            public float[] Forecast { get; set; } = [];
            public float[] Low { get; set; } = [];
            public float[] BusyCase { get; set; } = [];
        }
    }
}
