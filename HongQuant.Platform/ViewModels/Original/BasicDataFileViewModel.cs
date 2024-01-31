using HongQuant.Analysis.Original;
using ReactiveUI.Fody.Helpers;

namespace HongQuant.Platform.ViewModels.Original;
public class BasicDataFileViewModel
{
    public BasicDataFileViewModel(BasicDataFile basicDataFile)
    {
        BasicDataFile = basicDataFile;

        BasicDataFile.CalculateChange.Subscribe(items =>
        {
            Items = items.Select(item => new BasicTransformDataViewModel()
            {
                SignalDateTime = item.SignalDateTime,
                TradeDate = item.TradeDate,
                Equity = item.Equity,
                TodayProfitAndLoss = item.TodayProfitAndLoss,
            });
        });
    }

    public BasicDataFile BasicDataFile { get; }

    public string Path => BasicDataFile.FileInfo.Name;

    [Reactive]
    public IEnumerable<BasicTransformDataViewModel>? Items { get; set; }
}
