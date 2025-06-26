using HongQuant.Core.Services;
using HongQuant.Core.Stores;
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

    [Reactive]
    public bool IsTemplateSync { get; set; }

    public ICommand OpenFolderCommand { get; }

    public ICommand RefreshFilesCommand { get; }

    public ICommand ExportCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenProductTemplateSettingCommand { get; }

    public OriginalMainViewModel()
    {
        _originalService = Locator.Current.GetService<IOriginalService>()!;

        DropMonitor = new FileDropMonitor();

        DropMonitor.Dropped.Subscribe(OpenFile);

        OpenFolderCommand = ReactiveCommand.CreateFromTask(OpenFolder);

        RefreshFilesCommand = ReactiveCommand.CreateFromTask(RefreshFiles);

        ExportCommand = ReactiveCommand.CreateFromTask(Export);

        OpenProductTemplateSettingCommand = ReactiveCommand.Create<Unit>(integer => { });

        FolderPathChange();

        TemplateSyncChange();
    }

    private async Task Export()
    {
        var dir = await Interactions.OpenFolderDialog.Handle("");

        if (string.IsNullOrEmpty(dir)) return;

        foreach (var item in _originalService.FileIndexs.Items)
        {
            SaveFile(dir, item);
        }

        foreach (var item in _originalService.CombinationFileIndexs.Items)
        {
            SaveFile(dir, item);
        }
    }

    private void SaveFile(string dir, Analysis.Original.IDataFile item)
    {
        _originalService.Save(dir, item);
    }

    private void TemplateSyncChange()
    {
        var str = GlobalSettingStroe.GetValue("TemplateSync");

        if (!string.IsNullOrEmpty(str))
        {
            IsTemplateSync = bool.Parse(str);
        }

        this.WhenAnyValue(x => x.IsTemplateSync)
                    .Subscribe(x =>
                    {
                        GlobalSettingStroe.SetValue(new Core.Models.GlobalSetting
                        {
                            Key = "TemplateSync",
                            Value = x.ToString(),
                        });
                    });
    }

    private void FolderPathChange()
    {
        FolderPath = GlobalSettingStroe.GetValue("FolderPath") ?? string.Empty;

        this.WhenAnyValue(x => x.FolderPath)
            .Subscribe(x =>
            {
                GlobalSettingStroe.SetValue(new Core.Models.GlobalSetting
                {
                    Key = "FolderPath",
                    Value = x,
                });
            });
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

        TemplateSync();
    }

    private void TemplateSync()
    {
        if (IsTemplateSync)
        {
            var templates = _originalService.ProductTemplates.Items;

            foreach (var item in templates)
            {
                var fileIndexs = _originalService.FileIndexs.Items
                    .Where(x => item.Products.Any(y => y.Product == x.Product && y.Strategy == x.Strategy))
                    .ToList();

                _originalService.Combination(fileIndexs, item.TemplateName);
            }
        }
    }
}
