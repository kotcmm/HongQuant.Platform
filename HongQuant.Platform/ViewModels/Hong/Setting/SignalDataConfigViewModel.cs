using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Subjects;

namespace HongQuant.Platform.ViewModels.Hong.Setting;
public class SignalDataConfigViewModel : ReactiveObject
{
    /// <summary>
    /// 日度等级
    /// </summary>
    [Reactive]
    public int DayMaxLevel { get; set; }

    /// <summary>
    /// 周度等级
    /// </summary>
    [Reactive]
    public int WeekMaxLevel { get; set; }

    /// <summary>
    /// 月度等级
    /// </summary>
    [Reactive]
    public int MonthMaxLevel { get; set; }

    /// <summary>
    /// 开仓条件
    /// </summary>
    [Reactive]
    public double Entrance { get; set; }

    /// <summary>
    /// 平仓条件
    /// </summary>
    [Reactive]
    public double Quit { get; set; }

    /// <summary>
    /// 止损条件
    /// </summary>
    [Reactive]
    public double Stop { get; set; }

    /// <summary>
    /// 优选边界
    /// </summary>
    [Reactive]
    public int MaxCombo { get; set; }

    [Reactive]
    public int SelectTop { get; set; }

    private readonly Subject<SignalDataConfigViewModel> _changeSubject = new();
    public IObservable<SignalDataConfigViewModel> Change => _changeSubject;

    public SignalDataConfigViewModel()
    {
        this.WhenAnyValue(x => x.DayMaxLevel).Subscribe(x =>
        {
            _changeSubject.OnNext(this);
        });

        this.WhenAnyValue(x => x.WeekMaxLevel).Subscribe(x =>
        {
            _changeSubject.OnNext(this);
        });

        this.WhenAnyValue(x => x.MonthMaxLevel).Subscribe(x =>
        {
            _changeSubject.OnNext(this);
        });

        this.WhenAnyValue(x => x.Entrance).Subscribe(x =>
        {
            _changeSubject.OnNext(this);
        });

        this.WhenAnyValue(x => x.Quit).Subscribe(x =>
        {
            _changeSubject.OnNext(this);
        });

        this.WhenAnyValue(x => x.Stop).Subscribe(x =>
        {
            _changeSubject.OnNext(this);
        });

        this.WhenAnyValue(x => x.MaxCombo).Subscribe(x =>
        {
            _changeSubject.OnNext(this);
        });

        this.WhenAnyValue(x => x.SelectTop).Subscribe(x =>
        {
            _changeSubject.OnNext(this);
        });
    }
}
