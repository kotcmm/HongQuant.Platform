using HongQuant.Platform.ViewModels.RealTime;
using ReactiveUI;
using Splat;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.RealTime;
/// <summary>
/// RealTimeMain.xaml 的交互逻辑
/// </summary>
public partial class RealTimeMain : UserControl
{
    public RealTimeMain()
    {
        InitializeComponent();

        DataContext = Locator.Current.GetService<RealTimeMainViewModel>();

        RealTimeMainViewModel.OpenRealTimeAnalysis.RegisterHandler(OnOpenRealTimeAnalysis);
        RealTimeMainViewModel.OpenProductPool.RegisterHandler(OnOpenProductPool);
    }

    private ProductPoolView? _productPoolView;

    private void OnOpenProductPool(IInteractionContext<string, string> context)
    {
        if (_productPoolView == null)
        {
            _productPoolView = new ProductPoolView()
            {
                DataContext = new ProductPoolViewModel(),
                Owner = System.Windows.Application.Current.MainWindow,
            };

            _productPoolView.Closed += ProductPoolView_Closed;

            _productPoolView.Show();
        }
        else
        {
            _productPoolView.Focus();
        }

        context.SetOutput(string.Empty);
    }

    private void ProductPoolView_Closed(object? sender, EventArgs e)
    {
        _productPoolView = null;
    }

    private RealTimeAnalysisView? _realTimeAnalysisView;

    private void OnOpenRealTimeAnalysis(IInteractionContext<string, string> context)
    {
        if (_realTimeAnalysisView == null)
        {
            _realTimeAnalysisView = new RealTimeAnalysisView()
            {
                DataContext = new RealTimeAnalysisViewModel(),
                Owner = System.Windows.Application.Current.MainWindow,
            };

            _realTimeAnalysisView.Closed += RealTimeAnalysisView_Closed;

            _realTimeAnalysisView.Show();
        }
        else
        {
            _realTimeAnalysisView.Focus();
        }

        context.SetOutput(string.Empty);
    }

    private void RealTimeAnalysisView_Closed(object? sender, EventArgs e)
    {
        _realTimeAnalysisView = null;
    }
}
