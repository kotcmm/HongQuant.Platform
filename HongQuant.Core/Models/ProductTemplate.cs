namespace HongQuant.Core.Models;
public class ProductTemplate
{
    public Guid Id { get; set; }

    public string TemplateName { get; set; } = string.Empty;

    public IList<SelectProduct> Products { get; set; } = [];
}

public record SelectProduct(string Product, string Strategy);
