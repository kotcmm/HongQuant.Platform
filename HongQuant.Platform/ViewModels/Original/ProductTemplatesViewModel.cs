using HongQuant.Core.Models;
using LiteDB;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HongQuant.Platform.ViewModels.Original;
public class ProductTemplatesViewModel
{
    public string TemplateName { get; set; } = string.Empty;

    public IList<SelectProduct> Products { get; set; } = [];
}

public class ProductTemplatesEditViewModel : ReactiveObject
{
    public Guid Id { get; set; }

    private string _templateName = string.Empty;
    private string _sourceTemplateName = string.Empty;
    public string TemplateName
    {
        get => _templateName;
        set
        {
            _templateName = value;

            if (string.IsNullOrEmpty(_sourceTemplateName))
            {
                _sourceTemplateName = value;
            }

            SetIsEditing();
        }
    }

    public string Products => string.Join(",", SelectProducts.Select(x => x.Product));

    public IList<SelectProduct> SelectProducts { get; set; } = [];

    private void SetIsEditing()
    {
        IsEditing = _sourceTemplateName != _templateName;
    }

    [Reactive]
    public bool IsEditing { get; set; }
}