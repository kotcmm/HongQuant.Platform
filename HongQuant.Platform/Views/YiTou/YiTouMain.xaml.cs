using HongQuant.Platform.ViewModels.YiTou;
using Microsoft.Win32;
using ReactiveUI;
using Splat;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.YiTou;

/// <summary>周期结算 / 定投复利复用同一个业务视图，初始模式由工作台导航指定。</summary>
public partial class YiTouMain : UserControl
{
    private string? _initialAlgorithmName;
    public string? InitialAlgorithmName
    {
        get => _initialAlgorithmName;
        set
        {
            _initialAlgorithmName = value;
            if (DataContext is YiTouMainViewModel vm && value is not null)
            {
                var item = vm.AlgorithmTypes.FirstOrDefault(x => x.Name == value);
                if (item is not null) vm.SelectAlgorithmType = item;
            }
        }
    }

    public YiTouMain()
    {
        InitializeComponent();
        var viewModel = Locator.Current.GetService<YiTouMainViewModel>();
        DataContext = viewModel;
        if (viewModel is not null)
        {
            viewModel.ShowFileDialog.RegisterHandler(DoShowFileDialog);
            viewModel.SaveFileDialog.RegisterHandler(DoSaveFileDialog);
            viewModel.OpenChartView.RegisterHandler(DoOpenChartView);
        }
    }

    private CycleIncomeBarChartView? _cycleIncomeBarChart;

    private void DoOpenChartView(IInteractionContext<YiTouMainViewModel, bool> interaction)
    {
        if (_cycleIncomeBarChart is null)
        {
            _cycleIncomeBarChart = new CycleIncomeBarChartView
            {
                ViewModel = new CycleIncomeBarChartViewModel(interaction.Input)
            };
            _cycleIncomeBarChart.Closed += (sender, e) => _cycleIncomeBarChart = null;
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
        var dialog = new OpenFileDialog { Filter = "(*.xlsx)|*.xlsx" };
        interaction.SetOutput(dialog.ShowDialog() == true ? dialog.FileName : string.Empty);
    }

    private void DoSaveFileDialog(IInteractionContext<string, string> interaction)
    {
        var dialog = new SaveFileDialog { Filter = "(*.xlsx)|*.xlsx" };
        interaction.SetOutput(dialog.ShowDialog() == true ? dialog.FileName : string.Empty);
    }
}