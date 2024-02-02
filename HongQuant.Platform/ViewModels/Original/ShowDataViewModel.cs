using DynamicData;
using HongQuant.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.Original;
public class ShowDataViewModel : ReactiveObject, IDisposable
{
    private readonly IDisposable _cleanUp;
    private readonly IOriginalService _originalService;

    private readonly ReadOnlyObservableCollection<BasicDataFileViewModel> _fileIndexs;
    public ReadOnlyObservableCollection<BasicDataFileViewModel> FileIndexs => _fileIndexs;

    private readonly ReadOnlyObservableCollection<CombinationBasicDataFileViewModel> _combinationFileIndexs;
    public ReadOnlyObservableCollection<CombinationBasicDataFileViewModel> CombinationFileIndexs => _combinationFileIndexs;

    [Reactive]
    public BasicDataFileViewModel? FileIndex { get; set; }

    [Reactive]
    public DataFileViewModel? SelectFileIndex { get; set; } 

    public ICommand CombinationCommand { get; }

    public ICommand TabChangeCommand { get; }

    public ICommand CloseCommand { get; }

    public ShowDataViewModel()
    {
        _originalService = Locator.Current.GetService<IOriginalService>()!;

        var fileIndexsDisposable = _originalService
              .FileIndexs
              .Connect()
              .Transform(f => new BasicDataFileViewModel(f))
              .ObserveOn(RxApp.MainThreadScheduler)
              .Bind(out _fileIndexs)
              .DisposeMany()
              .Subscribe();

        var combinationFileIndexsDisposable = _originalService
              .CombinationFileIndexs
              .Connect()
              .Transform(f => new CombinationBasicDataFileViewModel(f))
              .ObserveOn(RxApp.MainThreadScheduler)
              .Bind(out _combinationFileIndexs)
              .ForEachChange(x =>
              {
                  if (x.Reason != ChangeReason.Remove)
                  {
                      SelectFileIndex = x.Current;
                  }
              })
              .DisposeMany()
              .Subscribe();

        _cleanUp = new CompositeDisposable(fileIndexsDisposable, combinationFileIndexsDisposable);

        CombinationCommand = ReactiveCommand.Create(Combination);
        TabChangeCommand = ReactiveCommand.Create<DataFileViewModel>(TabChange);
        CloseCommand = ReactiveCommand.Create<DataFileViewModel>(Close);

        this.WhenAnyValue(x => x.FileIndex).Subscribe(x =>
        {
            SelectFileIndex = x;
        });
    }

    private void Close(DataFileViewModel model)
    {
        _originalService.Remove(model.DataFile.Id);

        if(SelectFileIndex == model)
        {
            SelectFileIndex = FileIndex;
        }
    }

    private void TabChange(DataFileViewModel model)
    {
        SelectFileIndex = model;
    }

    private void Combination()
    {
        var selecteds = FileIndexs
            .Where(x => x.IsSelected)
            .Select(x=>x.DataFile)
            .ToList();

        _originalService.Combination(selecteds);
    }

    public void Dispose()
    {
        _cleanUp.Dispose();
    }
}
