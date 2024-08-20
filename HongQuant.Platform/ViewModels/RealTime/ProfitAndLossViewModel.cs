using DynamicData;
using HongQuant.Analysis.RealTime;
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
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.RealTime;
public class ProfitAndLossViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposed;

    private CycleType _cycleType = CycleType.HalfAnHour;

    public ISeries[] Series { get; set; }

    public Axis[] XAxes { get; set; } = [new Axis { 
        Labeler = va=>""//隐藏x轴的效果
    }];

    public Axis[] YAxes { get; set; } = [new Axis { 
        SeparatorsPaint = new SolidColorPaint(new SKColor(220, 220, 220)) ,
    }];

    public ICommand UpdateCycleCommand { get; set; }

    public ProfitAndLossViewModel()
    {
        var realTimeDataService = Locator.Current.GetService<IRealTimeDataService>()!;

        var dataDisposed = realTimeDataService.RealTimeDataSources
            .Connect()
            .Transform(x =>new ProfitAndLossInfo(x, GetProfitAndLoss))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out ReadOnlyObservableCollection<ProfitAndLossInfo> data)
            .DisposeMany()
            .Subscribe();

        var rowSeries = new ColumnSeries<ProfitAndLossInfo>
        {
            DataLabelsPaint = new SolidColorPaint(new SKColor(45, 45, 45)),
            DataLabelsPosition = DataLabelsPosition.Start,
            //DataLabelsTranslate = new(1, 0),
            DataLabelsFormatter = point => $"{point.Model!.Name} \r\n {point.Coordinate.PrimaryValue:F0}",
            MaxBarWidth = 50,
            Padding = 8,
        };

        rowSeries.OnPointMeasured(point =>
        {
            if (point.Visual is null) return;
            var paint = point.Model!.Value > 0 ? 
                new SolidColorPaint(new SKColor(250, 0, 0)) :
                new SolidColorPaint(new SKColor(0, 250, 0));
            point.Visual.Fill = paint;
        });

        Series = [rowSeries];

        var timerDisposed = Observable.Interval(TimeSpan.FromSeconds(2)).Subscribe(x =>
        {
            Series[0].Values = data
            .Where(x=>x.Position != 0)
            .OrderByDescending(x => x.Value)
            .ToArray();
        });

        _disposed = new CompositeDisposable(dataDisposed,timerDisposed);

        UpdateCycleCommand = ReactiveCommand.Create<CycleType>(cycleType =>
        {
            _cycleType = cycleType;

            foreach (var item in data)
            {
                item.UpdateValue(cycleType);
            }
        });
    }

    private double GetProfitAndLoss(AnalyzedData analyzedData)
    {
        return _cycleType switch
        {
            CycleType.HalfAnHour => analyzedData.ProfitAndLoss,
            CycleType.Day => analyzedData.DayProfitAndLoss,
            CycleType.Week => analyzedData.WeekProfitAndLoss,
            CycleType.Month => analyzedData.MonthProfitAndLoss,
            _ => throw new NotImplementedException()
        };
    }

    public void Dispose()
    {
        _disposed.Dispose();
    }
}
