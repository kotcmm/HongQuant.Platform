using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.Original;
public class ShowChartViewModel : ReactiveObject
{
    [Reactive]
    public IEnumerable<Axis>? TotalXAxes { get; set; }

    [Reactive]
    public ObservableCollection<ISeries>? TotalSeries { get; set; }

    public int ShowField { get; set; } = 0;

    public ICommand FieldChangeCommand { get; }

    public ShowChartViewModel(ShowDataViewModel showDataViewModel)
    {
        showDataViewModel.WhenAnyValue(x => x.SelectFileIndex)
            .Subscribe(x =>
            {
                _statistics = x?.Items?.ToList();

                if (_statistics != null)
                {
                    TotalXAxes = GetTotalXAxes(_statistics);
                    TotalSeries = GetTotalSeries(ShowField, _statistics);
                }
            });

        FieldChangeCommand = ReactiveCommand.Create<string>(FieldChange);
    }

    private List<BasicTransformDataViewModel>? _statistics;

    private void FieldChange(string showField)
    {
        ShowField = int.Parse(showField);

        if (_statistics != null)
        {
            TotalSeries = GetTotalSeries(ShowField, _statistics);
        }
    }

    private static List<Axis> GetTotalXAxes(List<BasicTransformDataViewModel> statistics)
    {
        return [new()
        {
            Labeler = (p) =>
            {
                if (p > 0 && statistics.Count > p && statistics.Count > 0)
                {
                    var data = statistics[(int)p];
                    return data.TradeDate.ToString("yyyyMMdd");
                }
                return "";
            },

            MinStep = 1,
            LabelsRotation = 30
        },
        ];
    }

    private static ObservableCollection<ISeries> GetTotalSeries(int x, List<BasicTransformDataViewModel> statistics)
    {
        return x switch
        {
            0 => [new ColumnSeries<ObservablePoint>
            {
                Name = "净利润",
                Values = statistics.Select((x,i)=> new ObservablePoint(i, Math.Round(x.TodayProfitAndLoss, 2))),
                Fill = new SolidColorPaint(new SKColor(25, 118, 210)),
                //XToolTipLabelFormatter = (chartPoint) => TooltipLabelFormatter(chartPoint, statistics),
            }],
            1 => [new ColumnSeries<ObservablePoint>
            {
                Name = "收益率",
                Values = statistics.Select((x,i)=> new ObservablePoint(i, Math.Round(x.Yields, 2))),
                Fill = new SolidColorPaint(new SKColor(255, 21, 14)),
                //XToolTipLabelFormatter = (chartPoint) => TooltipLabelFormatter(chartPoint, statistics),
            }],
            2 => [new ColumnSeries<ObservablePoint>
            {
                Name = "累计收益率",
                Values = statistics.Select((x,i)=> new ObservablePoint(i, Math.Round(x.TotalYields, 2))),
                Fill = new SolidColorPaint(new SKColor(12, 105, 255)),
                //XToolTipLabelFormatter = (chartPoint) => TooltipLabelFormatter(chartPoint, statistics),
            }],
            3 => [new ColumnSeries<ObservablePoint>
            {
                Name = "实时回撤",
                Values = statistics.Select((x,i)=> new ObservablePoint(i, Math.Round(x.WithdrawDeep, 2))),
                Fill = new SolidColorPaint(new SKColor(255, 128, 0)),
                //XToolTipLabelFormatter = (chartPoint) => TooltipLabelFormatter(chartPoint, statistics),
            }],
            4 => [new ColumnSeries<ObservablePoint>
            {
                Name = "最大回撤",
                Values = statistics.Select((x,i)=> new ObservablePoint(i, Math.Round(x.MaxWithdraw, 2))),
                Fill = new SolidColorPaint(new SKColor(12, 105, 255)),
                //XToolTipLabelFormatter = (chartPoint) => TooltipLabelFormatter(chartPoint, statistics),
            }],
            _ => throw new NotImplementedException(),
        };
    }

    //private static string TooltipLabelFormatter(ChartPoint<ObservablePoint, RoundedRectangleGeometry, LabelGeometry> chartPoint, List<BasicTransformDataViewModel> statistics)
    //{
    //    var index = chartPoint.Index;
    //    if (index == -1) return $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate}";

    //    return $" {statistics[index].StartDate.Date:yyyyMMdd} -" +
    //           $" {statistics[index].EndDate.Date:yyyyMMdd} ";
    //}
}
