using LiteDB;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HongQuant.Platform.ViewModels.Original;
public class ProductTemplatesViewModel
{
    public string TemplateName { get; set; } = string.Empty;

    public IList<string> Products { get; set; } = [];
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

    private string _sourceProducts = string.Empty;
    private string _products = string.Empty;
    public string Products 
    { 
        get => _products;
        set 
        {
            _products = value;

            if (string.IsNullOrEmpty(_sourceProducts))
            {
                _sourceProducts = value;
            }

            SetIsEditing();
        }
    }

    private void SetIsEditing()
    {
        IsEditing = _products != _sourceProducts ||
            _sourceTemplateName != _templateName;
    }

    [Reactive]
    public bool IsEditing { get; set; }
}