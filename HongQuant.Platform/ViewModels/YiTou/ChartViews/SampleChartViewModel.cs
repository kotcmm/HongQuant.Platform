using DynamicData;
using HongQuant.Core.Services;
using HongQuant.Platform.ViewModels.YiTou.IncomeStatistics;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;
using Splat;
using System.Collections.ObjectModel;

namespace HongQuant.Platform.ViewModels.YiTou.ChartViews;
public class SampleChartViewModel : ReactiveObject
{
    [Reactive]
    public IEnumerable<Axis>? TotalXAxes { get; set; }

    [Reactive]
    public ObservableCollection<ISeries>? TotalSeries { get; set; }

    [Reactive]
    public int ShowField { get; set; } = 0;

    public SampleChartViewModel()
    {
        var cycleIncomeService = Locator.Current.GetService<ICycleIncomeService>()!;

        cycleIncomeService.FinishStatistics
              .Connect()
              .Transform(x => new ChartStatisticsViewModel
              {
                  TotalValue = new ObservablePoint(x.Index, Math.Round(x.Result.Total, 2)),
                  MaxTotalValue = new ObservablePoint(x.Index, Math.Round(x.Result.MaxTotal, 2)),
                  MaxRetracementValue = new ObservablePoint(x.Index, Math.Round(x.Result.MaxRetracement, 2)),
                  DayCountValue = new ObservablePoint(x.Index, x.Result.DayCount),
                  LastMultiple = new ObservablePoint(x.Index, x.Result.LastMultiples),
                  StartDate = x.Result.Start,
                  EndDate = x.Result.End,
              })
              .Bind(out ReadOnlyObservableCollection<ChartStatisticsViewModel> statistics)
              .DisposeMany()
              .Subscribe();

        this.WhenAnyValue(x => x.ShowField).Subscribe(x =>
        {
            TotalSeries = GetTotalSeries(x, statistics);

            TotalXAxes = GetTotalXAxes(statistics);
        });
    }

    private static List<Axis> GetTotalXAxes(ReadOnlyObservableCollection<ChartStatisticsViewModel> statistics)
    {
        return [new()
        {
            Labeler = (p) =>
            {
                if (p > 0 && statistics.Count > p && statistics.Count > 0)
                {
                    var data = statistics[(int)p];
                    return data.StartDate.Date.ToString("yyyyMMdd");
                }
                return "";
            },

            MinStep = 1,
            LabelsRotation = 30
        },
        ];
    }

    private static ObservableCollection<ISeries> GetTotalSeries(int x, ReadOnlyObservableCollection<ChartStatisticsViewModel> statistics)
    {
        return x switch
        {
            0 => [new ColumnSeries<ObservablePoint>
            {
                Name = "累计收益",
                Values = statistics.Select(x => x.TotalValue),
                Fill = new SolidColorPaint(new SKColor(25, 118, 210)),
                XToolTipLabelFormatter = (chartPoint) => TooltipLabelFormatter(chartPoint, statistics),
            }],
            1 => [new ColumnSeries<ObservablePoint>
            {
                Name = "最大回撤",
                Values = statistics.Select(x => x.MaxRetracementValue),
                Fill = new SolidColorPaint(new SKColor(255, 21, 14)),
                XToolTipLabelFormatter = (chartPoint) => TooltipLabelFormatter(chartPoint, statistics),
            }],
            2 => [new ColumnSeries<ObservablePoint>
            {
                Name = "最大收益",
                Values = statistics.Select(x => x.MaxTotalValue),
                Fill = new SolidColorPaint(new SKColor(12, 105, 255)),
                XToolTipLabelFormatter = (chartPoint) => TooltipLabelFormatter(chartPoint, statistics),
            }],
            3 => [new ColumnSeries<ObservablePoint>
            {
                Name = "交易天数",
                Values = statistics.Select(x => x.DayCountValue),
                Fill = new SolidColorPaint(new SKColor(255, 128, 0)),
                XToolTipLabelFormatter = (chartPoint) => TooltipLabelFormatter(chartPoint, statistics),
            }],
            4 => [new ColumnSeries<ObservablePoint>
            {
                Name = "累计杠杆",
                Values = statistics.Select(x => x.LastMultiple),
                Fill = new SolidColorPaint(new SKColor(12, 105, 255)),
                XToolTipLabelFormatter = (chartPoint) => TooltipLabelFormatter(chartPoint, statistics),
            }],
            _ => throw new NotImplementedException(),
        };
    }

    private static string TooltipLabelFormatter(ChartPoint<ObservablePoint, RoundedRectangleGeometry, LabelGeometry> chartPoint, ReadOnlyObservableCollection<ChartStatisticsViewModel> statistics)
    {
        var index = chartPoint.Index;
        if (index == -1) return $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate}";

        return $" {statistics[index].StartDate.Date:yyyyMMdd} -" +
               $" {statistics[index].EndDate.Date:yyyyMMdd} ";
    }
}
