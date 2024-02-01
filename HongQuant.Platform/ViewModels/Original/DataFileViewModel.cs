using HongQuant.Analysis.Original;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HongQuant.Platform.ViewModels.Original;
public class DataFileViewModel : ReactiveObject
{
    public DataFileViewModel(IDataFile dataFile)
    {
        DataFile = dataFile;

        DataFile.CalculateChange.Subscribe(items =>
        {
            Items = items.Select(item => new BasicTransformDataViewModel(item));
        });
    }

    public IDataFile DataFile { get; }

    public string Name => DataFile.Name;

    [Reactive]
    public IEnumerable<BasicTransformDataViewModel>? Items { get; set; }
}
