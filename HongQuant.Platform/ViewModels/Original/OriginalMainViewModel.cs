using HongQuant.Core.Services;
using HongQuant.Platform.Infrastucture.Drops.Files;
using ReactiveUI;
using Splat;
using System.IO;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.Original;
public class OriginalMainViewModel : ReactiveObject
{
    private readonly IOriginalService _originalService;

    public FileDropMonitor DropMonitor { get; }

    public ICommand StartRunCommand { get; }

    public OriginalMainViewModel()
    {
        _originalService = Locator.Current.GetService<IOriginalService>()!;

        DropMonitor = new FileDropMonitor();

        DropMonitor.Dropped.Subscribe(OpenFile);

        StartRunCommand = ReactiveCommand.Create(StartRun);
    }

    private void OpenFile(IEnumerable<FileInfo> files)
    {
        _originalService.LoadFile(files);
    }

    public void StartRun()
    {
        foreach (var item in _originalService.FileIndexs.Items)
        {
            item.Calculate();
        }
    }
}
