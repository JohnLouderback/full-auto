using DownscalerV3.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DownscalerV3.Views;

public sealed partial class DebugSettingsFlyout : UserControl {
  public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
    "Target",
    typeof(FrameworkElement),
    typeof(DebugSettingsFlyout),
    new PropertyMetadata(null, OnTargetChanged)
  );

  public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
    "IsOpen",
    typeof(bool),
    typeof(DebugSettingsFlyout),
    new PropertyMetadata(null, OnIsOpenChanged)
  );

  public MainViewModel ViewModel { get; }

  public FrameworkElement Target {
    get => (FrameworkElement)GetValue(TargetProperty);
    set => SetValue(TargetProperty, value);
  }

  public bool IsOpen {
    get => (bool)GetValue(IsOpenProperty);
    set => SetValue(IsOpenProperty, value);
  }


  public DebugSettingsFlyout() {
    ViewModel = App.GetService<MainViewModel>();
    InitializeComponent();
  }


  private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var control = d as DebugSettingsFlyout;
    if (control != null &&
        control.DebugSubMenu != null) {
      control.DebugSubMenu.IsOpen = (bool)e.NewValue;
    }
  }


  private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var control = d as DebugSettingsFlyout;
    if (control != null &&
        control.DebugSubMenu != null) {
      control.DebugSubMenu.Target = (FrameworkElement)e.NewValue;
    }
  }
}
