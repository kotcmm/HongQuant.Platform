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
