using HongQuant.Core.Services;
using ReactiveUI;
using Splat;

namespace HongQuant.Platform.ViewModels.RealTime;
public class RealTimeMainViewModel : ReactiveObject
{
    private readonly IRealTimeDataService _realTimeDataService;

    public RealTimeMainViewModel()
    {
        _realTimeDataService = Locator.Current.GetService<IRealTimeDataService>()!;
    }
}
