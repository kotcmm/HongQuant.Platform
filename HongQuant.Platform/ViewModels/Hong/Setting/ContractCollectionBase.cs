using DynamicData;
using HongQuant.Core.Services;
using HongQuant.Platform.ViewModels.Hong.Setting.ContractConfigViewModels;
using ReactiveUI;
using Splat;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.Hong.Setting;
public abstract class ContractCollectionBase : ReactiveObject, IDisposable
{
    private readonly IDisposable _cleanUp;
    protected readonly IContractConfigService _hongContractConfigService;

    private readonly ReadOnlyObservableCollection<ContractTypeViewModel> _contractTypes;
    public ReadOnlyObservableCollection<ContractTypeViewModel> ContractTypes => _contractTypes;

    protected ContractCollectionBase()
    {
        _hongContractConfigService = Locator.Current.GetService<IContractConfigService>()!;

        _cleanUp = _hongContractConfigService.ContractTypes.Connect()
              .Transform(type => type.Bulid())
              .ObserveOn(RxApp.MainThreadScheduler)
              .Bind(out _contractTypes)
              .DisposeMany()
              .Subscribe();
    }

    public void Dispose()
    {
        _cleanUp.Dispose();
    }
}
