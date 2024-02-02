using HongQuant.Platform.Infrastucture;
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
    }

    private void DoOpenFolder(IInteractionContext<string, string> context)
    {
        var dialog = new OpenFolderDialog();

        var result = dialog.ShowDialog();

        context.SetOutput(result == true ? dialog.FolderName : string.Empty);
    }
}