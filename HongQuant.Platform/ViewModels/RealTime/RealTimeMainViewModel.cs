using HongQuant.Core.Services;
using ReactiveUI;
using Splat;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.RealTime;
public class RealTimeMainViewModel : ReactiveObject
{
    private readonly IRealTimeDataService _realTimeDataService;

    public ICommand StartCommand { get; }

    public RealTimeMainViewModel()
    {
        _realTimeDataService = Locator.Current.GetService<IRealTimeDataService>()!;

        StartCommand = ReactiveCommand.CreateFromTask(() =>
        {
           return _realTimeDataService.Start();
        });
    }
}
