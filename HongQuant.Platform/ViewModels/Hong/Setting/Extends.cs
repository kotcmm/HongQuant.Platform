using HongQuant.Core.Models;
using HongQuant.Platform.ViewModels.Hong.Setting.ContractConfigViewModels;

namespace HongQuant.Platform.ViewModels.Hong.Setting;
internal static class Extends
{
    public static ContractTypeViewModel Bulid(this ContractType type)
    {
        return new ContractTypeViewModel()
        {
            Id = type.Id,
            Name = type.Name,
            Contracts = type.Contracts.Select(x => new ContractViewModel()
            {
                TypeId = type.Id,
                Id = x.Id,
                Key = x.Key,
                Name = x.Name,
                OtherNames = x.Name.OtherNames.ToList()
            })
        };
    }
}
