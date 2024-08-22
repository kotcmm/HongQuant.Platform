namespace HongQuant.Platform.ViewModels.RealTime.Items;
public class ProductPoolDataViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Products { get; set; } = string.Empty;

    public IList<string> GetProductArray()
    {
        return Products.Split(',');
    }
}
