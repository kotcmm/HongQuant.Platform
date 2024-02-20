using DynamicData;
using HongQuant.Core.Models;
using HongQuant.Core.Services;
using ReactiveUI;
using Splat;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.Original;
public class ProductTemplateSettingViewModel : ReactiveObject, IDisposable
{
    private readonly IDisposable _cleanUp;
    private readonly IOriginalService _originalService;

    private readonly ReadOnlyObservableCollection<ProductTemplatesEditViewModel> _productTemplates;
    public ReadOnlyObservableCollection<ProductTemplatesEditViewModel> ProductTemplates => _productTemplates;

    public static readonly Interaction<string, bool> Confirm = new();

    public ProductTemplateSettingViewModel() 
    {
        _originalService = Locator.Current.GetService<IOriginalService>()!;

        var productTemplatessDisposable = _originalService
             .ProductTemplates
             .Connect()
             .Transform(f => new ProductTemplatesEditViewModel
             {
                 Id = f.Id,
                 Products = string.Join(",",f.Products),
                 TemplateName = f.TemplateName
             })
             .ObserveOn(RxApp.MainThreadScheduler)
             .Bind(out _productTemplates)
             .DisposeMany()
             .Subscribe();

        SaveCommand = ReactiveCommand.Create<ProductTemplatesEditViewModel>(Save);
        DeleteCommand = ReactiveCommand.CreateFromTask<ProductTemplatesEditViewModel>(Delete);

        _cleanUp = new CompositeDisposable(productTemplatessDisposable);
    }

    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    private void Save(ProductTemplatesEditViewModel model)
    {
        var productTemplate = new ProductTemplate
        {
            Id = model.Id,
            TemplateName = model.TemplateName,
            Products = model.Products.Split(',')
        };

        _originalService.SaveProductTemplate(productTemplate);
    }   
    
    private async Task Delete(ProductTemplatesEditViewModel model)
    {
        var delete = await Confirm.Handle($"是否删除{model.TemplateName}");

        if (delete)
        {
            _originalService.RemoveProductTemplate(model.Id);
        }
    }

    public void Dispose()
    {
        _cleanUp.Dispose();
    }
}
