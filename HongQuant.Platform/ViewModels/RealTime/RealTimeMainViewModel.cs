using HongQuant.Core.Services;
using ReactiveUI;
using Splat;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.RealTime;
public class RealTimeMainViewModel : ReactiveObject
{
    private readonly IRealTimeDataService _realTimeDataService;

    public ICommand StartCommand { get; }
    public ICommand AnalysisCommand { get; }
    public ICommand ProductPoolCommand { get; }

    public static readonly Interaction<string, string> OpenRealTimeAnalysis = new();
    public static readonly Interaction<string, string> OpenProductPool = new();

    public RealTimeMainViewModel()
    {
        _realTimeDataService = Locator.Current.GetService<IRealTimeDataService>()!;

        StartCommand = ReactiveCommand.CreateFromTask(() =>
        {
           return _realTimeDataService.Start();
        });

        AnalysisCommand = ReactiveCommand.Create(() =>
        {
            OpenRealTimeAnalysis.Handle(string.Empty).Subscribe();
        });

        ProductPoolCommand = ReactiveCommand.Create(() =>
        {
            OpenProductPool.Handle(string.Empty).Subscribe();
        });
    }
}
