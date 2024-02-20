using HongQuant.Core.Models;
using LiteDB;

namespace HongQuant.Core.Stores;
public class ProductTemplateStore
{
    /// <summary>
    ///     加载全部模板
    /// </summary>
    /// <returns></returns>
    public static List<ProductTemplate> LoadTemplates()
    {
        using var db = new HongQuantDatabase();

        var query = db.ProductTemplates.FindAll();

        var list = query.ToList();

        return list;
    }

    /// <summary>
    ///     添加或者更新模板
    /// </summary>
    public static void AddOrUpdate(ProductTemplate template)
    {
        using var db = new HongQuantDatabase();

        var old = db.ProductTemplates.FindById(template.Id);

        if (old != null)
        {
            db.ProductTemplates.Update(template);
        }
        else
        {
            db.ProductTemplates.Insert(template);
        }
    }

    /// <summary>
    ///     删除模板
    /// </summary>
    public static void Remove(Guid id)
    {
        using var db = new HongQuantDatabase();

        db.ProductTemplates.Delete(id);
    }
}
