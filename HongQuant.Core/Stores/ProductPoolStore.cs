
namespace HongQuant.Core.Stores;
public class ProductPoolStore
{
    public IEnumerable<string> GetPools(string product)
    {
        using var db = new HongQuantDatabase();

        var query = db.ProductPools.FindAll().ToList();

        return query.Where(x => x.Products.Contains(product))
                    .Select(x => x.Name)
                    .ToList();
    }
}
