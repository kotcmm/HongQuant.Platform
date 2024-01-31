using HongQuant.Analysis.YiTou.Configs;

namespace HongQuant.Platform.ViewModels.YiTou.Configs;
public interface IConfigIncomeViewModel
{
    IObservable<IConfigIncome> Change { get; }

    IConfigIncome Config { get; }
}
