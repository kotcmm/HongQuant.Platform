using HongQuant.Analysis.YiTou.Configs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.YiTou.Configs;

public class CycleConfigIncomeViewModel : ReactiveObject, IConfigIncomeViewModel
{
    [Reactive]
    public int Month { get; set; }

    public IObservable<IConfigIncome> Change { get; }

    private readonly CycleConfigIncome _configIncome = new();
    public IConfigIncome Config => _configIncome;

    public CycleConfigIncomeViewModel()
    {
        Month = _configIncome.Month;

        Change = Observable.Create<IConfigIncome>(o =>
        {
            this.WhenAnyValue(x => x.Month)
            .Subscribe(x =>
            {
                _configIncome.Month = Month;

                o.OnNext(Config);
            });

            return Disposable.Empty;
        });
    }
}
