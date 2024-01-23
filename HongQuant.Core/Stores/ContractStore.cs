using HongQuant.Core.Models;

namespace HongQuant.Core.Stores;
public class ContractStore
{
    /// <summary>
    ///     加载全部品种类型
    /// </summary>
    /// <returns></returns>
    public static List<ContractType> LoadTypes()
    {
        using var db = new HongQuantDatabase();

        var query = db.ContractTypes
            .Include(x => x.Contracts)
            .FindAll();

        var list = query.ToList();

        foreach (var type in list)
        {
            type.Contracts.ForEach(x => { x.ContractType = type; });
        }

        return list;
    }

    /// <summary>
    ///     添加品种类型
    /// </summary>
    public static void AddType(ContractType type)
    {
        using var db = new HongQuantDatabase();

        db.ContractTypes.Insert(type);
    }

    /// <summary>
    ///     保存品种类型
    /// </summary>
    public static void UpdateType(ContractType type)
    {
        using var db = new HongQuantDatabase();

        db.ContractTypes.Update(type);
    }

    public static void RemoveType(Guid id)
    {
        using var db = new HongQuantDatabase();

        db.ContractTypes.Delete(id);

        db.Contracts.DeleteMany(c => c.ContractType.Id == id);
    }

    public static void AddContract(ContractType type, Contract contract)
    {
        using var db = new HongQuantDatabase();

        db.ContractTypes.Update(type);
        db.Contracts.Insert(contract);
    }

    public static void UpdateContract(Contract contract)
    {
        using var db = new HongQuantDatabase();

        db.Contracts.Update(contract);
    }

    public static void RemoveContract(ContractType type, Guid id)
    {
        using var db = new HongQuantDatabase();

        db.ContractTypes.Update(type);
        db.Contracts.Delete(id);
    }

    public static void ReplaceTypes(List<ContractType> types)
    {
        using var db = new HongQuantDatabase();

        db.ContractTypes.DeleteAll();
        db.Contracts.DeleteAll();

        db.ContractTypes.Insert(types);
        db.Contracts.Insert(types.SelectMany(t=>t.Contracts));
    }
}
