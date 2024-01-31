using HongQuant.Analysis.YiTou.IncomeStatistics;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HongQuant.Platform.ViewModels.YiTou.IncomeStatistics;
public class IncomeStatisticsViewModel : ReactiveObject
{
    [Reactive]
    public DateTime Start { get; set; }

    [Reactive]
    public DateTime End { get; set; }

    [Reactive]
    public double Total { get; set; }

    [Reactive]
    public int DayCount { get; set; }

    [Reactive]
    public double MaxTotal { get; set; }

    [Reactive]
    public DateTime MaxTotalDate { get; set; }

    [Reactive]
    public double MaxRetracement { get; set; }

    [Reactive]
    public DateTime RetracementDate { get; set; }

    public NetWorthInfo NetWorthInfo { get; set; } = null!;

    [Reactive]
    public double LastMultiples { get; set; }

    public BaseIncomeStatistics Statistics { get; set; } = null!;
}
