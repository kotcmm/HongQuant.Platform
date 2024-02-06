using HongQuant.Core.Services;
using HongQuant.Platform.Infrastucture;
using HongQuant.Platform.Infrastucture.Drops.Files;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Diagnostics;
using System.IO;
using System.Reactive;
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

    public ReactiveCommand<Unit, Unit> OpenProductTemplateSettingCommand { get; }

    public OriginalMainViewModel()
    {
        _originalService = Locator.Current.GetService<IOriginalService>()!;

        DropMonitor = new FileDropMonitor();

        DropMonitor.Dropped.Subscribe(OpenFile);

        OpenFolderCommand = ReactiveCommand.CreateFromTask(OpenFolder);

        RefreshFilesCommand = ReactiveCommand.CreateFromTask(RefreshFiles);

        OpenProductTemplateSettingCommand = ReactiveCommand.Create<Unit>(integer => { });
    }

    private async Task RefreshFiles()
    {
        var exportFolderPath = Path.Combine(FolderPath, "Export");

        if (Directory.Exists(exportFolderPath))
        {
            Directory.Delete(exportFolderPath, true);
        }

        Process p = Process.Start("BatchFileExport\\BatchFileExport.exe", FolderPath);
        await p.WaitForExitAsync();
        if (p != null)
        {
            p.Close();
            p.Dispose();
        }

        if (Directory.Exists(exportFolderPath))
        {
            var fileInfos = Directory.GetFiles(exportFolderPath);

            OpenFile(fileInfos.Select(f => new FileInfo(f)));
        }
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
