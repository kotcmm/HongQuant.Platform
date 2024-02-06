namespace HongQuant.Core.Models;
public class ProductTemplate
{
    public string TemplateName { get; set; } = string.Empty;

    public IList<string> Products { get; set; } = [];
}
