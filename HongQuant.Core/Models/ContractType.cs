namespace HongQuant.Core.Models;
public class ContractType(Guid id, string name)
{
    public ContractType() : this(Guid.Empty, string.Empty) { }

    public Guid Id { get; set; } = id;

    public string Name { get; set; } = name;

    public int SortNumber { get; set; }

    public List<Contract> Contracts { get; set; } = [];
}
