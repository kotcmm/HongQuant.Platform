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

    public OriginalMainViewModel()
    {
        _originalService = Locator.Current.GetService<IOriginalService>()!;

        DropMonitor = new FileDropMonitor();

        DropMonitor.Dropped.Subscribe(OpenFile);
    }

    private void OpenFile(IEnumerable<FileInfo> files)
    {
        _originalService.LoadFile(files);
    }
}
