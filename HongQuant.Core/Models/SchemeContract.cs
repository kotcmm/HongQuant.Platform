namespace HongQuant.Core.Models;
public class SchemeContract(Guid id, Contract contract)
{
    public SchemeContract() : this(Guid.Empty, new())
    {

    }

    public Guid Id { get; set; } = id;

    public int SortNumber { get; set; }

    public Contract Contract { get; set; } = contract;
}
