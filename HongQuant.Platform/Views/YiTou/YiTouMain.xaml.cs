using HongQuant.Platform.ViewModels.YiTou;
using Microsoft.Win32;
using ReactiveUI;
using Splat;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.YiTou;
/// <summary>
/// YiTouMain.xaml 的交互逻辑
/// </summary>
public partial class YiTouMain : UserControl
{
    public YiTouMain()
    {
        InitializeComponent();
        var viewModel = Locator.Current.GetService<YiTouMainViewModel>();

        DataContext = viewModel;

        if(viewModel != null)
        {
            viewModel.ShowFileDialog.RegisterHandler(DoShowFileDialog);
            viewModel.SaveFileDialog.RegisterHandler(DoSaveFileDialog);
            viewModel.OpenChartView.RegisterHandler(DoOpenChartView);
        }
    }

    private CycleIncomeBarChartView? _cycleIncomeBarChart;

    private void DoOpenChartView(IInteractionContext<YiTouMainViewModel, bool> interaction)
    {
        if (_cycleIncomeBarChart == null)
        {
            _cycleIncomeBarChart = new CycleIncomeBarChartView()
            {
                ViewModel = new CycleIncomeBarChartViewModel(interaction.Input)
            };
            _cycleIncomeBarChart.Closed += (sender, e) =>
            {
                _cycleIncomeBarChart = null;
            };
            _cycleIncomeBarChart.Show();
        }
        else
        {
            _cycleIncomeBarChart.Focus();
        }

        interaction.SetOutput(true);
    }

    private void DoShowFileDialog(IInteractionContext<string, string> interaction)
    {
        var dialog = new OpenFileDialog()
        {
            Filter = "(*.xlsx)|*.xlsx"
        };

        var result = dialog.ShowDialog();

        interaction.SetOutput(result == true ? dialog.FileName : string.Empty);
    }

    private void DoSaveFileDialog(IInteractionContext<string, string> interaction)
    {
        var dialog = new SaveFileDialog()
        {
            Filter = "(*.xlsx)|*.xlsx"
        };

        var result = dialog.ShowDialog();

        interaction.SetOutput(result == true ? dialog.FileName : string.Empty);
    }
}
