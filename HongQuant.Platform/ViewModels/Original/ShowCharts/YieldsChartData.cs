using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.Original.ShowCharts;
public class YieldsChartData : ReactiveObject, IChartData
{
    private List<BasicTransformDataViewModel>? _statistics;

    public ICommand FieldChangeCommand { get; }

    public YieldsChartData(List<BasicTransformDataViewModel>? statistics)
    {
        _statistics = statistics;

        FieldChangeCommand = ReactiveCommand.Create<string>(FieldChange);

        FieldChange();
    }

    [Reactive]
    public IEnumerable<Axis>? TotalXAxes { get; set; }

    [Reactive]
    public IEnumerable<Axis>? TotalYAxes { get; set; }

    [Reactive]
    public ObservableCollection<ISeries>? TotalSeries { get; set; }

    private void FieldChange(string select = "1")
    {
        if (_statistics == null) return;

        var showField = int.Parse(select);

        var group = _statistics.GroupBy(x => showField switch
        {
            1 => x.TradeDate.ToString("yyyy"),
            2 => x.TradeDate.ToString("yyyyMM"),
            3 => x.TradeDate.ToString("yyyyMMdd"),
            _ => throw new ArgumentException()
        });

        TotalXAxes = GetTotalXAxes(group);

        TotalYAxes = GetTotalYAxes();

        TotalSeries = GetTotalSeries(group);
    }

    private static ObservableCollection<ISeries>? GetTotalSeries(IEnumerable<IGrouping<string, BasicTransformDataViewModel>> group)
    {
        var yields = group
            .Select((x, i) =>
            {
                var sumYields = Math.Round(x.Sum(y => y.Yields) * 100, 2);
                return new ObservablePoint(i, sumYields);
            })
            .ToList();

        return [new ColumnSeries<ObservablePoint>
        {
            Values = yields,
            Fill = new SolidColorPaint(new SKColor(255, 21, 14)),
        }];
    }

    private static IEnumerable<Axis>? GetTotalYAxes()
    {
        return [new()
        {
            Labeler = (p)=> $"{p}%",
        },];
    }

    private static IEnumerable<Axis>? GetTotalXAxes(IEnumerable<IGrouping<string, BasicTransformDataViewModel>> group)
    {
        var dates = group.Select(x => x.Key).ToList();

        return [new()
        {
            Labels = dates,
            MinStep = 1,
            LabelsRotation = 30
        },];
    }

}
