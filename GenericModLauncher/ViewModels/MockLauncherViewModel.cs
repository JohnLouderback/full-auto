using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using static GenericModLauncher.Utils;

namespace GenericModLauncher.ViewModels;

public partial class MockLauncherViewModel : ILauncherViewModel, INotifyPropertyChanged {
  public string GameTitle => "Commando!";

  public ImageSource? GameLogoPath => null;

  /// <inheritdoc />
  public ImageSource? BackgroundImagePath => LoadImage(@"H:\Downloads\JDUI_UD_P.png");

  /// <inheritdoc />
  public ItemViewModel SelectedItem {
    get => Items[0];
    set {}
  }

  /// <inheritdoc />
  public ObservableCollection<ItemViewModel> Items { get; } = [
    new() {
      Group       = "Base Game", Name = "Commando",
      Description = "A classic action game where you fight against an army of enemies.",
      ReleaseYear = "1985",
      OnLaunch    = () => {}
    },
    new() { Group = "Mods", Name = "Super Commando", OnLaunch = () => {} },
    new() { Group = "Mods", Name = "Stealth Mode", OnLaunch   = () => {} },
    new() { Group = "Mods", Name = "Unlimited Ammo", OnLaunch = () => {} }
  ];


  /// <inheritdoc />
  public void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {}
}
