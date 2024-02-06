using HongQuant.Platform.Infrastucture;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using ReactiveUI;

namespace HongQuant.Platform;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        Interactions.OpenFolderDialog.RegisterHandler(DoOpenFolder);

        Interactions.GlobalError.RegisterHandler(GlobalError);
    }

    private void DoOpenFolder(IInteractionContext<string, string> context)
    {
        var dialog = new OpenFolderDialog();

        var result = dialog.ShowDialog();

        context.SetOutput(result == true ? dialog.FolderName : string.Empty);
    }

    private async Task GlobalError(IInteractionContext<string, bool> context)
    {
        await this.ShowMessageAsync("错误提示", context.Input);

        context.SetOutput(false);
    }
}