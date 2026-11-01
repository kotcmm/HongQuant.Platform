using HongQuant.Platform.Infrastucture;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using ReactiveUI;
using System.Windows;
using System.Windows.Input;

namespace HongQuant.Platform;

public partial class MainWindow
{
    private bool _isDark;

    public MainWindow()
    {
        InitializeComponent();
        Interactions.OpenFolderDialog.RegisterHandler(DoOpenFolder);
        Interactions.GlobalError.RegisterHandler(GlobalError);
    }

    private void DragTitlebar(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;
        if (e.ClickCount == 2)
        {
            ToggleWindowState();
            return;
        }
        if (WindowState == WindowState.Maximized)
        {
            // 最大化时拖动标题栏也能恢复窗口
            return;
        }
        DragMove();
    }

    private void ToggleTheme_Click(object sender, RoutedEventArgs e)
    {
        _isDark = !_isDark;
        var name = _isDark ? "TerminalDark.xaml" : "TerminalLight.xaml";
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var current = dictionaries.FirstOrDefault(x =>
            x.Source?.OriginalString.Contains("TerminalLight.xaml", StringComparison.OrdinalIgnoreCase) == true ||
            x.Source?.OriginalString.Contains("TerminalDark.xaml", StringComparison.OrdinalIgnoreCase) == true);
        if (current is null) return;
        dictionaries[dictionaries.IndexOf(current)] = new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/HongQuant.Platform;component/Resources/{name}")
        };
        ThemeButton.Content = _isDark ? "☀" : "☾";
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Maximize_Click(object sender, RoutedEventArgs e) => ToggleWindowState();
    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void ToggleWindowState() =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void DoOpenFolder(IInteractionContext<string, string> context)
    {
        var dialog = new OpenFolderDialog();
        context.SetOutput(dialog.ShowDialog() == true ? dialog.FolderName : string.Empty);
    }

    private async Task GlobalError(IInteractionContext<string, bool> context)
    {
        await this.ShowMessageAsync("错误提示", context.Input);
        context.SetOutput(false);
    }
}