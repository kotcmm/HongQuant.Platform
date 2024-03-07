using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace HongQuant.Platform.ViewModels.Original.ShowCharts;
public abstract class BaseLineChartData : ReactiveObject, IChartData
{
    public BaseLineChartData(List<BasicTransformDataViewModel>? statistics)
    {
        UpdateData(statistics);
    }

    [Reactive]
    public IEnumerable<Axis>? TotalXAxes { get; set; }

    [Reactive]
    public IEnumerable<Axis>? TotalYAxes { get; set; }

    [Reactive]
    public ObservableCollection<ISeries>? TotalSeries { get; set; }

    protected virtual void UpdateData(List<BasicTransformDataViewModel>? statistics)
    {
        if (statistics == null) return;

        TotalXAxes = GetTotalXAxes(statistics);

        TotalYAxes = GetTotalYAxes();

        TotalSeries = GetTotalSeries(statistics);
    }

    protected virtual List<Axis> GetTotalXAxes(List<BasicTransformDataViewModel> statistics)
    {
        var dates = statistics.Select(x => x.TradeDate.ToString("yyyyMMdd")).ToList();

        return [new()
        {
            Labels = dates,
            MinStep = 1,
            LabelsRotation = 30
        },];
    }

    protected virtual List<Axis> GetTotalYAxes()
    {
        return [new()
        {
            Labeler = (p)=> $"{p}%",
        },];
    }

    protected abstract IEnumerable<double> GetSeries(List<BasicTransformDataViewModel> statistics);

    protected virtual ObservableCollection<ISeries> GetTotalSeries(List<BasicTransformDataViewModel> statistics)
    {
        var series = GetSeries(statistics);

        return [new LineSeries<double>
            {
                Values = series,
                GeometrySize = 0,
                LineSmoothness = 1,
                Fill = null,
                Stroke = new SolidColorPaint(new SKColor(255, 21, 14)),
            }];
    }
}
