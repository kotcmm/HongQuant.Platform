namespace HongQuant.Core.Models;
public class ProductPool
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public IList<string> Products { get; set; } = [];
}
