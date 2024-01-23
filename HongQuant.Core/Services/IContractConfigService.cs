using DynamicData;
using HongQuant.Core.Models;

namespace HongQuant.Core.Services;
public interface IContractConfigService
{
    IObservableCache<ContractType, Guid> ContractTypes { get; }

    void AddContract(Guid typeId, string key, ContractName name);
    void AddType(string name);
    void EditContract(Guid typeId, Guid id, string key, ContractName name);
    void RemoveContract(Guid typeId, Guid id);
    void RemoveType(Guid id);
    void ReplaceTypes(List<ContractType> types);
    void UpdateType(Guid id, string name);
}
