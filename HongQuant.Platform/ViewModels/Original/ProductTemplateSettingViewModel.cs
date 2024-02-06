using DynamicData;
using HongQuant.Core.Services;
using ReactiveUI;
using Splat;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.Original;
public class ProductTemplateSettingViewModel : ReactiveObject, IDisposable
{
    private readonly IDisposable _cleanUp;
    private readonly IOriginalService _originalService;

    private readonly ReadOnlyObservableCollection<ProductTemplatesViewModel> _productTemplates;
    public ReadOnlyObservableCollection<ProductTemplatesViewModel> ProductTemplates => _productTemplates;

    public ProductTemplateSettingViewModel() 
    {
        _originalService = Locator.Current.GetService<IOriginalService>()!;

        var productTemplatessDisposable = _originalService
             .ProductTemplates
             .Connect()
             .Transform(f => new ProductTemplatesViewModel
             {
                 Products = f.Products,
                 TemplateName = f.TemplateName
             })
             .ObserveOn(RxApp.MainThreadScheduler)
             .Bind(out _productTemplates)
             .DisposeMany()
             .Subscribe();


        _cleanUp = new CompositeDisposable(productTemplatessDisposable);
    }

    public void Dispose()
    {
        _cleanUp.Dispose();
    }
}
