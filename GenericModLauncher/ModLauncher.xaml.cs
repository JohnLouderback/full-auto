using System.Windows;
using GenericModLauncher.ViewModels;

namespace GenericModLauncher;

public partial class ModLauncher : Window {
  private readonly ILauncherViewModel viewModel;


  public ModLauncher(ILauncherViewModel viewModel) {
    this.viewModel = viewModel;
    InitializeComponent();
    DataContext = this.viewModel;
  }
}
