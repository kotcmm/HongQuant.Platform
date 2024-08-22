using HongQuant.Core.Stores;
using HongQuant.Platform.ViewModels.RealTime.Items;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.RealTime;
public class ProductPoolViewModel : ReactiveObject
{
    public ICommand AddOrUpdateCommand { get; set; }

    public ICommand DeleteCommand { get; set; }

    [Reactive]
    public ProductPoolDataViewModel Pool { get; set; } = new ProductPoolDataViewModel();

    [Reactive]
    public IEnumerable<ProductPoolDataViewModel> ProductPools { get; set; } = [];

    public ProductPoolViewModel()
    {
        AddOrUpdateCommand = ReactiveCommand.Create(AddOrUpdate);
        DeleteCommand = ReactiveCommand.Create<ProductPoolDataViewModel>(Delete);

        Load();
    }

    private void Delete(ProductPoolDataViewModel model)
    {
        var store = new ProductPoolStore();

        store.Remove(model.Id);

        Load();
    }

    private void Load()
    {
        var store = new ProductPoolStore();

        ProductPools = store.GetProductPools().Select(x=>new ProductPoolDataViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Products = string.Join(",", x.Products)
        });
    }

    private void AddOrUpdate()
    {
        if (string.IsNullOrEmpty(Pool.Name) ||
            string.IsNullOrEmpty(Pool.Products)) return;

        var store = new ProductPoolStore();

        store.AddOrUpdatePool(new Core.Models.ProductPool
        {
            Id = Pool.Id,
            Name= Pool.Name,
            Products = Pool.GetProductArray()
        });

        Load();
    }
}
