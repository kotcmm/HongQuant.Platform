namespace HongQuant.Platform.ViewModels.Hong.Setting.ContractConfigViewModels
{
    public class ContractViewModel
    {
        public Guid Id { get; set; }

        public string Key { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public IList<string> OtherNames { get; set; } = [];

        public Guid TypeId { get; set; }
    }
}
