using CommunityToolkit.Mvvm.ComponentModel;

namespace Frontend.Models;
public class TabItemModel : ObservableObject
{
    private string _name = string.Empty;
    private bool _isVisible;

    public TabItemModel(string name, bool visible = false)
    {
        Name = name;
        IsVisible = visible;
    }
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }
}
