using System.Collections.ObjectModel;
using System.Windows.Media;

namespace GenericModLauncher.ViewModels;

public interface ILauncherViewModel {
  string       GameTitle    { get; }
  ImageSource? GameLogoPath { get; }

  ImageSource? BackgroundImagePath { get; }

  ItemViewModel SelectedItem { get; set; }

  ObservableCollection<ItemViewModel> Items { get; }
}
