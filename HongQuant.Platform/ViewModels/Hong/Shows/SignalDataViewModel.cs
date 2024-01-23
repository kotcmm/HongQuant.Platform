using DynamicData;
using HongQuant.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.Hong.Shows;
public class SignalDataViewModel : ReactiveObject, IDisposable
{
    private readonly IDisposable _cleanUp;
    private readonly IHongAnalysisService _hongAnalysisService;

    private readonly ReadOnlyObservableCollection<SignalFileIndexViewModel> _fileIndexs;
    public ReadOnlyObservableCollection<SignalFileIndexViewModel> FileIndexs => _fileIndexs;

    [Reactive]
    public SignalFileIndexViewModel? FileIndex {  get; set; }

    public SignalDataViewModel()
    {
        _hongAnalysisService = Locator.Current.GetService<IHongAnalysisService>()!;

        _cleanUp = _hongAnalysisService.FileIndexs.Connect()
              .Transform(f => new SignalFileIndexViewModel(f))
              .ObserveOn(RxApp.MainThreadScheduler)
              .Bind(out _fileIndexs)
              .DisposeMany()
              .Subscribe();
    }

    public void Dispose()
    {
        _cleanUp.Dispose();
    }
}
