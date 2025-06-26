using DynamicData;
using HongQuant.Core.Models;
using HongQuant.Core.Services;
using HongQuant.Platform.Infrastucture;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.Original;
public class ShowDataViewModel : ReactiveObject, IDisposable
{
    private readonly IDisposable _cleanUp;
    private readonly IOriginalService _originalService;

    private readonly ReadOnlyObservableCollection<BasicDataFileViewModel> _fileIndexs;
    public ReadOnlyObservableCollection<BasicDataFileViewModel> FileIndexs => _fileIndexs;

    private readonly ReadOnlyObservableCollection<CombinationBasicDataFileViewModel> _combinationFileIndexs;
    public ReadOnlyObservableCollection<CombinationBasicDataFileViewModel> CombinationFileIndexs => _combinationFileIndexs;

    private readonly ReadOnlyObservableCollection<ProductTemplatesViewModel> _productTemplates;
    public ReadOnlyObservableCollection<ProductTemplatesViewModel> ProductTemplates => _productTemplates;

    public static readonly Interaction<ShowDataViewModel, string> OpenShowChart = new();
    public static readonly Interaction<ShowDataViewModel, string> OpenShowLast = new();

    [Reactive]
    public BasicDataFileViewModel? FileIndex { get; set; }

    [Reactive]
    public DataFileViewModel? SelectFileIndex { get; set; }

    [Reactive]
    public bool IsTemplateDialogOpen { get; set; }

    [Reactive]
    public string TemplateName { get; set; } = string.Empty;

    [Reactive]
    public string Product { get; set; } = string.Empty;

    [Reactive]
    public string Strategy { get; set; } = string.Empty;

    [Reactive]
    public ProductTemplatesViewModel? SelectProductTemplate { get; set; }

    public ICommand CombinationCommand { get; }

    public ICommand TabChangeCommand { get; }

    public ICommand CloseCommand { get; }

    public ICommand OpenSaveTemplateCommand { get; }

    public ICommand SaveTemplateCommand { get; }

    public ICommand ShowChartCommand { get; }

    public ICommand ShowLastCommand { get; }

    public ICommand ExportCommand { get; }

    public ShowDataViewModel()
    {
        _originalService = Locator.Current.GetService<IOriginalService>()!;

        this.WhenAnyValue(x => x.FileIndex).Subscribe(x =>
        {
            SelectFileIndex = x;
        });

        var filterTemplate = this.WhenAnyValue(x => x.SelectProductTemplate).Select(x =>
        {
            if (x == null)
            {
                return (Func<BasicDataFileViewModel, bool>)(f => {
                    f.IsSelected = false;
                    return true;
                });
            }

            return (Func<BasicDataFileViewModel, bool>)(f =>
            {
                var flag = x.Products.Any(m => m.Product == f.DataFile.Product && m.Strategy == f.DataFile.Strategy);
                f.IsSelected = flag;
                return flag;
            });
        });

        var filterStrategy = this.WhenAnyValue(x => x.Strategy).Select(x =>
        {
            if (string.IsNullOrEmpty(x)) 
                return f => true;

            return (Func<BasicDataFileViewModel, bool>)(f => x.Split(",").Contains(f.DataFile.Strategy, StringComparer.OrdinalIgnoreCase));
        });

        var filterProduct = this.WhenAnyValue(x => x.Product).Select(x =>
        {
            if (string.IsNullOrEmpty(x))
                return f => true;

            return (Func<BasicDataFileViewModel, bool>)(f => x.Split(",").Contains(f.DataFile.Product, StringComparer.OrdinalIgnoreCase));
        });

        var fileIndexsDisposable = _originalService
              .FileIndexs
              .Connect()
              .Transform(f => new BasicDataFileViewModel(f))
              .Filter(filterTemplate)
              .Filter(filterStrategy)
              .Filter(filterProduct)
              .ObserveOn(RxApp.MainThreadScheduler)
              .Bind(out _fileIndexs)
              .DisposeMany()
              .Subscribe();

        var combinationFileIndexsDisposable = _originalService
              .CombinationFileIndexs
              .Connect()
              .Transform(f => new CombinationBasicDataFileViewModel(f))
              .ObserveOn(RxApp.MainThreadScheduler)
              .Bind(out _combinationFileIndexs)
              .ForEachChange(x =>
              {
                  if (x.Reason != ChangeReason.Remove)
                  {
                      SelectFileIndex = x.Current;
                  }
              })
              .DisposeMany()
              .Subscribe();

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

        _cleanUp = new CompositeDisposable(fileIndexsDisposable,
            combinationFileIndexsDisposable,
            productTemplatessDisposable);

        CombinationCommand = ReactiveCommand.Create(Combination);
        TabChangeCommand = ReactiveCommand.Create<DataFileViewModel>(TabChange);
        CloseCommand = ReactiveCommand.Create<DataFileViewModel>(Close);
        OpenSaveTemplateCommand = ReactiveCommand.Create(OpenSaveTemplate);
        ShowChartCommand = ReactiveCommand.CreateFromTask(ShowChartAsync);
        ShowLastCommand = ReactiveCommand.CreateFromTask(ShowLastAsync);
        ExportCommand = ReactiveCommand.CreateFromTask(Export);
        SaveTemplateCommand = ReactiveCommand.CreateFromTask(SaveTemplateAsync);
    }

    private async Task ShowChartAsync()
    {
        await OpenShowChart.Handle(this);
    }

    private async Task ShowLastAsync()
    {
        await OpenShowLast.Handle(this);
    }

    private async Task Export()
    {
        var dir = await Interactions.OpenFolderDialog.Handle("");

        if (string.IsNullOrEmpty(dir) || SelectFileIndex == null) return;

        _originalService.Save(dir, SelectFileIndex.DataFile);
    }

    private async Task SaveTemplateAsync()
    {
        var selecteds = FileIndexs.Where(x => x.IsSelected);

        var products = FileIndexs.Where(x => x.IsSelected)
            .Select(x => new SelectProduct(x.DataFile.Product, x.DataFile.Strategy)).ToList();

        IsTemplateDialogOpen = false;

        if (products.Count > 0)
        {
            var productTemplate = new ProductTemplate
            {
                Id = Guid.NewGuid(),
                TemplateName = TemplateName,
                Products = products
            };

            _originalService.SaveProductTemplate(productTemplate);
        }
        else
        {
            await Interactions.GlobalError.Handle("没有选择品种");
        }
    }

    private void OpenSaveTemplate()
    {
        IsTemplateDialogOpen = true;
    }

    private void Close(DataFileViewModel model)
    {
        _originalService.Remove(model.DataFile.Id);

        if(SelectFileIndex == model)
        {
            SelectFileIndex = FileIndex;
        }
    }

    private void TabChange(DataFileViewModel model)
    {
        SelectFileIndex = model;
    }

    private void Combination()
    {
        var selecteds = FileIndexs
            .Where(x => x.IsSelected)
            .Select(x=>x.DataFile)
            .ToList();

        _originalService.Combination(selecteds);
    }

    public void Dispose()
    {
        _cleanUp.Dispose();
    }
}
