using ReactiveUI;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.Hong.Setting.ContractConfigViewModels;
public class CreateOrEditContractViewModel : ReactiveObject
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string DefaultName { get; set; } = string.Empty;

    public string InputOtherName { get; set; } = string.Empty;

    public Guid TypeId { get; set; }

    public IList<string> OtherNames { get; private set; }

    public ICommand AddOtherNameCommand { get; }

    public ICommand DeleteOtherNameCommand { get; }

    public CreateOrEditContractViewModel() : this([])
    {
        OtherNames = new ObservableCollection<string>();
    }

    public CreateOrEditContractViewModel(IList<string> otherNames)
    {
        OtherNames = new ObservableCollection<string>(otherNames);

        AddOtherNameCommand = ReactiveCommand.Create(() =>
        {
            if (OtherNames.Contains(InputOtherName))
            {
                return;
            }
            OtherNames.Add(InputOtherName);
        });

        DeleteOtherNameCommand = ReactiveCommand.Create<string>((s) =>
        {
            OtherNames.Remove(s);
        });
    }
}
