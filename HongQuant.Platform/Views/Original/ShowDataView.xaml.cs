using HongQuant.Platform.ViewModels.Original;
using ReactiveUI;
using Splat;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace HongQuant.Platform.Views.Original;
/// <summary>
/// ShowDataView.xaml 的交互逻辑
/// </summary>
public partial class ShowDataView : UserControl
{
    public ShowDataView()
    {
        InitializeComponent();

        DataContext = Locator.Current.GetService<ShowDataViewModel>();

        ShowDataViewModel.OpenShowChart.RegisterHandler(ShowChart);
        ShowDataViewModel.OpenShowLast.RegisterHandler(ShowLast);
    }

    private ShowChartView? _showChartView;

    private void ShowChart(IInteractionContext<ShowDataViewModel, string> context)
    {
        if(_showChartView == null)
        {
            _showChartView = new ShowChartView() 
            { 
                DataContext = new ShowChartViewModel(context.Input),
                Owner = System.Windows.Application.Current.MainWindow,
            };

            _showChartView.Closed += ShowChartView_Closed;

            _showChartView.Show();
        }
        else
        {
            _showChartView.Focus();
        }

        context.SetOutput(string.Empty);
    }

    private void ShowChartView_Closed(object? sender, EventArgs e)
    {
        _showChartView = null;
    }

    private ShowLastView? _showLastView;

    private void ShowLast(IInteractionContext<ShowDataViewModel, string> context)
    {
        if (_showLastView == null)
        {
            _showLastView = new ShowLastView()
            {
                DataContext = new ShowLastViewModel(context.Input),
                Owner = System.Windows.Application.Current.MainWindow,
            };

            _showLastView.Closed += ShowLastView_Closed;

            _showLastView.Show();
        }
        else
        {
            _showLastView.Focus();
        }

        context.SetOutput(string.Empty);
    }

    private void ShowLastView_Closed(object? sender, EventArgs e)
    {
        _showLastView = null;
    }
}


public class TypeToBoolConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2) return false;

        return values[0] == values[1];
    }


    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return [];
    }
}

