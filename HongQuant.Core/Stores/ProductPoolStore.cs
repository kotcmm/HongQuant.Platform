
using HongQuant.Core.Models;

namespace HongQuant.Core.Stores;
public class ProductPoolStore
{
    public IEnumerable<string> GetPools(string product)
    {
        using var db = new HongQuantDatabase();

        var query = db.ProductPools.FindAll().ToList();

        return query.Where(x => x.Products.Contains(product,StringComparer.CurrentCultureIgnoreCase))
                    .Select(x => x.Name)
                    .ToList();
    }

    public IEnumerable<ProductPool> GetProductPools()
    {
        using var db = new HongQuantDatabase();

        var query = db.ProductPools.FindAll().ToList();

        return query;
    }

    public void AddOrUpdatePool(ProductPool pool)
    {
        using var db = new HongQuantDatabase();

        var old = db.ProductPools.FindById(pool.Id);

        if (old != null)
        {
            db.ProductPools.Update(pool);
        }
        else
        {
            db.ProductPools.Insert(pool);
        }
    }

    public void Remove(int id)
    {
        using var db = new HongQuantDatabase();

        db.ProductPools.Delete(id);
    }
}
