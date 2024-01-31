using HongQuant.Analysis.YiTou.IncomeStatistics;
using HongQuant.Platform.Infrastucture;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using OfficeOpenXml;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.YiTou.ChartViews;
public class NetWorthAndRateChartViewModel : ReactiveObject
{
    [Reactive]
    public ObservableCollection<ISeries>? Series { get; set; }

    [Reactive]
    public IEnumerable<Axis>? XAxes { get; set; }

    [Reactive]
    public IEnumerable<Axis>? YAxes { get; set; }

    [Reactive]
    public IEnumerable<Axis>? YAxes1 { get; set; }

    [Reactive]
    public ObservableCollection<ISeries>? LineSeries { get; set; }

    public ICommand SaveFile { get; }

    public NetWorthAndRateChartViewModel(BaseIncomeStatistics statistics)
    {
        SaveFile = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await Interactions.YiTouOpenFileDialog.Handle("");

            if (!string.IsNullOrEmpty(result))
            {
                Save(result);
            }
        });

        XAxes = GetXAxes(statistics);
        YAxes1 = GetYAxes1();
        LineSeries = GetLineSeries(statistics);
        Series = GetSeries(statistics);
        AddIfMaxNetWorths(statistics, LineSeries);
        YAxes = AddIfMultiples(statistics, LineSeries);
    }

    private readonly static SKColor Blue = new(25, 118, 210);
    private readonly static SKColor Yellow = new(255, 201, 14);
    private readonly static SKColor Red = new(255, 21, 14);

    private static List<Axis>? AddIfMultiples(BaseIncomeStatistics statistics, ObservableCollection<ISeries> lineSeries)
    {
        if (statistics is IMultiples multiple)
        {
            lineSeries.Insert(0, new ColumnSeries<double>
            {
                Name = "倍数",
                Values = multiple.Multiples.Select(x => x.Multiples),
                XToolTipLabelFormatter = (chartPoint) =>
                {
                    var index = chartPoint.Index;
                    if (index == -1) return $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate:F}";
                    var data = statistics.IncomeInfoResults[index];
                    return $"{data.Date:yyyyMMdd}";
                },
                ScalesYAt = 1,
                Stroke = null,
                Fill = new SolidColorPaint(Yellow),
            });

            return
                [
                    new() {
                        LabelsPaint = new SolidColorPaint(Blue)
                    },
                    new() {
                        LabelsPaint = new SolidColorPaint(Yellow),
                        Position = LiveChartsCore.Measure.AxisPosition.End
                    }
                ];
        }

        return null;
    }

    private static void AddIfMaxNetWorths(BaseIncomeStatistics statistics, ObservableCollection<ISeries> lineSeries)
    {
        if (statistics is IMaxNetWorths maxNetWorths && maxNetWorths.MaxNetWorthInfos.Count > 0)
        {
            lineSeries.Add(new LineSeries<double>
            {
                Name = "净值突破",
                Values = new ObservableCollection<double>(maxNetWorths.MaxNetWorthInfos.Select(x => x.MaxNetWorth)),
                XToolTipLabelFormatter = (chartPoint) =>
                {
                    var index = chartPoint.Index;
                    if (index == -1) return $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate:F4}";
                    var data = statistics.IncomeInfoResults[index];
                    return $"{data.Date:yyyyMMdd}";
                },
                Fill = null,
                Stroke = new SolidColorPaint(Red, 1),
                GeometryStroke = new SolidColorPaint(Red, 1),
                GeometrySize = 1,
            });
        }
    }

    private static ObservableCollection<ISeries> GetSeries(BaseIncomeStatistics statistics)
    {
        var retracementRates = statistics.IncomeInfoResults.Select(x => x.RetracementRate);

        var netWorths = statistics.IncomeInfoResults.Select(x => x.NetWorth >= 1 ? 0 : ((x.NetWorth - 1) * 100f));

        return [new LineSeries<double>
        {
            Name = "权益回撤",
            Values = new ObservableCollection<double>(retracementRates),
            Fill = null,
            Stroke = new SolidColorPaint(Blue, 1),
            GeometryStroke = new SolidColorPaint(Blue, 1),
            GeometrySize = 1,
            XToolTipLabelFormatter = (chartPoint) =>
            {
                var index = chartPoint.Index;
                if (index == -1) return $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate}";
                var data = statistics.IncomeInfoResults[index];
                return $" {data.Date:yyyyMMdd}";
            },
        },new LineSeries<double>
        {
            Name = "本金回撤",
            Values = new ObservableCollection<double>(netWorths),
            Fill = null,
            Stroke = new SolidColorPaint(Yellow, 1),
            GeometryStroke = new SolidColorPaint(Yellow, 1),
            GeometrySize = 1,
            XToolTipLabelFormatter = (chartPoint) =>
            {
                var index = chartPoint.Index;
                if (index == -1) return $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate}";
                var data = statistics.IncomeInfoResults[index];
                return $" {data.Date:yyyyMMdd}";
            },
        }];
    }

    private static ObservableCollection<ISeries> GetLineSeries(BaseIncomeStatistics statistics)
    {
        var netWorths = statistics.IncomeInfoResults.Select(x => x.NetWorth);

        return [new LineSeries<double>
        {
            Name = "期末净值",
            Values = new ObservableCollection<double>(netWorths),
            Fill = null,
            Stroke = new SolidColorPaint(Blue, 1),
            GeometryStroke = new SolidColorPaint(Blue, 1),
            GeometrySize = 1,
            XToolTipLabelFormatter = (chartPoint) =>
            {
                var index = chartPoint.Index;
                if (index == -1) return $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate}";
                var data = statistics.IncomeInfoResults[index];
                return $" {data.Date:yyyyMMdd}";
            },
        }];
    }

    private static List<Axis> GetYAxes1()
    {
        return [new()
        {
            Labeler = (p) => $"{p:F2}%",
        }];
    }

    private static List<Axis> GetXAxes(BaseIncomeStatistics statistics)
    {
        return [new()
        {
            Labeler = (p) =>
            {
                if (p > 0 && statistics.IncomeInfoResults.Count > p && statistics.IncomeInfoResults.Count > 0)
                {
                    var data = statistics.IncomeInfoResults[(int)p];
                    return data.Date.ToString("yyyyMMdd");
                }
                return "";
            },
            MinStep = 1,
            LabelsRotation = 30
        }];
    }

    public void Save(string path)
    {
        if (LineSeries == null || Series == null || XAxes == null) return;

        using var package = new ExcelPackage();

        var worksheet = package.Workbook.Worksheets.Add("Info");

        worksheet.Cells["A1"].Value = "时间";
        worksheet.Cells["B1"].Value = "倍数";
        worksheet.Cells["C1"].Value = "期末净值";
        worksheet.Cells["D1"].Value = "权益回撤%";
        worksheet.Cells["E1"].Value = "本金回撤%";

        int row = 2;
        int i = 0;

        var multiples = LineSeries[0].Values ?? Enumerable.Empty<object>();

        foreach (var item in multiples)
        {
            worksheet.Cells["A" + row].Value = XAxes.First().Labeler(i++);
            worksheet.Cells["B" + row].Value = item;
            row++;
        }

        row = 2;

        var netWorths = LineSeries[1].Values ?? Enumerable.Empty<object>();

        foreach (var item in netWorths)
        {
            worksheet.Cells["C" + row].Value = item;
            row++;
        }

        row = 2;

        var retracementRates = Series[0].Values ?? Enumerable.Empty<object>();

        foreach (var item in retracementRates)
        {
            worksheet.Cells["D" + row].Value = item;
            row++;
        }

        row = 2;

        var netWorthRetracementRates = Series[1].Values ?? Enumerable.Empty<object>();

        foreach (var item in netWorthRetracementRates)
        {
            worksheet.Cells["E" + row].Value = item;
            row++;
        }

        var xlFile = new FileInfo(path);

        package.SaveAs(xlFile);
    }
}
