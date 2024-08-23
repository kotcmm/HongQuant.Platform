using DynamicData;
using HongQuant.Analysis.RealTime;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.RealTime.Items;
public class CombRealTimeDataSourceViewModel
{
    public CombRealTimeDataSourceViewModel(CombRealTimeDataSource realTimeData)
    {
        Product = realTimeData.Product;
        Strategy = realTimeData.Strategy;

        realTimeData.AnalyzedDatas
            .Connect()
            .Transform(x => new CombRealTimeDataViewModel(x))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _items)
            .DisposeMany()
            .Subscribe();
    }

    public bool IsSelected { get; set; }

    public string Product { get; set; }

    public string Strategy { get; set; }

    private readonly ReadOnlyObservableCollection<CombRealTimeDataViewModel> _items;
    public ReadOnlyObservableCollection<CombRealTimeDataViewModel> Items => _items;
}
