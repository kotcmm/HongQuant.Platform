namespace HongQuant.Platform.ViewModels.Hong.Setting.ContractConfigViewModels;
public class EditContractTypeViewModel(Guid id, string name)
{
    public Guid Id { get; set; } = id;

    public string Name { get; set; } = name;
}
