namespace HongQuant.Platform.ViewModels.Original.ShowCharts;
public class TotalYieldsChartData : BaseLineChartData
{
    public TotalYieldsChartData(List<BasicTransformDataViewModel>? statistics) :
        base(statistics)
    {
    }

    protected override IEnumerable<double> GetSeries(List<BasicTransformDataViewModel> statistics)
    {
        return statistics.Select(x => Math.Round(x.TotalYields * 100, 2));
    }
}
