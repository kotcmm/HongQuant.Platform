using HongQuant.Analysis.YiTou.Configs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.YiTou.Configs;

public class Multiple5ConfigIncomeViewModel : ReactiveObject, IConfigIncomeViewModel
{
    /// <summary>
    /// 设置最多盈利
    /// </summary>
    [Reactive]
    public double MaxIncome { get; set; }

    /// <summary>
    /// 初始倍数
    /// </summary>
    [Reactive]
    public double Start { get; set; }

    /// <summary>
    /// 倍数增量
    /// </summary>
    [Reactive]
    public double Increment { get; set; }

    /// <summary>
    /// 倍数上限
    /// </summary>
    [Reactive]
    public double Max { get; set; }

    /// <summary>
    /// 倍数增加间隔
    /// </summary>
    [Reactive]
    public int Month { get; set; }
    /// <summary>
    /// 是否有低仓
    /// </summary>
    [Reactive]
    public bool Have { get; set; }

    /// <summary>
    /// 复利系数
    /// </summary>
    [Reactive]
    public double NetWorthIncrement { get; set; }

    [Reactive]
    public bool HaveNetWorth { get; set; }

    [Reactive]
    public bool UseTime { get; set; }

    [Reactive]
    public bool UseIncome { get; set; }

    [Reactive]
    public int MaxTime { get; set; }

    public IObservable<IConfigIncome> Change { get; protected set; }

    private readonly Multiple5ConfigIncome _configIncome = new();
    public IConfigIncome Config => _configIncome;

    public Multiple5ConfigIncomeViewModel()
    {
        MaxIncome = _configIncome.MaxIncome;
        Start = _configIncome.Start;
        Increment = _configIncome.Increment;
        Max = _configIncome.Max;
        Month = _configIncome.Month;
        Have = _configIncome.Have;
        NetWorthIncrement = _configIncome.NetWorthIncrement;
        HaveNetWorth = _configIncome.HaveNetWorth;
        UseTime = _configIncome.UseTime;
        UseIncome = _configIncome.UseIncome;
        MaxTime = _configIncome.MaxTime;

        Change = Observable.Create<IConfigIncome>(o =>
        {
            this.WhenAnyValue(x => x.MaxIncome, x => x.Start,
                x => x.HaveNetWorth, x => x.Max, x => x.Month,
                x => x.Have, x => x.NetWorthIncrement)
             .Subscribe(x =>
             {
                 _configIncome.MaxIncome = MaxIncome;
                 _configIncome.Start = Start;
                 _configIncome.HaveNetWorth = HaveNetWorth;
                 _configIncome.Max = Max;
                 _configIncome.Month = Month;
                 _configIncome.Have = Have;
                 _configIncome.NetWorthIncrement = NetWorthIncrement;
                 o.OnNext(Config);
             });

            this.WhenAnyValue(x => x.UseIncome, x => x.MaxIncome, x => x.Increment)
             .Subscribe(x =>
             {
                 _configIncome.UseIncome = UseIncome;
                 _configIncome.MaxIncome = MaxIncome;
                 _configIncome.Increment = Increment;
                 o.OnNext(Config);
             });

            this.WhenAnyValue(x => x.UseTime, x => x.MaxTime)
             .Subscribe(x =>
             {
                 _configIncome.UseTime = UseTime;
                 _configIncome.MaxTime = MaxTime;

                 o.OnNext(Config);
             });

            return Disposable.Empty;
        });

    }
}
