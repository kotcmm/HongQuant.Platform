using HongQuant.Core.Services;
using HongQuant.Platform.ViewModels.Hong;
using HongQuant.Platform.ViewModels.Hong.Setting;
using HongQuant.Platform.ViewModels.Hong.Setting.ContractCombinationViewModels;
using HongQuant.Platform.ViewModels.Hong.Setting.ContractConfigViewModels;
using HongQuant.Platform.ViewModels.Hong.Shows;
using HongQuant.Platform.ViewModels.Original;
using HongQuant.Platform.ViewModels.RealTime;
using HongQuant.Platform.ViewModels.YiTou;
using HongQuant.Platform.ViewModels.YiTou.ChartViews;
using Splat;

namespace HongQuant.Platform
{
    public class AppBootstrapper : IEnableLogger
    {
        public AppBootstrapper()
        {
            Locator.CurrentMutable.Register(() => new OriginalMainViewModel());
            Locator.CurrentMutable.Register(() => new ShowDataViewModel());
            Locator.CurrentMutable.Register(() => new ProductTemplateSettingViewModel());

            Locator.CurrentMutable.Register(() => new HongMainViewModel());
            Locator.CurrentMutable.Register(() => new ContractConfigViewModel());
            Locator.CurrentMutable.Register(() => new ContractCombinationViewModel()); 
            Locator.CurrentMutable.Register(() => new SignalDataViewModel()); 
            Locator.CurrentMutable.Register(() => new BaseSettingViewModel());

            Locator.CurrentMutable.Register(() => new YiTouMainViewModel());
            Locator.CurrentMutable.Register(() => new SampleChartViewModel());

            Locator.CurrentMutable.Register(() => new RealTimeMainViewModel());
            Locator.CurrentMutable.Register(() => new ProductListViewModel());

            Locator.CurrentMutable.RegisterConstant(new ContractConfigService(), typeof(IContractConfigService));
            Locator.CurrentMutable.RegisterConstant(new CombinationService(), typeof(ICombinationService));
            Locator.CurrentMutable.RegisterConstant(new HongAnalysisService(), typeof(IHongAnalysisService));
            // 两个独立研究页不能共享可变收益统计集合；每个 ViewModel 使用自己的计算服务。
            Locator.CurrentMutable.Register(() => new CycleIncomeService(), typeof(ICycleIncomeService));
            Locator.CurrentMutable.RegisterConstant(new OriginalService(), typeof(IOriginalService));
            Locator.CurrentMutable.RegisterConstant(new RealTimeDataService(), typeof(IRealTimeDataService));
        }
    }
}
