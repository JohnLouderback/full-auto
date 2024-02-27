using CommunityToolkit.WinUI;
using DependencyPropertyGenerator;
using DownscalerV3.Utils;
using DownscalerV3.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DownscalerV3.Views;

[DependencyProperty<FrameworkElement>(
  "Target",
  Description = "The element that the flyout should be positioned relative to."
)]
[DependencyProperty<bool>("IsOpen", Description = "Whether or not the flyout is open.")]
public sealed partial class DebugSettingsFlyout : UserControl {
  public MainViewModel ViewModel { get; }


  public DebugSettingsFlyout() {
    ViewModel = App.GetService<MainViewModel>();
    InitializeComponent();
  }


  partial void OnIsOpenChanged(bool oldValue, bool newValue) {
    var control = this;
    if (control.DebugSubMenu == null) return;
    
    control.DebugSubMenu.IsOpen = newValue;

    // If the flyout is being opened, then focus the first checkbox.
    if (newValue) {
      control.DispatcherQueue.SetTimeout(
        () => {
          // Programmatically find first checkbox and focus it.
          var firstCheckBox = control.DebugSubMenu.FindChild<CheckBox>();
          firstCheckBox?.Focus(FocusState.Programmatic);
        },
        50
      );
    }
  }


  partial void OnTargetChanged(FrameworkElement oldValue, FrameworkElement newValue) {
    var control = this;
    if (control.DebugSubMenu != null) {
      control.DebugSubMenu.Target = newValue;
    }
  }
}
