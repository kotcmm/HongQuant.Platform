using DynamicData;
using HongQuant.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.Original;
public class ShowDataViewModel : ReactiveObject, IDisposable
{
    private readonly IDisposable _cleanUp;

    private readonly ReadOnlyObservableCollection<BasicDataFileViewModel> _fileIndexs;
    public ReadOnlyObservableCollection<BasicDataFileViewModel> FileIndexs => _fileIndexs;

    [Reactive]
    public BasicDataFileViewModel? FileIndex { get; set; }

    public ShowDataViewModel()
    {
        var originalService = Locator.Current.GetService<IOriginalService>()!;

        _cleanUp = originalService.FileIndexs.Connect()
          .Transform(f => new BasicDataFileViewModel(f))
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
