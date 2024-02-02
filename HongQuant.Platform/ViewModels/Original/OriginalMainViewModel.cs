using HongQuant.Core.Services;
using HongQuant.Platform.Infrastucture;
using HongQuant.Platform.Infrastucture.Drops.Files;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.Original;
public class OriginalMainViewModel : ReactiveObject
{
    private readonly IOriginalService _originalService;

    public FileDropMonitor DropMonitor { get; }

    [Reactive]
    public string FolderPath { get; set; } = string.Empty;

    public ICommand OpenFolderCommand { get; }

    public ICommand RefreshFilesCommand { get; }

    public OriginalMainViewModel()
    {
        _originalService = Locator.Current.GetService<IOriginalService>()!;

        DropMonitor = new FileDropMonitor();

        DropMonitor.Dropped.Subscribe(OpenFile);

        OpenFolderCommand = ReactiveCommand.CreateFromTask(OpenFolder);

        RefreshFilesCommand = ReactiveCommand.Create(RefreshFiles);
    }

    private void RefreshFiles()
    {

    }

    private async Task OpenFolder()
    {
        FolderPath = await Interactions.OpenFolderDialog.Handle("");
    }

    private void OpenFile(IEnumerable<FileInfo> files)
    {
        _originalService.LoadFile(files);
    }
}
