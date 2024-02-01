using HongQuant.Analysis.Original;
using ReactiveUI.Fody.Helpers;

namespace HongQuant.Platform.ViewModels.Original;
public class BasicDataFileViewModel(IDataFile dataFile) : DataFileViewModel(dataFile)
{
    [Reactive]
    public bool IsSelected {  get; set; }
}
