namespace HongQuant.Platform.ViewModels.Original.ShowCharts;
public class WithdrawDeepChartData : BaseLineChartData
{
    public WithdrawDeepChartData(List<BasicTransformDataViewModel>? statistics) : base(statistics)
    {
    }

    protected override IEnumerable<double> GetSeries(List<BasicTransformDataViewModel> statistics)
    {
        return statistics.Select(x =>  Math.Round(x.WithdrawDeep * 100, 2));
    }
}
