using Frontend.Models;
using System.Collections.ObjectModel;

namespace Frontend.ViewModels.Interfaces
{
    public interface ITabableViewModel
    {
        bool HasTabs { get; set; }
        ObservableCollection<TabItemModel> TabsShowing { get; }
        string ActiveTab { get; set; }
    }
}
