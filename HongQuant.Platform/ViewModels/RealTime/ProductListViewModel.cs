using DynamicData;
using HongQuant.Core.Services;
using HongQuant.Platform.ViewModels.RealTime.Items;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.RealTime;
public class ProductListViewModel : ReactiveObject
{
    private readonly ReadOnlyObservableCollection<RealTimeDataSourceViewModel> _productIndexs;
    public ReadOnlyObservableCollection<RealTimeDataSourceViewModel> ProductIndexs => _productIndexs;

    [Reactive]
    public RealTimeDataSourceViewModel? ProductIndex { get; set; }

    public ProductListViewModel()
    {
        var realTimeDataService = Locator.Current.GetService<IRealTimeDataService>()!;

        realTimeDataService.RealTimeDataSources
            .Connect()
            .Transform(f => new RealTimeDataSourceViewModel(f))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _productIndexs)
            .DisposeMany()
            .Subscribe();
    }
}
