using HongQuant.Platform.ViewModels.YiTou.ChartViews;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.YiTou;
public class CycleIncomeBarChartViewModel : ReactiveObject
{
    [Reactive]
    public NetWorthAndRateChartViewModel? NetWorthAndRateChartViewModel { get; set; }

    public CycleIncomeBarChartViewModel(YiTouMainViewModel mainViewModel)
    {
        mainViewModel.WhenAnyValue(x=>x.IncomeStatistic).Subscribe(x =>
        {
            if(x != null)
                NetWorthAndRateChartViewModel = new NetWorthAndRateChartViewModel(x.Statistics);
        });  
    }
}
