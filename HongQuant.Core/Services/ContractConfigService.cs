using DynamicData;
using HongQuant.Core.Models;
using HongQuant.Core.Stores;

namespace HongQuant.Core.Services;
public class ContractConfigService : IContractConfigService
{
    private readonly SourceCache<ContractType, Guid> _typeCache = new(x => x.Id);
    public IObservableCache<ContractType, Guid> ContractTypes => _typeCache.AsObservableCache();

    public ContractConfigService()
    {
        LoadTypes();
    }

    public void AddType(string name)
    {
        var type = new ContractType(Guid.NewGuid(), name);

        _typeCache.AddOrUpdate(type);

        ContractStore.AddType(type);
    }

    public void RemoveType(Guid id)
    {
        _typeCache.RemoveKey(id);

        ContractStore.RemoveType(id);
    }

    public void UpdateType(Guid id, string name)
    {
        var type = _typeCache.Lookup(id).Value;

        type.Name = name;

        _typeCache.AddOrUpdate(type);

        ContractStore.UpdateType(type);
    }

    public void AddContract(Guid typeId, string key, ContractName name)
    {
        if (_typeCache.Items.SelectMany(x => x.Contracts).Any(x => x.Key == key))
        {
            throw new Exception("重复key");
        }

        var type = _typeCache.Lookup(typeId).Value;

        var contract = new Contract(Guid.NewGuid(), key, name, type);

        type.Contracts.Add(contract);

        _typeCache.AddOrUpdate(type);

        ContractStore.AddContract(type,contract);
    }

    public void EditContract(Guid typeId, Guid id, string key, ContractName name)
    {
        if (_typeCache.Items.SelectMany(x => x.Contracts).Any(x => x.Key == key && x.Id != id))
        {
            throw new Exception("重复key");
        }

        var type = _typeCache.Lookup(typeId).Value;

        var contract = type.Contracts.Find(x => x.Id == id);

        if (contract is not null)
        {
            contract.Key = key;
            contract.Name = name;

            _typeCache.AddOrUpdate(type);

            ContractStore.UpdateContract(contract);
        }
    }

    public void RemoveContract(Guid typeId, Guid id)
    {
        var type = _typeCache.Lookup(typeId).Value;

        var index = type.Contracts.FindIndex(x => x.Id == id);

        if (index > -1)
        {
            type.Contracts.RemoveAt(index);

            _typeCache.AddOrUpdate(type);

            ContractStore.RemoveContract(type, id);
        }
    }

    public void ReplaceTypes(List<ContractType> types)
    {
        _typeCache.Clear();

        _typeCache.AddOrUpdate(types);

        ContractStore.ReplaceTypes(types);
    }

    private void LoadTypes()
    {
        var types = ContractStore.LoadTypes();

        _typeCache.AddOrUpdate(types);
    }
}
