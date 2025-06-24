using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GenericModLauncher.ViewModels;

namespace GenericModLauncher;

public partial class ModLauncher : Window {
  private readonly ILauncherViewModel viewModel;


  public ModLauncher(ILauncherViewModel viewModel) {
    this.viewModel = viewModel;
    InitializeComponent();
    DataContext = this.viewModel;
  }


  private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject {
    for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
      var child = VisualTreeHelper.GetChild(parent, i);
      if (child is T typedChild) {
        return typedChild;
      }

      var result = FindVisualChild<T>(child);
      if (result != null) {
        return result;
      }
    }

    return null;
  }


  private void OnItemActivated(object sender, MouseButtonEventArgs e) {
    if (GameList.SelectedItem is ItemViewModel item) {
      item.OnLaunch();
    }
  }


  private void OnItemKeyDown(object sender, KeyEventArgs e) {
    if (e.Key is Key.Enter or Key.Space &&
        GameList.SelectedItem is ItemViewModel item) {
      item.OnLaunch();
      e.Handled = true;
    }
  }


  private void RootElement_Loaded(object sender, RoutedEventArgs e) {
    var listBox = FindVisualChild<ListBox>(this);
    if (listBox != null &&
        listBox.Items.Count > 0) {
      var firstItem = listBox.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
      if (firstItem == null) {
        // Ensure the container is generated
        listBox.UpdateLayout();
        listBox.ScrollIntoView(listBox.Items[0]);
        firstItem = listBox.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
      }

      firstItem?.Focus();
    }
  }
}
