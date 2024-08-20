using DynamicData;
using HongQuant.Core.Services;
using HongQuant.Platform.ViewModels.RealTime.Items;
using LiveChartsCore;
using LiveChartsCore.ConditionalDraw;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using SkiaSharp;
using Splat;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.RealTime;
public class ProfitAndLossViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposed;

    public ISeries[] Series { get; set; }
    public ISeries[] DaySeries { get; set; }
    public ISeries[] WeekSeries { get; set; }
    public ISeries[] MonthSeries { get; set; }

    public Axis[] XAxes { get; set; } = [new Axis { 
        Labeler = va=>""//隐藏x轴的效果
    }];

    public Axis[] YAxes { get; set; } = [new Axis { 
        //SeparatorsPaint = new SolidColorPaint(new SKColor(220, 220, 220)) ,
        ShowSeparatorLines = false,
        Labeler = va=>""//隐藏Y轴的效果
    }];

    public ProfitAndLossViewModel()
    {
        var realTimeDataService = Locator.Current.GetService<IRealTimeDataService>()!;

        var dataDisposed = realTimeDataService.RealTimeDataSources
            .Connect()
            .Transform(x => new AnalyzedDataViewModel(x))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out ReadOnlyObservableCollection<AnalyzedDataViewModel> data)
            .DisposeMany()
            .Subscribe();

        Series = [CreateColumnSeries()];
        DaySeries = [CreateColumnSeries()];
        WeekSeries = [CreateColumnSeries()];
        MonthSeries = [CreateColumnSeries()];

        var timerDisposed = Observable.Interval(TimeSpan.FromSeconds(2)).Subscribe(x =>
        {
            Series[0].Values = GetValue(data,d=>d.ProfitAndLoss);
            DaySeries[0].Values = GetValue(data,d=>d.DayProfitAndLoss);
            WeekSeries[0].Values = GetValue(data,d=>d.WeekProfitAndLoss);
            MonthSeries[0].Values = GetValue(data,d=>d.MonthProfitAndLoss);
        });

        _disposed = new CompositeDisposable(dataDisposed, timerDisposed);
    }

    private static ProfitAndLossInfo[] GetValue(ReadOnlyObservableCollection<AnalyzedDataViewModel> data, Func<AnalyzedDataViewModel, ProfitAndLossInfo> func)
    {
        return [.. data
                .Where(x => x.HasPosition())
                .Select(x => func(x))
                .OrderByDescending(x => x.Value)];
    }

    private static ColumnSeries<ProfitAndLossInfo> CreateColumnSeries()
    {
        var columnSeries = new ColumnSeries<ProfitAndLossInfo>
        {
            DataLabelsPaint = new SolidColorPaint(new SKColor(45, 45, 45)),
            DataLabelsPosition = DataLabelsPosition.Start,
            DataLabelsFormatter = point => $"{point.Model!.Name} \r\n {point.Coordinate.PrimaryValue:F0}",
            MaxBarWidth = 50,
            Padding = 8,
        };

        columnSeries.OnPointMeasured(point =>
        {
            if (point.Visual is null) return;
            var paint = point.Model!.Value > 0 ?
                new SolidColorPaint(new SKColor(250, 0, 0)) :
                new SolidColorPaint(new SKColor(0, 250, 0));
            point.Visual.Fill = paint;
        });
        return columnSeries;
    }

    public void Dispose()
    {
        _disposed.Dispose();
    }
}
