namespace HongQuant.Platform.ViewModels.Hong.Setting.ContractConfigViewModels
{
    public class ContractTypeViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public IEnumerable<ContractViewModel> Contracts { get; set; } = [];
    }
}
