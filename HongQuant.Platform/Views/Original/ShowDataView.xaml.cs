using HongQuant.Platform.ViewModels.Original;
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

