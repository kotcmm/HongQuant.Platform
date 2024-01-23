namespace HongQuant.Core.Models;
public class ContractName(string name) : IEquatable<ContractName>
{
    public HashSet<string> OtherNames { get; set; } = [];

    public string Name { get; set; } = name;

    public ContractName() : this(string.Empty)
    {
    }

    public ContractName(string name, IEnumerable<string> otherNames) : this(name)
    {
        Replace(otherNames);
    }

    public void AddRange(IEnumerable<string> otherNames)
    {
        foreach (var item in otherNames)
        {
            OtherNames.Add(item);
        }
    }

    public void Replace(IEnumerable<string> otherNames)
    {
        OtherNames = new HashSet<string>(otherNames);
    }

    public override string ToString()
    {
        return string.Join(",", new List<string>(OtherNames) { Name });
    }

    public bool Contains(string name)
    {
        return name == Name || OtherNames.Contains(name);
    }

    public bool Equals(ContractName? other)
    {
        if (other is null) return false;

        return other.Contains(Name) ||
            Contains(other.Name);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ContractName)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = Name.GetHashCode();
        foreach (var name in OtherNames)
        {
            hashCode = (hashCode * 397) ^ name.GetHashCode();
        }
        return hashCode;
    }

    public static bool operator ==(ContractName left, ContractName right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ContractName left, ContractName right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string(ContractName C) => C.Name;
}
