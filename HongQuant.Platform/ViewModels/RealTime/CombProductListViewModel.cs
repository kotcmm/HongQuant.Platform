using DynamicData;
using HongQuant.Core.Services;
using HongQuant.Platform.ViewModels.RealTime.Items;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.RealTime;
public class CombProductListViewModel : ReactiveObject
{
    private readonly ReadOnlyObservableCollection<CombRealTimeDataSourceViewModel> _productCombIndexs;
    public ReadOnlyObservableCollection<CombRealTimeDataSourceViewModel> ProductCombIndexs => _productCombIndexs;
    
    [Reactive]
    public CombRealTimeDataSourceViewModel? ProductIndex { get; set; }

    public CombProductListViewModel()
    {
        var realTimeDataService = Locator.Current.GetService<IRealTimeDataService>()!;

        realTimeDataService.CombRealTimeDataSources
            .Connect()
            .Transform(f => new CombRealTimeDataSourceViewModel(f))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _productCombIndexs)
            .DisposeMany()
            .Subscribe();
    }
}
