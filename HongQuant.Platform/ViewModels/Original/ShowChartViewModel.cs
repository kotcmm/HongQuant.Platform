using HongQuant.Platform.ViewModels.Original.ShowCharts;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.Original;
public class ShowChartViewModel : ReactiveObject
{
    private List<BasicTransformDataViewModel>? _statistics;

    public int ShowField { get; set; } = 0;

    public ICommand FieldChangeCommand { get; }

    [Reactive]
    public IChartData? ChartData { get; set; }

    public ShowChartViewModel(ShowDataViewModel showDataViewModel)
    {
        showDataViewModel.WhenAnyValue(x => x.SelectFileIndex).Subscribe(DataFileChange);

        FieldChangeCommand = ReactiveCommand.Create<string>(FieldChange);
    }

    private void FieldChange(string showField)
    {
        ShowField = int.Parse(showField);
        UpdateChartData();
    }

    private void DataFileChange(DataFileViewModel? dataFile)
    {
        _statistics = dataFile?.Items?.ToList();
        UpdateChartData();
    }

    private void UpdateChartData()
    {
        ChartData = ShowField switch
        {
            0 => new YieldsChartData(_statistics),
            1 => new TotalYieldsChartData(_statistics),
            2 => new WithdrawDeepChartData(_statistics),
            _ => throw new NotImplementedException(),
        };
    }
}
