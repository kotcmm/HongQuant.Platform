using HongQuant.Platform.Infrastucture;
using HongQuant.Platform.ViewModels.YiTou;
using Microsoft.Win32;
using ReactiveUI;
using System.Windows.Data;

namespace HongQuant.Platform.Views.YiTou;
/// <summary>
/// CycleIncomeBarChartView.xaml 的交互逻辑
/// </summary>
public partial class CycleIncomeBarChartView
{
    public CycleIncomeBarChartView()
    {
        InitializeComponent();

        Interactions.YiTouOpenFileDialog.RegisterHandler(DoSaveFileDialog);
    }

    private CycleIncomeBarChartViewModel? _viewModel;
    public CycleIncomeBarChartViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            _viewModel = value;
            DataContext = value;
        }
    }

    private void DoSaveFileDialog(IInteractionContext<string, string> interaction)
    {
        var dialog = new SaveFileDialog()
        {
            Filter = "(*.xlsx)|*.xlsx"
        };

        var result = dialog.ShowDialog();

        interaction.SetOutput(result == true? dialog.FileName : string.Empty);
    }
}

public class IntToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return (int)value == int.Parse(parameter.ToString()!);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        bool isChecked = (bool)value;

        if (!isChecked)
        {
            return null!;
        }
        return int.Parse(parameter.ToString()!);
    }
}
