using HongQuant.Core.Services;
using HongQuant.Platform.ViewModels.Hong;
using HongQuant.Platform.ViewModels.Hong.Setting;
using HongQuant.Platform.ViewModels.Hong.Setting.ContractCombinationViewModels;
using HongQuant.Platform.ViewModels.Hong.Setting.ContractConfigViewModels;
using HongQuant.Platform.ViewModels.Hong.Shows;
using Splat;

namespace HongQuant.Platform
{
    public class AppBootstrapper : IEnableLogger
    {
        public AppBootstrapper()
        {
            Locator.CurrentMutable.Register(() => new HongMainViewModel());
            Locator.CurrentMutable.Register(() => new ContractConfigViewModel());
            Locator.CurrentMutable.Register(() => new ContractCombinationViewModel()); 
            Locator.CurrentMutable.Register(() => new SignalDataViewModel()); 
            Locator.CurrentMutable.Register(() => new BaseSettingViewModel()); 

            Locator.CurrentMutable.RegisterConstant(new ContractConfigService(), typeof(IContractConfigService));
            Locator.CurrentMutable.RegisterConstant(new CombinationService(), typeof(ICombinationService));
            Locator.CurrentMutable.RegisterConstant(new HongAnalysisService(), typeof(IHongAnalysisService));
        }
    }
}
