namespace HongQuant.Core.Models;
public class CombinationScheme(Guid id, string name)
{

    public CombinationScheme() : this(Guid.Empty, string.Empty)
    {

    }

    public Guid Id { get; set; } = id;

    public string Name { get; set; } = name;

    public bool IsDefault { get; set; }

    public List<SchemeContract> Contracts { get; set; } = [];
}
