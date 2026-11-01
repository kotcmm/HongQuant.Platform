using HongQuant.Platform.ViewModels.Original;
using Splat;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.Original;

/// <summary>
/// 组合模板管理窗口：沿用原有的 ViewModel、保存/删除命令和确认交互，
/// 仅将 MahApps 蓝色窗口与确认框统一为终端浅色 WPF 样式。
/// </summary>
public partial class ProductTemplateSettingView : Window
{
    private readonly IDisposable _confirmationHandler;
    private TaskCompletionSource<bool>? _pendingConfirmation;

    public ProductTemplateSettingView()
    {
        InitializeComponent();
        DataContext = Locator.Current.GetService<ProductTemplateSettingViewModel>()
            ?? throw new InvalidOperationException("未注册组合模板管理 ViewModel。");

        _confirmationHandler = ProductTemplateSettingViewModel.Confirm.RegisterHandler(
            async interaction =>
            {
                bool confirmed = await ShowDeleteConfirmationAsync(interaction.Input);
                interaction.SetOutput(confirmed);
            });
        Closed += OnClosed;
    }

    private Task<bool> ShowDeleteConfirmationAsync(string templateName)
    {
        _pendingConfirmation?.TrySetResult(false);
        _pendingConfirmation = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        DeleteConfirmationMessage.Text = $"确定要删除「{templateName}」吗？删除后无法恢复。";
        DeleteConfirmationOverlay.Visibility = Visibility.Visible;
        CancelDeleteButton.Focus();
        return _pendingConfirmation.Task;
    }

    private void CompleteConfirmation(bool confirmed)
    {
        DeleteConfirmationOverlay.Visibility = Visibility.Collapsed;
        var pending = _pendingConfirmation;
        _pendingConfirmation = null;
        pending?.TrySetResult(confirmed);
    }

    private void CancelDelete_Click(object sender, RoutedEventArgs e) =>
        CompleteConfirmation(false);

    private void ConfirmDelete_Click(object sender, RoutedEventArgs e) =>
        CompleteConfirmation(true);

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

    private void OnClosed(object? sender, EventArgs e)
    {
        _pendingConfirmation?.TrySetResult(false);
        _pendingConfirmation = null;
        _confirmationHandler.Dispose();
        (DataContext as IDisposable)?.Dispose();
    }
}
