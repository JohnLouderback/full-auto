using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GenericModLauncher.ViewModels;

namespace GenericModLauncher;

/// <summary>
///   Provides a behavior to track focus on ListBoxItems and update the selected item in the view
///   model.
/// </summary>
public static class FocusTrackingBehavior {
  public static readonly DependencyProperty TrackFocusProperty =
    DependencyProperty.RegisterAttached(
      "TrackFocus",
      typeof(bool),
      typeof(FocusTrackingBehavior),
      new PropertyMetadata(defaultValue: false, OnTrackFocusChanged)
    );


  public static bool GetTrackFocus(DependencyObject element) {
    return (bool)element.GetValue(TrackFocusProperty);
  }


  public static void SetTrackFocus(DependencyObject element, bool value) {
    element.SetValue(TrackFocusProperty, value);
  }


  private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject {
    while (current != null &&
           current is not T) {
      current = VisualTreeHelper.GetParent(current);
    }

    return current as T;
  }


  private static void OnTrackFocusChanged(
    DependencyObject d,
    DependencyPropertyChangedEventArgs e
  ) {
    if (d is ListBoxItem item &&
        e.NewValue is true) {
      item.GotFocus += (_, _) => {
        if (item.DataContext is ItemViewModel vm &&
            FindAncestor<ListBox>(item)?.DataContext is ILauncherViewModel launcher) {
          launcher.SelectedItem = vm;
        }
      };
    }
  }
}
