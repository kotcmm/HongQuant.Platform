using HongQuant.Platform.Terminal;
using HongQuant.Platform.Infrastucture;
using HongQuant.Platform.Views.Original;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace HongQuant.Platform;

public partial class MainWindow : Window
{
    private readonly TerminalDataBridge _data = new();
    private int _lastRequest;

    public MainWindow()
    {
        InitializeComponent();
        Interactions.OpenFolderDialog.RegisterHandler(context =>
        {
            var picker = new OpenFolderDialog();
            context.SetOutput(picker.ShowDialog(this) == true ? picker.FolderName : string.Empty);
        });
        Interactions.GlobalError.RegisterHandler(context =>
        {
            MessageBox.Show(this, context.Input, "操作失败", MessageBoxButton.OK, MessageBoxImage.Error);
            context.SetOutput(false);
        });
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        try
        {
            await TerminalBrowser.EnsureCoreWebView2Async();
            var core = TerminalBrowser.CoreWebView2;
            core.Settings.AreDevToolsEnabled = Debugger.IsAttached;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.IsZoomControlEnabled = false;
            TerminalBrowser.ZoomFactor = 1.0;
            core.Settings.IsStatusBarEnabled = false;
            core.WebMessageReceived += OnWebMessage;
            core.NavigationCompleted += async (_, args) =>
            {
                if (args.IsSuccess) await SendState(0);
                else MessageBox.Show($"UI 加载失败：{args.WebErrorStatus}", "HONG QUANT");
            };
            var uiFolder = Path.Combine(AppContext.BaseDirectory, "Resources", "TerminalUi");
            if (!Directory.Exists(uiFolder))
                throw new DirectoryNotFoundException($"缺少终端页面资源：{uiFolder}");
            core.SetVirtualHostNameToFolderMapping("hongquant.local", uiFolder,
                CoreWebView2HostResourceAccessKind.DenyCors);
            core.Navigate("https://hongquant.local/hongquant-terminal.html");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"无法启动研究终端。请确认已安装 Microsoft Edge WebView2 Runtime。\n{ex.Message}", "HONG QUANT");
        }
    }

    private static string? ReadString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() : null;

    private async void OnWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            using var json = JsonDocument.Parse(e.WebMessageAsJson);
            var message = json.RootElement;
            var action = ReadString(message, "action") ?? string.Empty;
            var request = message.TryGetProperty("requestId", out var r) && r.TryGetInt32(out var id)
                ? id : ++_lastRequest;
            _lastRequest = Math.Max(request, _lastRequest);

            switch (action)
            {
                case "ready":
                    await SendState(request);
                    return;
                case "window-minimize":
                    WindowState = WindowState.Minimized;
                    return;
                case "window-toggle":
                    ToggleMaximize();
                    return;
                case "window-close":
                    Close();
                    return;
                case "window-drag":
                    BeginSystemDrag();
                    return;
                case "choose-path":
                    ChooseFolder();
                    break;
                case "refresh":
                    await RefreshFiles();
                    break;
                case "select-source":
                    _data.SelectSource(ReadString(message, "id") ?? string.Empty);
                    break;
                case "select-assets":
                    if (message.TryGetProperty("indexes", out var indexes) &&
                        indexes.ValueKind == JsonValueKind.Array)
                        _data.SelectAssets(indexes.EnumerateArray().Where(x => x.TryGetInt32(out _)).Select(x => x.GetInt32()));
                    break;
                case "sync-combination":
                    _data.SynchronizeCombination();
                    break;
                case "edit-templates":
                    new ProductTemplateSettingView { Owner = this }.ShowDialog();
                    break;
                case "export-data":
                    ExportData();
                    return;
                case "export-stats":
                    ExportStats(ReadString(message, "mode") ?? "compound");
                    return;
                case "export-chart":
                    await ExportChart();
                    return;
                case "parameters":
                    if (message.TryGetProperty("values", out var values) && values.ValueKind == JsonValueKind.Object)
                        foreach (var property in values.EnumerateObject())
                            if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetDouble(out var number))
                                _data.SetParameter(property.Name, number);
                    break;
                default:
                    return;
            }
            await SendState(request);
        }
        catch (Exception ex)
        {
            await ReportError(ex.Message);
        }
    }

    private async Task SendState(int request)
    {
        if (TerminalBrowser.CoreWebView2 is null) return;
        var state = _data.BuildState(request);
        await TerminalBrowser.ExecuteScriptAsync("window.__hqNative && window.__hqNative.applyState(" + state + ");");
    }

    private async Task ReportError(string error)
    {
        if (TerminalBrowser.CoreWebView2 is not null)
            await TerminalBrowser.ExecuteScriptAsync(
                "window.__hqNative && window.__hqNative.showError(" + JsonSerializer.Serialize(error) + ");");
    }

    private void ChooseFolder()
    {
        var dialog = new OpenFolderDialog { Title = "选择策略数据目录" };
        if (Directory.Exists(_data.FolderPath)) dialog.InitialDirectory = _data.FolderPath;
        if (dialog.ShowDialog(this) == true) _data.FolderPath = dialog.FolderName;
    }

    private async Task RefreshFiles()
    {
        var folder = _data.FolderPath;
        var exporter = Path.Combine(AppContext.BaseDirectory, "BatchFileExport", "BatchFileExport.exe");
        if (Directory.Exists(folder) && File.Exists(exporter))
        {
            var process = Process.Start(new ProcessStartInfo(exporter)
            {
                Arguments = "\""+ folder + "\"",
                WorkingDirectory = Path.GetDirectoryName(exporter)!,
                UseShellExecute = false
            }) ?? throw new InvalidOperationException("批量导出工具启动失败。");
            using (process)
            {
                await process.WaitForExitAsync();
                if (process.ExitCode != 0)
                    throw new InvalidOperationException($"批量导出工具执行失败，退出码 {process.ExitCode}。");
            }
        }

        var exportFolder = Path.Combine(folder, "Export");
        var files = Directory.Exists(exportFolder)
            ? Directory.GetFiles(exportFolder, "*.csv") : [];
        if (files.Length == 0)
        {
            var dialog = new OpenFileDialog
            {
                Title = "导入真实策略数据文件",
                Filter = "CSV 数据文件 (*.csv)|*.csv",
                Multiselect = true
            };
            if (dialog.ShowDialog(this) != true) return;
            files = dialog.FileNames;
        }
        _data.LoadFiles(files);
    }

    private void ExportData()
    {
        var dialog = new OpenFolderDialog { Title = "选择权益数据导出目录" };
        if (dialog.ShowDialog(this) == true) _data.ExportSelected(dialog.FolderName);
    }

    private void ExportStats(string mode)
    {
        var dialog = new SaveFileDialog { Title = "导出统计数据", Filter = "Excel 工作簿 (*.xlsx)|*.xlsx" };
        if (dialog.ShowDialog(this) == true) _data.ExportStatistics(dialog.FileName, mode);
    }

    private async Task ExportChart()
    {
        var dialog = new SaveFileDialog { Title = "保存图表截图", Filter = "PNG 图片 (*.png)|*.png" };
        if (dialog.ShowDialog(this) != true) return;
        await using var stream = File.Create(dialog.FileName);
        await TerminalBrowser.CoreWebView2.CapturePreviewAsync(
            CoreWebView2CapturePreviewImageFormat.Png, stream);
    }

    private void ToggleMaximize() =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    // WebView2 内部的 pointerdown 不会自动冒泡成 WPF MouseDown；
    // 因此通过 WM_NCLBUTTONDOWN/HTCAPTION 交给 Windows 处理拖动与窗口贴靠。
    private void BeginSystemDrag()
    {
        if (!GetCursorPos(out var cursor)) return;
        if (WindowState == WindowState.Maximized)
        {
            var dpi = VisualTreeHelper.GetDpi(this);
            var ratio = Math.Clamp((cursor.X / dpi.DpiScaleX - Left) / Math.Max(1, ActualWidth), 0.12, 0.88);
            WindowState = WindowState.Normal;
            Left = cursor.X / dpi.DpiScaleX - ActualWidth * ratio;
            Top = cursor.Y / dpi.DpiScaleY - 22;
        }
        ReleaseCapture();
        SendMessage(new WindowInteropHelper(this).Handle, 0xA1, new IntPtr(2), IntPtr.Zero);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CursorPoint { public int X; public int Y; }
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out CursorPoint point);
    [DllImport("user32.dll")] private static extern bool ReleaseCapture();
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam);
}
