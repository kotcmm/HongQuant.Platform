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

    // 构建 ViewModel 时一次性生成只读展示文本。
    // 不再让每次鼠标悬停或 UI 重绘都枚举业务层的可变品种集合。
    private IList<SelectProduct> _selectProducts = [];
    private string _products = string.Empty;
    public string Products => _products;

    public IList<SelectProduct> SelectProducts
    {
        get => _selectProducts;
        set
        {
            _selectProducts = value ?? [];
            _products = string.Join(",", _selectProducts
                .Where(product => product is not null)
                .Select(product => product.Product));
            this.RaisePropertyChanged(nameof(Products));
        }
    }

    private void SetIsEditing()
    {
        IsEditing = _sourceTemplateName != _templateName;
    }

    [Reactive]
    public bool IsEditing { get; set; }
}