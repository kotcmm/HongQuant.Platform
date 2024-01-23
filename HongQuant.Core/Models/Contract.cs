namespace HongQuant.Core.Models;
public class Contract(Guid id, string key, ContractName name, ContractType contractType) : IEquatable<Contract>
{
    public Contract() : this(Guid.Empty, string.Empty, new(), new()) { }

    public Guid Id { get; set; } = id;

    public string Key { get; set; } = key;

    public int SortNumber { get; set; }

    public ContractName Name { get; set; } = name;

    public ContractType ContractType { get; set; } = contractType;

    public bool Contains(string name)
    {
        return Key.Equals(name, StringComparison.CurrentCultureIgnoreCase) ||
            Name.Contains(name);
    }

    public bool Equals(Contract? other)
    {
        if (other is null) return false;

        return Key.Equals(other.Key, StringComparison.CurrentCultureIgnoreCase) ||
            Name.Contains(other.Key) ||
            other.Name.Contains(Key) ||
            Name.Equals(other.Name);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Contract)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = Key.ToUpper().GetHashCode();
        return (hashCode * 397) ^ Name.GetHashCode();
    }

    public static bool operator ==(Contract left, Contract right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Contract left, Contract right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        return $"{ContractType}-{Key}:{Name}";
    }
}
