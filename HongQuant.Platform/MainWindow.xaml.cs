using HongQuant.Platform.Terminal;
using HongQuant.Platform.Infrastucture;
using HongQuant.Platform.Views.Original;
using HongQuant.Platform.ViewModels.Original;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media;

namespace HongQuant.Platform;

public partial class MainWindow : Window
{
    private readonly TerminalDataBridge _data = new();
    private int _lastRequest;
    private int _stateRevision;
    private bool _updatingHeader;
    private bool _initialStateSent;
    private bool _browserErrorReported;

    // 组合 ID 与 ComboBox 的纯字符串选项按位置对应。
    private List<string> _headerCombinationIds = [];

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

            // 在加载用户页面前监听运行时异常，避免「界面显示但按钮全部失效」时静默失败。
            await core.AddScriptToExecuteOnDocumentCreatedAsync(@"
                window.addEventListener('error', function(event) {
                    window.chrome?.webview?.postMessage({
                        action:'client-error',
                        message:String(event.message || 'JavaScript error'),
                        location:String(event.filename || '') + ':' + String(event.lineno || 0)
                    });
                });
                window.addEventListener('unhandledrejection', function(event) {
                    window.chrome?.webview?.postMessage({
                        action:'client-error',
                        message:String(event.reason || 'Promise rejection'),
                        location:'unhandledrejection'
                    });
                });
            ");
            core.WebMessageReceived += OnWebMessage;
            core.ProcessFailed += (_, args) =>
                Debug.WriteLine("[HongQuant WebView2] Renderer process failure: " + args.ProcessFailedKind);
            core.NavigationCompleted += async (_, args) =>
            {
                if (!args.IsSuccess)
                {
                    MessageBox.Show($"UI 加载失败：{args.WebErrorStatus}", "HONG QUANT");
                    return;
                }

                // 有些环境 ready 消息先于 NavigationCompleted；只初始化一次真实数据。
                if (!_initialStateSent)
                    await SendState(0);

                // 检查桥接与交互 DOM 是否确实初始化，而非只检查脚本可解析。
                var health = await TerminalBrowser.ExecuteScriptAsync(
                    "Boolean(window.__hqNative && window.__hqNative.applyState && document.querySelector('.nav-item[data-view=\"data\"]') && document.querySelector('#assetList'))");
                if (health != "true" && !_browserErrorReported)
                {
                    _browserErrorReported = true;
                    MessageBox.Show(this, "研究终端页面未完成初始化，主内容区操作不可用。请检查调试输出中的脚本错误。",
                        "页面初始化失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                    if (!_initialStateSent)
                        await SendState(request);
                    return;
                case "input-probe":
                    Debug.WriteLine("[HongQuant WebView2] Pointer received in document: "
                        + ReadString(message, "target"));
                    return;
                case "client-error":
                    Debug.WriteLine("[HongQuant WebView2] "
                        + ReadString(message, "message") + " @ " + ReadString(message, "location"));
                    if (!_browserErrorReported)
                    {
                        _browserErrorReported = true;
                        MessageBox.Show(this,
                            "研究终端页面发生脚本错误：" + Environment.NewLine
                            + ReadString(message, "message") + Environment.NewLine
                            + ReadString(message, "location"),
                            "页面脚本错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                // 窗口控制已经移入原生 WPF 标题栏；不再从 WebView2 转发鼠标事件。
                case "view-changed":
                    HeaderModule.Text = ReadString(message, "name") switch
                    {
                        "compound" => "定投复利",
                        "settle" => "周期结算",
                        "chart" => "图表分析",
                        "ds" => "设计规范",
                        _ => "基础数据"
                    };
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
                    // JS 已即时更新勾选与计数，省去两次昂贵收益统计及表格重绘。
                    // 组合计算/其他状态变化仍继续发送完整状态。
                    return;
                case "sync-combination":
                case "create-combination":
                    _data.SynchronizeCombination();
                    break;
                case "toggle-template-sync":
                    _data.ToggleTemplateSync();
                    break;
                case "close-combination":
                    _data.CloseCombination(ReadString(message, "id") ?? string.Empty);
                    break;
                case "select-template":
                    _data.SelectTemplate(ReadString(message, "id") ?? string.Empty);
                    break;
                case "save-template":
                    _data.SaveSelectionAsTemplate(ReadString(message, "name") ?? string.Empty);
                    break;
                case "latest-view":
                    ShowLatestSelected();
                    return;
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
                    if (message.TryGetProperty("options", out var options) &&
                        options.ValueKind == JsonValueKind.Object &&
                        options.TryGetProperty("useIncome", out var income) &&
                        options.TryGetProperty("useTime", out var time) &&
                        income.ValueKind is JsonValueKind.True or JsonValueKind.False &&
                        time.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        _data.SetStoppingOptions(income.GetBoolean(), time.GetBoolean());
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
        // WPF 标题栏和 WebView2 都能发起选择；状态版本由唯一发送方递增。
        var state = _data.BuildState(++_stateRevision);
        _initialStateSent = true;
        UpdateNativeHeader(state);
        await TerminalBrowser.ExecuteScriptAsync("window.__hqNative && window.__hqNative.applyState(" + state + ");");
    }

    private async Task ReportError(string error)
    {
        if (TerminalBrowser.CoreWebView2 is not null)
            await TerminalBrowser.ExecuteScriptAsync(
                "window.__hqNative && window.__hqNative.showError(" + JsonSerializer.Serialize(error) + ");");
    }

    // 首次刷新和“数据路径”按钮都选择文件夹，并持久化本地目录。
    // 取消选择时必须保留原目录和已加载的数据。
    private bool ChooseFolder()
    {
        var dialog = new OpenFolderDialog { Title = "选择策略数据文件夹" };
        if (Directory.Exists(_data.FolderPath)) dialog.InitialDirectory = _data.FolderPath;
        if (dialog.ShowDialog(this) != true) return false;

        _data.FolderPath = dialog.FolderName;
        return true;
    }

    private async Task RefreshFiles()
    {
        // 未配置路径、路径被移动或删除时，先选择整个文件夹。
        // 不再弹出 CSV 单文件/多文件选择器。
        if (!Directory.Exists(_data.FolderPath) && !ChooseFolder()) return;

        var folder = _data.FolderPath;
        var exporter = Path.Combine(AppContext.BaseDirectory, "BatchFileExport", "BatchFileExport.exe");
        if (File.Exists(exporter))
        {
            var process = Process.Start(new ProcessStartInfo(exporter)
            {
                Arguments = "\"" + folder + "\"",
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

        // 延续原先的 Export 文件夹约定；若没有导出数据，也允许直接选择
        // 存放 CSV 的文件夹，而不是要求再次逐个选择 CSV 文件。
        var exportFolder = Path.Combine(folder, "Export");
        var files = Directory.Exists(exportFolder)
            ? Directory.GetFiles(exportFolder, "*.csv", SearchOption.TopDirectoryOnly)
            : [];
        if (files.Length == 0)
            files = Directory.GetFiles(folder, "*.csv", SearchOption.TopDirectoryOnly);
        if (files.Length == 0)
            throw new InvalidOperationException(
                $"在所选文件夹及其 Export 子文件夹中没有找到 CSV 数据文件：{folder}");

        _data.LoadFiles(files);
    }

    // 原来右键菜单的“最新查看”：继续使用原有窗口及 ShowLastViewModel，
    // 只把 WebView 品种勾选映射回旧视图模型，避免另造一套不一致的计算逻辑。
    private void ShowLatestSelected()
    {
        var selectedFiles = _data.GetSelectedFiles();
        if (selectedFiles.Count == 0)
            throw new InvalidOperationException("请先勾选需要查看最新数据的品种。");

        var selectedNames = selectedFiles.Select(file => file.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var viewModel = new ShowDataViewModel();
        foreach (var item in viewModel.FileIndexs)
            item.IsSelected = selectedNames.Contains(item.DataFile.Name) && item.Items?.Any() == true;

        var latest = new ShowLastView
        {
            Owner = this,
            DataContext = new ShowLastViewModel(viewModel)
        };
        latest.Closed += (_, _) => viewModel.Dispose();
        latest.Show();
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

    private void Minimize_Click(object sender, RoutedEventArgs e) =>
        SystemCommands.MinimizeWindow(this);

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            SystemCommands.RestoreWindow(this);
        else
            SystemCommands.MaximizeWindow(this);
    }

    private void Close_Click(object sender, RoutedEventArgs e) =>
        SystemCommands.CloseWindow(this);

    private async void HeaderCombinationChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingHeader) return;
        var index = HeaderCombinationPicker.SelectedIndex;
        if (index < 0 || index >= _headerCombinationIds.Count) return;

        _data.SelectSource(_headerCombinationIds[index]);
        try { await SendState(++_lastRequest); }
        catch (Exception ex) { await ReportError(ex.Message); }
    }

    private void UpdateNativeHeader(string stateJson)
    {
        using var json = JsonDocument.Parse(stateJson);
        var state = json.RootElement;
        _updatingHeader = true;
        try
        {
            var currentId = ReadString(state, "sourceId") ?? string.Empty;
            var ids = new List<string>();
            var names = new List<string>();
            if (state.TryGetProperty("sources", out var sources)
                && sources.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in sources.EnumerateArray())
                {
                    if (ReadString(item, "grp") != "组合") continue;
                    var id = ReadString(item, "id") ?? string.Empty;
                    if (id == "combo" || string.IsNullOrEmpty(id)) continue;
                    ids.Add(id);
                    names.Add(ReadString(item, "name") ?? "未命名组合");
                }
            }

            // 使用字符串列表，不使用复杂对象作为 ComboBox 内容。
            // 不添加“选择组合”等占位选项，也不自动选中第一个组合。
            _headerCombinationIds = ids;
            HeaderCombinationPicker.ItemsSource = names;
            HeaderCombinationPicker.SelectedIndex = ids.FindIndex(id => id == currentId);
            HeaderCombinationPicker.IsEnabled = ids.Count > 0;
            HeaderCombinationPicker.ToolTip = ids.Count == 0 ? "暂无可选组合" : "选择已计算的组合";

            HeaderStatus.Text = ReadString(state, "status") ?? "未导入数据";
            HeaderSourceName.Text = ReadString(state, "activeSourceName") ?? "—";
        }
        finally
        {
            _updatingHeader = false;
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var hwnd = new WindowInteropHelper(this).Handle;
        EnableWindowDecoration(hwnd);
    }

    private static void EnableWindowDecoration(IntPtr hwnd)
    {
        // Windows 11 优先采用 DWM 原生圆角和系统投影阴影；
        // Windows 10 仍保留 1px 的可见窗口描边。
        const uint cornerPreference = 33; // DWMWA_WINDOW_CORNER_PREFERENCE
        const uint borderColor = 34;      // DWMWA_BORDER_COLOR
        const uint ncRenderingPolicy = 2; // DWMWA_NCRENDERING_POLICY
        var corners = 2; // DWMWCP_ROUND
        var border = 0x00D8C7BB; // COLORREF (BGR)
        var enabled = 2; // DWMNCRP_ENABLED
        _ = DwmSetWindowAttribute(hwnd, cornerPreference, ref corners, sizeof(int));
        _ = DwmSetWindowAttribute(hwnd, borderColor, ref border, sizeof(int));
        _ = DwmSetWindowAttribute(hwnd, ncRenderingPolicy, ref enabled, sizeof(int));
        var margins = new DwmMargins { Left = 1, Right = 1, Top = 1, Bottom = 1 };
        _ = DwmExtendFrameIntoClientArea(hwnd, ref margins);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DwmMargins { public int Left, Right, Top, Bottom; }

    [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr hwnd, uint attribute, ref int value, int size);
    [DllImport("dwmapi.dll")] private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref DwmMargins margins);

}
