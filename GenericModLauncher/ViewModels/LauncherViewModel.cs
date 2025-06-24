using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using GenericModLauncher.Models;
using static GenericModLauncher.Utils;

namespace GenericModLauncher.ViewModels;

public partial class LauncherViewModel : ILauncherViewModel, INotifyPropertyChanged {
  private readonly ILauncherConfiguration               config;
  private          ImageSource?                         gameLogoImage;
  private          ImageSource?                         backgroundImage;
  private          ObservableCollection<ItemViewModel>? items;
  private          ItemViewModel?                       selectedItem;

  public string GameTitle => config.Game.DisplayName;

  public ImageSource? GameLogoPath => gameLogoImage ??= !string.IsNullOrEmpty(config.Game.LogoPath)
                                                          ? LoadImage(config.Game.LogoPath)
                                                          : null;

  /// <inheritdoc />
  public ImageSource? BackgroundImagePath =>
    backgroundImage ??= !string.IsNullOrEmpty(config.BackgroundImagePath)
                          ? LoadImage(config.BackgroundImagePath)
                          : null;

  /// <inheritdoc />
  public ItemViewModel SelectedItem {
    get => selectedItem ??= Items.FirstOrDefault() ??
                            new ItemViewModel {
                              OnLaunch = () => {}
                            };
    set {
      if (value is null) {
        throw new ArgumentNullException(nameof(value), "Selected item cannot be null.");
      }

      selectedItem = value;
      OnPropertyChanged();
    }
  }

  /// <inheritdoc />
  public ObservableCollection<ItemViewModel> Items =>
    items ??= new ObservableCollection<ItemViewModel>(
      (config.Game is not null
         ? new[] {
           new ItemViewModel {
             Group       = "Base Game",
             Name        = config.Game.DisplayName,
             Description = config.Game.Description ?? string.Empty,
             ReleaseYear = config.Game.ReleaseYear ?? string.Empty,
             OnLaunch = () => {
               if (config.Game.OnLaunch is not null) {
                 config.Game.OnLaunch(config.Game, mod: null, mixins: null);
               }
             }
           }
         }
         : []).Concat(
        config.Game?.Mods is not null
          ? config.Game.Mods.Select(mod => new ItemViewModel {
              Group       = "Mods",
              Name        = mod.DisplayName,
              Description = mod.Description ?? string.Empty,
              ReleaseYear = mod.ReleaseYear ?? string.Empty,
              OnLaunch = () => {
                if (mod.OnLaunch is not null) {
                  mod.OnLaunch(config.Game, mod, mixins: null);
                }
              }
            }
          )
          : []
      )
    );


  public LauncherViewModel(ILauncherConfiguration config) {
    this.config = config;
  }
}
